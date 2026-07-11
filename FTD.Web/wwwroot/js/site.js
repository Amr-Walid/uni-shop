// ── CART BADGE ────────────────────────────────────────────────────────────────
// Single source of truth for showing/updating the navbar cart badge. Both the
// quick add-to-cart flow and the periodic count refresh route through this so
// the badge's visibility can never drift out of sync with its text again —
// previously the badge had a hardcoded inline `display:none` that neither
// caller ever removed, so it stayed invisible even once items were added.
function setCartBadge(count) {
    var badge = document.getElementById('cartCount');
    if (!badge) return;
    var n = Number(count) || 0;
    badge.textContent = n;
    badge.style.display = n > 0 ? 'grid' : 'none';
}

// ── ADD TO CART (quick, no redirect) ─────────────────────────────────────────
function addToCartQuick(productId, btn) {
    var token = document.querySelector('input[name=__RequestVerificationToken]');
    var formData = new FormData();
    formData.append('productId', productId);
    formData.append('qty', 1);
    if (token) formData.append('__RequestVerificationToken', token.value);

    fetch('/Cart/Add', {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        body: formData
    })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (data && data.count !== undefined) setCartBadge(data.count);
            // Flash button success state briefly (uses design-token color,
            // not a hardcoded legacy hex).
            if (btn) {
                var prevHtml = btn.innerHTML;
                btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 16 16" fill="none"><path d="M3 8l3.5 3.5L13 5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
                btn.style.background = 'var(--success)';
                btn.style.borderColor = 'var(--success)';
                setTimeout(function () {
                    btn.innerHTML = prevHtml;
                    btn.style.background = '';
                    btn.style.borderColor = '';
                }, 1200);
            }
        })
        .catch(function () { });
}

// ── LANG ─────────────────────────────────────────────────────────────────────
function applyLangVisibility() {
    var en = document.body.classList.contains('en');
    document.querySelectorAll('[data-ar]').forEach(function (el) {
        el.style.display = en ? 'none' : (el.tagName === 'SPAN' ? 'inline' : 'block');
    });
    document.querySelectorAll('[data-en]').forEach(function (el) {
        el.style.display = en ? (el.tagName === 'SPAN' ? 'inline' : 'block') : 'none';
    });
    // Swap placeholder text on inputs that declare bilingual placeholders
    // (e.g. the search box) — this was previously hardcoded to Arabic only
    // and never updated when the language was toggled.
    document.querySelectorAll('[data-ph-ar][data-ph-en]').forEach(function (el) {
        el.setAttribute('placeholder', en ? el.getAttribute('data-ph-en') : el.getAttribute('data-ph-ar'));
    });
}

function getCookie(name) {
    var match = document.cookie.match('(?:^|; )' + name + '=([^;]*)');
    return match ? decodeURIComponent(match[1]) : null;
}

function setCookie(name, value) {
    document.cookie = name + '=' + encodeURIComponent(value) + '; path=/; max-age=31536000; SameSite=Lax';
}

function initLang() {
    // Cookie is the SSR-known preference (already applied server-side by _Layout.cshtml
    // on first byte); localStorage is kept as a fallback/legacy source. Reconcile both
    // so a stale localStorage value never fights the server-rendered state, and so a
    // value from either source is persisted to the other for next time.
    var cookieLang = getCookie('ftd_lang');
    var saved = cookieLang || localStorage.getItem('ftd_lang');
    if (saved === 'en') {
        document.body.classList.add('en');
        document.getElementById('htmlRoot')?.setAttribute('lang', 'en');
        document.getElementById('htmlRoot')?.setAttribute('dir', 'ltr');
    }
    if (saved) {
        localStorage.setItem('ftd_lang', saved);
        setCookie('ftd_lang', saved);
    }
    applyLangVisibility();
}

