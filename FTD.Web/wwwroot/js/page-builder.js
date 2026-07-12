/* ═══════════════════════════════════════════════════════════════════════════
   PAGE BUILDER — unified visual editor for custom pages
   • Quill.js rich text editors (with inline image uploads)
   • Native drag & drop section reordering (AJAX persisted)
   • AJAX save / delete / toggle-visibility per section
   • Dynamic row adders (cards / FAQ / gallery / testimonials)
   • AJAX image uploads with live preview
   ═══════════════════════════════════════════════════════════════════════════ */
(function () {
  'use strict';

  var UPLOAD_URL = '/Admin/AdminPageSections/UploadImage';
  var REORDER_URL = '/Admin/AdminPageSections/UpdateSortOrders';

  function csrf() {
    var el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
  }

  function toast(msg, isErr) {
    var t = document.getElementById('pbToast');
    if (!t) return;
    t.textContent = msg;
    t.classList.toggle('err', !!isErr);
    t.classList.add('show');
    clearTimeout(t._h);
    t._h = setTimeout(function () { t.classList.remove('show'); }, 2600);
  }

  /* ── Image upload (shared) ─────────────────────────────────────────────── */
  function uploadImage(file) {
    var fd = new FormData();
    fd.append('file', file);
    fd.append('__RequestVerificationToken', csrf());
    return fetch(UPLOAD_URL, { method: 'POST', body: fd })
      .then(function (r) {
        if (!r.ok) return r.json().then(function (j) { throw new Error(j.error || 'فشل الرفع'); });
        return r.json();
      });
  }

  /* ── Quill editors ─────────────────────────────────────────────────────── */
  var TOOLBAR = [
    [{ header: [2, 3, false] }],
    ['bold', 'italic', 'underline', 'strike'],
    [{ color: [] }, { background: [] }],
    [{ list: 'ordered' }, { list: 'bullet' }],
    [{ align: [] }, { direction: 'rtl' }],
    ['link', 'image'],
    ['clean']
  ];

  function initQuill(holder) {
    if (holder._quill) return;
    var targetId = holder.getAttribute('data-target');
    var ta = document.getElementById(targetId);
    if (!ta) return;

    var q = new Quill(holder, {
      theme: 'snow',
      modules: {
        toolbar: {
          container: TOOLBAR,
          handlers: {
            image: function () {
              var input = document.createElement('input');
              input.type = 'file';
              input.accept = 'image/*';
              input.onchange = function () {
                var f = input.files && input.files[0];
                if (!f) return;
                toast('جارٍ رفع الصورة…');
                uploadImage(f).then(function (res) {
                  var range = q.getSelection(true);
                  q.insertEmbed(range ? range.index : 0, 'image', res.url);
                  toast('تم رفع الصورة ✓');
                }).catch(function (e) { toast(e.message, true); });
              };
              input.click();
            }
          }
        }
      }
    });

    var dir = holder.getAttribute('data-dir') || 'rtl';
    q.root.setAttribute('dir', dir);
    if (ta.value) q.clipboard.dangerouslyPasteHTML(ta.value);
    q.on('text-change', function () {
      ta.value = q.root.innerHTML === '<p><br></p>' ? '' : q.root.innerHTML;
    });
    holder._quill = q;
    ta._quill = q;
  }

  function initQuillIn(scope) {
    (scope || document).querySelectorAll('.quill-holder').forEach(initQuill);
  }

  /* ── AJAX image upload inputs + preview ───────────────────────────────── */
  document.addEventListener('change', function (e) {
    var input = e.target.closest('.ajax-img-upload');
    if (!input) return;
    var f = input.files && input.files[0];
    if (!f) return;
    var targetId = input.getAttribute('data-target');
    var hidden = document.getElementById(targetId);
    var preview = document.querySelector('.img-preview[data-preview="' + targetId + '"]');

    toast('جارٍ رفع الصورة…');
    uploadImage(f).then(function (res) {
      if (hidden) hidden.value = res.url;
      if (preview) {
        preview.style.backgroundImage = "url('" + res.url + "')";
        preview.innerHTML = '';
      }
      var clearBtn = document.querySelector('.clear-img[data-target="' + targetId + '"]');
      if (clearBtn) clearBtn.style.display = '';
      toast('تم رفع الصورة ✓');
    }).catch(function (err) {
      toast(err.message, true);
      input.value = '';
    });
  });

  document.addEventListener('click', function (e) {
    var btn = e.target.closest('.clear-img');
    if (!btn) return;
    var targetId = btn.getAttribute('data-target');
    var hidden = document.getElementById(targetId);
    var preview = document.querySelector('.img-preview[data-preview="' + targetId + '"]');
    if (hidden) hidden.value = '';
    if (preview) { preview.style.backgroundImage = ''; preview.innerHTML = ''; }
    btn.style.display = 'none';
  });

  /* ── Section accordion / add / delete / visibility ────────────────────── */
  window.pbToggleSection = function (e, id) {
    var sec = document.getElementById('pbsec_' + id);
    if (!sec) return;
    sec.classList.toggle('open');
    if (sec.classList.contains('open')) initQuillIn(sec);
  };

  window.pbAddSection = function (type) {
    var f = document.getElementById('pbAddForm');
    document.getElementById('pbAddType').value = type;
    f.submit();
  };

  window.pbDeleteSection = function (id) {
    if (!confirm('حذف هذا القسم نهائياً؟')) return;
    var fd = new FormData();
    fd.append('id', id);
    fd.append('__RequestVerificationToken', csrf());
    fetch('/Admin/AdminPageSections/DeleteSection', {
      method: 'POST', body: fd,
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function (r) {
      if (!r.ok) throw new Error();
      var el = document.getElementById('pbsec_' + id);
      if (el) { el.style.transition = 'all .3s'; el.style.opacity = '0'; el.style.transform = 'scale(.97)'; setTimeout(function () { el.remove(); }, 280); }
      toast('تم حذف القسم ✓');
    }).catch(function () { toast('حصل خطأ أثناء الحذف', true); });
  };

  window.pbToggleVisible = function (id, btn) {
    var fd = new FormData();
    fd.append('id', id);
    fd.append('__RequestVerificationToken', csrf());
    fetch('/Admin/AdminPageSections/ToggleSectionVisible', {
      method: 'POST', body: fd,
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
      .then(function (j) {
        var visible = j.visible;
        btn.textContent = visible ? '👁' : '🚫';
        btn.classList.toggle('a-btn-success', visible);
        btn.classList.toggle('a-btn-danger', !visible);
        var sec = document.getElementById('pbsec_' + id);
        var badge = sec.querySelector('.pb-hidden-badge');
        if (!visible && !badge) {
          var b = document.createElement('span');
          b.className = 'pb-hidden-badge'; b.textContent = 'مخفي';
          sec.querySelector('.pb-sec-head > div').appendChild(b);
        } else if (visible && badge) badge.remove();
        toast(visible ? 'القسم ظاهر الآن ✓' : 'تم إخفاء القسم');
      }).catch(function () { toast('حصل خطأ', true); });
  };

  /* ── AJAX save per section (stay in place, no reload) ─────────────────── */
  document.addEventListener('submit', function (e) {
    var form = e.target.closest('.pb-section-form');
    if (!form) return;
    e.preventDefault();

    // sync open Quill editors into their textareas
    form.querySelectorAll('.quill-holder').forEach(function (h) {
      if (h._quill) {
        var ta = document.getElementById(h.getAttribute('data-target'));
        if (ta) ta.value = h._quill.root.innerHTML === '<p><br></p>' ? '' : h._quill.root.innerHTML;
      }
    });

    var btn = form.querySelector('button[type="submit"]');
    var ok = form.querySelector('.pb-save-ok');
    if (btn) { btn.disabled = true; btn.textContent = '⏳ جارٍ الحفظ…'; }

    fetch(form.action, {
      method: 'POST',
      body: new FormData(form),
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
      .then(function () {
        if (ok) { ok.classList.add('show'); setTimeout(function () { ok.classList.remove('show'); }, 2200); }
        toast('تم حفظ القسم ✓');
      })
      .catch(function () { toast('حصل خطأ أثناء الحفظ', true); })
      .finally(function () {
        if (btn) { btn.disabled = false; btn.textContent = '💾 حفظ هذا القسم'; }
      });
  });

  /* ── Drag & Drop reorder (native HTML5, grip-handle driven) ───────────── */
  function initDragDrop() {
    var list = document.getElementById('pbSectionList');
    if (!list) return;
    var draggedEl = null;

    list.addEventListener('dragstart', function (e) {
      var grip = e.target.closest('.pb-grip');
      if (!grip) { e.preventDefault(); return; }
      draggedEl = grip.closest('.pb-section');
      draggedEl.classList.add('dragging');
      e.dataTransfer.effectAllowed = 'move';
      try { e.dataTransfer.setData('text/plain', draggedEl.dataset.sectionId); } catch (_) {}
      // drag image = whole section card
      if (e.dataTransfer.setDragImage) e.dataTransfer.setDragImage(draggedEl, 20, 20);
    });

    list.addEventListener('dragover', function (e) {
      if (!draggedEl) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      var over = e.target.closest('.pb-section');
      if (!over || over === draggedEl) return;
      var rect = over.getBoundingClientRect();
      var before = (e.clientY - rect.top) < rect.height / 2;
      if (before) list.insertBefore(draggedEl, over);
      else list.insertBefore(draggedEl, over.nextSibling);
    });

    list.addEventListener('drop', function (e) { e.preventDefault(); });

    list.addEventListener('dragend', function () {
      if (!draggedEl) return;
      draggedEl.classList.remove('dragging');
      draggedEl = null;
      persistOrder(list);
    });
  }

  function persistOrder(list) {
    var ids = Array.prototype.map.call(
      list.querySelectorAll('.pb-section'),
      function (el) { return parseInt(el.dataset.sectionId, 10); }
    );
    fetch(REORDER_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': csrf(),
        'X-Requested-With': 'XMLHttpRequest'
      },
      body: JSON.stringify(ids)
    }).then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
      .then(function () { toast('تم حفظ الترتيب الجديد ✓'); })
      .catch(function () { toast('فشل حفظ الترتيب — أعد المحاولة', true); });
  }

  /* ── Dynamic row adders ───────────────────────────────────────────────── */
  var rowCounters = {};
  function nextUid(sid) {
    rowCounters[sid] = (rowCounters[sid] || 1000) + 1;
    return sid + '_n' + rowCounters[sid];
  }

  document.addEventListener('click', function (e) {
    var addCard = e.target.closest('.add-card-row');
    var addFaq = e.target.closest('.add-faq-row');
    var addGal = e.target.closest('.add-gal-row');
    var addTst = e.target.closest('.add-tst-row');

    if (addCard) addRow('cardRows-' + addCard.dataset.sid, cardRowHtml(addCard.dataset.sid));
    if (addFaq) addRow('faqRows-' + addFaq.dataset.sid, faqRowHtml(addFaq.dataset.sid));
    if (addGal) addRow('galRows-' + addGal.dataset.sid, galRowHtml(addGal.dataset.sid));
    if (addTst) addRow('tstRows-' + addTst.dataset.sid, tstRowHtml(addTst.dataset.sid));
  });

  function addRow(containerId, html) {
    var c = document.getElementById(containerId);
    if (!c) return;
    var tmp = document.createElement('div');
    tmp.innerHTML = html.trim();
    c.appendChild(tmp.firstChild);
  }

  var ROW_BOX = 'background:var(--a-bg);border-radius:10px;padding:1rem;margin-bottom:.8rem;border:1px solid var(--a-border)';
  var DEL_BTN = 'style="background:none;border:none;color:var(--a-text3);cursor:pointer"';

  function cardRowHtml(sid) {
    var uid = nextUid(sid);
    return '<div class="card-row builder-row" style="' + ROW_BOX + '">' +
      '<div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:.8rem"><span style="font-size:11px;font-weight:700;color:var(--a-text3)">🃏 كارد</span><button type="button" onclick="this.closest(\'.card-row\').remove()" ' + DEL_BTN + '>✕</button></div>' +
      '<div style="display:grid;grid-template-columns:110px 1fr 1fr;gap:.8rem;margin-bottom:.8rem">' +
      '<div class="a-form-group"><label>Emoji/Icon</label><input type="text" name="card_icon_' + sid + '" placeholder="✓ أو bi-truck"></div>' +
      '<div class="a-form-group"><label>العنوان بالعربي</label><input type="text" name="card_title_ar_' + sid + '" placeholder="مميزة"></div>' +
      '<div class="a-form-group"><label>Title EN</label><input type="text" name="card_title_en_' + sid + '" placeholder="Feature" dir="ltr"></div></div>' +
      '<div style="display:grid;grid-template-columns:1fr 1fr;gap:.8rem;margin-bottom:.8rem">' +
      '<div class="a-form-group"><label>الوصف AR</label><textarea name="card_body_ar_' + sid + '" rows="2"></textarea></div>' +
      '<div class="a-form-group"><label>Desc EN</label><textarea name="card_body_en_' + sid + '" rows="2" dir="ltr"></textarea></div></div>' +
      '<div style="display:flex;gap:.8rem;align-items:center">' +
      '<div class="img-preview" data-preview="card_img_' + uid + '" style="width:64px;height:48px;border-radius:8px;border:1px solid var(--a-border);background:#fff center/cover no-repeat;flex-shrink:0"></div>' +
      '<input type="hidden" name="card_image_' + sid + '" id="card_img_' + uid + '" value="">' +
      '<input type="file" accept=".png,.jpg,.jpeg,.webp,.svg,.gif" class="ajax-img-upload" data-target="card_img_' + uid + '" style="font-size:12px;flex:1">' +
      '<button type="button" class="a-btn-sm a-btn-danger clear-img" data-target="card_img_' + uid + '" style="display:none">✕</button></div>' +
      '<small style="font-size:10.5px;color:var(--a-text3)">صورة الكارد اختيارية — لو اترفعت بتظهر مكان الأيقونة</small></div>';
  }

  function faqRowHtml(sid) {
    return '<div class="faq-row builder-row" style="' + ROW_BOX + '">' +
      '<div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:.8rem"><span style="font-size:11px;font-weight:700;color:var(--a-text3)">❓ سؤال</span><button type="button" onclick="this.closest(\'.faq-row\').remove()" ' + DEL_BTN + '>✕</button></div>' +
      '<div style="display:grid;grid-template-columns:1fr 1fr;gap:.8rem;margin-bottom:.8rem">' +
      '<div class="a-form-group"><label>السؤال بالعربي</label><input type="text" name="faq_q_ar_' + sid + '" placeholder="السؤال بالعربي"></div>' +
      '<div class="a-form-group"><label>Question EN</label><input type="text" name="faq_q_en_' + sid + '" dir="ltr"></div></div>' +
      '<div style="display:grid;grid-template-columns:1fr 1fr;gap:.8rem">' +
      '<div class="a-form-group"><label>الإجابة بالعربي</label><textarea name="faq_a_ar_' + sid + '" rows="2"></textarea></div>' +
      '<div class="a-form-group"><label>Answer EN</label><textarea name="faq_a_en_' + sid + '" rows="2" dir="ltr"></textarea></div></div></div>';
  }

  function galRowHtml(sid) {
    var uid = nextUid(sid);
    return '<div class="gal-row builder-row" style="background:var(--a-bg);border-radius:10px;padding:.8rem;border:1px solid var(--a-border)">' +
      '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:.5rem"><span style="font-size:11px;font-weight:700;color:var(--a-text3)">🖼 صورة</span><button type="button" onclick="this.closest(\'.gal-row\').remove()" ' + DEL_BTN + '>✕</button></div>' +
      '<div class="img-preview" data-preview="gal_img_' + uid + '" style="width:100%;height:110px;border-radius:8px;border:1px solid var(--a-border);background:#fff center/cover no-repeat;margin-bottom:.5rem"></div>' +
      '<input type="hidden" name="gal_url_' + sid + '" id="gal_img_' + uid + '" value="">' +
      '<input type="file" accept=".png,.jpg,.jpeg,.webp,.gif" class="ajax-img-upload" data-target="gal_img_' + uid + '" style="font-size:11px;width:100%;margin-bottom:.5rem">' +
      '<input type="text" name="gal_cap_ar_' + sid + '" placeholder="تعليق بالعربي (اختياري)" style="margin-bottom:.4rem">' +
      '<input type="text" name="gal_cap_en_' + sid + '" placeholder="Caption EN (optional)" dir="ltr"></div>';
  }

  function tstRowHtml(sid) {
    var uid = nextUid(sid);
    return '<div class="tst-row builder-row" style="' + ROW_BOX + '">' +
      '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:.8rem"><span style="font-size:11px;font-weight:700;color:var(--a-text3)">💬 رأي عميل</span><button type="button" onclick="this.closest(\'.tst-row\').remove()" ' + DEL_BTN + '>✕</button></div>' +
      '<div style="display:grid;grid-template-columns:1fr 1fr 120px;gap:.8rem;margin-bottom:.8rem">' +
      '<div class="a-form-group"><label>اسم العميل</label><input type="text" name="tst_name_' + sid + '"></div>' +
      '<div class="a-form-group"><label>الصفة / المدينة</label><input type="text" name="tst_role_' + sid + '"></div>' +
      '<div class="a-form-group"><label>التقييم ⭐</label><select name="tst_rating_' + sid + '" class="a-select"><option value="5">5 نجوم</option><option value="4">4 نجوم</option><option value="3">3 نجوم</option><option value="2">2 نجوم</option><option value="1">1 نجوم</option></select></div></div>' +
      '<div style="display:grid;grid-template-columns:1fr 1fr;gap:.8rem;margin-bottom:.8rem">' +
      '<div class="a-form-group"><label>نص الرأي بالعربي</label><textarea name="tst_text_ar_' + sid + '" rows="2"></textarea></div>' +
      '<div class="a-form-group"><label>Review EN</label><textarea name="tst_text_en_' + sid + '" rows="2" dir="ltr"></textarea></div></div>' +
      '<div style="display:flex;gap:.8rem;align-items:center">' +
      '<div class="img-preview" data-preview="tst_img_' + uid + '" style="width:44px;height:44px;border-radius:50%;border:1px solid var(--a-border);background:#fff center/cover no-repeat;flex-shrink:0"></div>' +
      '<input type="hidden" name="tst_avatar_' + sid + '" id="tst_img_' + uid + '" value="">' +
      '<input type="file" accept=".png,.jpg,.jpeg,.webp" class="ajax-img-upload" data-target="tst_img_' + uid + '" style="font-size:11px;flex:1">' +
      '<button type="button" class="a-btn-sm a-btn-danger clear-img" data-target="tst_img_' + uid + '" style="display:none">✕</button></div></div>';
  }

  /* ── Boot ─────────────────────────────────────────────────────────────── */
  document.addEventListener('DOMContentLoaded', function () {
    initDragDrop();
    // Open first section by default for quicker editing
    var first = document.querySelector('#pbSectionList .pb-section');
    if (first) { first.classList.add('open'); initQuillIn(first); }
  });

  // Lazy-init Quill inside any panel that gets expanded (incl. legacy sidebar panel)
  document.addEventListener('click', function (e) {
    var head = e.target.closest('.pb-sec-head');
    if (!head) return;
    setTimeout(function () {
      var container = head.parentElement;
      if (container && container.classList.contains('open')) initQuillIn(container);
    }, 30);
  });

  // Before submitting the sidebar meta form, sync legacy Quill editors
  document.addEventListener('submit', function (e) {
    if (e.target.id !== 'pageMetaForm') return;
    e.target.querySelectorAll('.quill-holder').forEach(function (h) {
      if (h._quill) {
        var ta = document.getElementById(h.getAttribute('data-target'));
        if (ta) ta.value = h._quill.root.innerHTML === '<p><br></p>' ? '' : h._quill.root.innerHTML;
      }
    });
  });
})();
