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
            // Update cart count
            var badge = document.getElementById('cartCount');
            if (badge && data.count !== undefined) badge.textContent = data.count;
            // Flash button green briefly
            if (btn) {
                btn.textContent = '✓';
                btn.style.background = '#00C48C';
                btn.style.color = 'white';
                btn.style.borderColor = '#00C48C';
                setTimeout(function () {
                    btn.textContent = '🛒';
                    btn.style.background = '';
                    btn.style.color = '';
                    btn.style.borderColor = '';
                }, 1200);
            }
        })
        .catch(function () { });
}

// ── MOBILE NAV ───────────────────────────────────────────────────────────────
function toggleMobileNav() {
    var nav = document.getElementById('mobileNav');
    var btn = document.getElementById('hamburger');
    if (!nav || !btn) return;
    nav.classList.toggle('open');
    btn.classList.toggle('open');
}

// Close mobile nav on outside click
document.addEventListener('click', function (e) {
    var nav = document.getElementById('mobileNav');
    var btn = document.getElementById('hamburger');
    if (!nav || !btn) return;
    if (nav.classList.contains('open') && !nav.contains(e.target) && !btn.contains(e.target)) {
        nav.classList.remove('open');
        btn.classList.remove('open');
    }
});

// ── LANG ─────────────────────────────────────────────────────────────────────
function applyLangVisibility() {
    var en = document.body.classList.contains('en');
    document.querySelectorAll('[data-ar]').forEach(function (el) {
        el.style.display = en ? 'none' : (el.tagName === 'SPAN' ? 'inline' : 'block');
    });
    document.querySelectorAll('[data-en]').forEach(function (el) {
        el.style.display = en ? (el.tagName === 'SPAN' ? 'inline' : 'block') : 'none';
    });
}

function initLang() {
    var saved = localStorage.getItem('ftd_lang');
    if (saved === 'en') {
        document.body.classList.add('en');
        document.getElementById('htmlRoot')?.setAttribute('lang', 'en');
        document.getElementById('htmlRoot')?.setAttribute('dir', 'ltr');
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
    localStorage.setItem('ftd_lang', en ? 'en' : 'ar');
    applyLangVisibility();
}

// ── NAV SCROLL ────────────────────────────────────────────────────────────────
window.addEventListener('scroll', function () {
    var nav = document.getElementById('mainNav');
    if (nav) nav.classList.toggle('scrolled', window.scrollY > 20);
});

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
                resultsDiv.innerHTML = '<div class="search-no-results">لا توجد نتائج / No results for "' + q + '"</div>';
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
        .then(function (d) {
            var el = document.getElementById('cartCount');
            if (el) el.textContent = d.count || 0;
        })
        .catch(function () { });
}

// ── MEGA PANEL (click-based, no hover) ───────────────────────────────────────
function initDropdowns() {
    var btn = document.getElementById('productsMegaBtn');
    var trigger = document.getElementById('productsMegaTrigger');
    var panel = document.getElementById('productsMegaPanel');
    if (!btn || !trigger || !panel) return;

    // Click trigger → toggle panel
    trigger.addEventListener('click', function (e) {
        e.preventDefault();
        btn.classList.toggle('open');
    });

    // Click inside panel link → navigate + close
    panel.querySelectorAll('a').forEach(function (link) {
        link.addEventListener('click', function () {
            btn.classList.remove('open');
        });
    });

    // Click anywhere outside → close
    document.addEventListener('click', function (e) {
        if (!btn.contains(e.target)) {
            btn.classList.remove('open');
        }
    });
}

// ── INIT ─────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {

    // Dropdowns
    initDropdowns();

    // Hamburger button
    var hamburgerBtn = document.getElementById('hamburger');
    if (hamburgerBtn) {
        hamburgerBtn.addEventListener('click', function () {
            toggleMobileNav();
        });
    }

    initLang();
    updateCartCount();

    // Close mobile nav on link click
    document.querySelectorAll('.mobile-nav a').forEach(function (a) {
        a.addEventListener('click', function () {
            var nav = document.getElementById('mobileNav');
            var btn = document.getElementById('hamburger');
            if (nav) nav.classList.remove('open');
            if (btn) btn.classList.remove('open');
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

    // Smooth scroll for hash links
    document.querySelectorAll('a[href^="/#"]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            var hash = this.getAttribute('href').substring(1);
            var el = document.querySelector(hash);
            if (el) {
                e.preventDefault();
                el.scrollIntoView({ behavior: 'smooth' });
                window.history.pushState(null, '', hash);
            }
        });
    });

    // If on home page and has hash
    if (location.hash) {
        setTimeout(function () {
            var el = document.querySelector(location.hash);
            if (el) el.scrollIntoView({ behavior: 'smooth' });
        }, 150);
    }
});