function toggleLang() {
    document.body.classList.toggle('en');
    var en = document.body.classList.contains('en');
    var root = document.getElementById('htmlRoot');
    if (root) {
        root.setAttribute('lang', en ? 'en' : 'ar');
        root.setAttribute('dir', en ? 'ltr' : 'rtl');
    }
    var lang = en ? 'en' : 'ar';
    localStorage.setItem('ftd_lang', lang);
    setCookie('ftd_lang', lang);
    applyLangVisibility();
}


// ── SEARCH OVERLAY ────────────────────────────────────────────────────────────
function openSearch() {
    var ov = document.getElementById('searchOverlay');
    if (!ov) return;
    ov.classList.add('open');
    setTimeout(function () {
        var inp = document.getElementById('searchInput');
        if (inp) inp.focus();
    }, 100);
}

function closeSearch() {
    var ov = document.getElementById('searchOverlay');
    if (ov) ov.classList.remove('open');
}

function doSearch(q) {
    document.getElementById('searchInput').value = q;
    triggerSearch(q);
}

// Live search AJAX
var searchTimer;
function triggerSearch(q) {
    clearTimeout(searchTimer);
    var resultsDiv = document.getElementById('searchResults');
    var tagsDiv = document.getElementById('searchTags');
    if (!q || q.trim().length < 2) {
        if (resultsDiv) resultsDiv.style.display = 'none';
        if (tagsDiv) tagsDiv.style.display = 'flex';
        return;
    }
    if (tagsDiv) tagsDiv.style.display = 'none';

    searchTimer = setTimeout(async function () {
        try {
            var res = await fetch('/Products/Search?q=' + encodeURIComponent(q));
            var data = await res.json();
            var en = document.body.classList.contains('en');

            if (!data.results || data.results.length === 0) {
                var escapedQ = q.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
                resultsDiv.innerHTML = '<div class="search-no-results">'
                    + '<span data-ar>لا توجد نتائج لـ "' + escapedQ + '"</span>'
                    + '<span data-en>No results for "' + escapedQ + '"</span>'
                    + '</div>';
                if (typeof applyLangVisibility === 'function') applyLangVisibility();
            } else {
                resultsDiv.innerHTML = data.results.map(function (p) {
                    return '<a href="' + p.url + '" class="search-result-item" onclick="closeSearch()">'
                        + '<div class="search-result-emoji">' + (p.emoji || '📦') + '</div>'
                        + '<div>'
                        + '<div class="search-result-brand">' + (p.brand || '') + '</div>'
                        + '<div class="search-result-name">' + (en ? p.nameEn : p.nameAr) + '</div>'
                        + '<div class="search-result-price">' + p.price + '</div>'
                        + '</div></a>';
                }).join('');
            }
            resultsDiv.style.display = 'block';
        } catch (e) {
            console.error('Search error:', e);
        }
    }, 300);
}

// ── CART COUNT (from session via AJAX) ────────────────────────────────────────
function updateCartCount() {
    fetch('/Cart/Count')
        .then(function (r) { return r.json(); })
        .then(function (d) { setCartBadge(d.count); })
        .catch(function () { });
}

// ── INIT ─────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {

    initLang();
    updateCartCount();

    // Close mobile menu when any of its links are clicked
    document.querySelectorAll('.mobile-menu-link').forEach(function (a) {
        a.addEventListener('click', function () {
            var menu = document.getElementById('mobile-menu');
            if (menu) menu.classList.remove('open');
        });
    });

    // Search overlay
    var ov = document.getElementById('searchOverlay');
    if (ov) {
        ov.addEventListener('click', function (e) {
            if (e.target === this) closeSearch();
        });
    }

    var inp = document.getElementById('searchInput');
    if (inp) {
        inp.addEventListener('input', function () { triggerSearch(this.value); });
        inp.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                window.location.href = '/Products?q=' + encodeURIComponent(this.value);
                closeSearch();
            }
        });
    }

    // Keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeSearch();
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') { e.preventDefault(); openSearch(); }
    });

    // Hash-link smooth scroll is handled by the navbar component script

    // Note: hash-on-load scroll handled natively by the browser
});
