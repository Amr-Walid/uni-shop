// ============================================
// UNI-SHOP — Data + Interactions
// ============================================

const PRODUCTS = [
  {
    id: 't30',
    slug: 't30-ultra-tablet',
    name: 'T30 Ultra Tablet',
    brand: 'DOOGEE',
    category: 'tablets',
    price: 8999,
    badge: { label: 'NEW', kind: 'new' },
    tagline: '11" 2K Display · 8300mAh',
    desc: '11" 2K display, 8300mAh battery, Helio G99 processor. Built for professionals who demand speed and stamina.',
    specs: [
      ['Display', '11" 2K IPS'],
      ['Battery', '8,300 mAh'],
      ['Processor', 'Helio G99'],
      ['Memory', '8GB + 256GB'],
      ['Camera', '20MP + 13MP'],
      ['OS', 'Android 13'],
    ],
    color: '#00b4e6',
    icon: 'tablet',
  },
  {
    id: 'f40',
    slug: 'f40-portable-fan',
    name: 'F40 Portable Fan',
    brand: 'JisuLife',
    category: 'fans',
    price: 1299,
    badge: { label: 'HOT', kind: 'hot' },
    tagline: '10000mAh · 24h Runtime',
    desc: '10,000mAh battery, up to 24 hours runtime, 4 speed settings. Portable comfort engineered for endurance.',
    specs: [
      ['Battery', '10,000 mAh'],
      ['Runtime', 'Up to 24 hours'],
      ['Speeds', '4 levels'],
      ['Weight', '412 g'],
      ['Charging', 'USB-C 5V/2A'],
      ['Warranty', '18 months'],
    ],
    color: '#00d4ff',
    icon: 'fan',
  },
  {
    id: 'webcam4k',
    slug: '4k-pro-webcam',
    name: '4K Pro Webcam',
    brand: 'Dreame',
    category: 'webcams',
    price: 2499,
    badge: { label: '4K', kind: '4k' },
    tagline: 'Cinematic Broadcasting',
    desc: '4K resolution, 90° wide-angle lens, built-in noise-cancelling microphone. Studio quality on your desk.',
    specs: [
      ['Resolution', '4K @ 30fps'],
      ['Field of View', '90°'],
      ['Autofocus', 'PDAF'],
      ['Microphone', 'Dual noise-cancel'],
      ['Connection', 'USB-C 3.0'],
      ['Mount', 'Universal clip + tripod'],
    ],
    color: '#7bd2f4',
    icon: 'camera',
  },
  {
    id: 'r20',
    slug: 'r20-rugged-tablet',
    name: 'R20 Rugged Tablet',
    brand: 'DOOGEE',
    category: 'tablets',
    price: 12500,
    badge: { label: 'RUGGED', kind: 'rugged' },
    tagline: 'IP68 · 10.4" · 15600mAh',
    desc: 'Rugged tablet resistant to water, dust and shocks per IP68 standard. Ideal for industrial use and outdoor adventures.',
    specs: [
      ['Display', '10.4" 2K'],
      ['Battery', '15,600 mAh'],
      ['Rating', 'IP68 / IP69K'],
      ['Drop Test', 'MIL-STD-810H'],
      ['Memory', '12GB + 256GB'],
      ['Processor', 'Helio G99'],
    ],
    color: '#0f172a',
    icon: 'shield',
  },
  {
    id: 'f8',
    slug: 'f8-pro-neck-fan',
    name: 'F8 Pro Neck Fan',
    brand: 'JisuLife',
    category: 'fans',
    price: 899,
    badge: null,
    tagline: 'Ultra-light Wearable',
    desc: 'Wearable neck design, 135g ultra-light, USB-C charging. Effortless cooling that moves with you.',
    specs: [
      ['Weight', '135 g'],
      ['Battery', '5,000 mAh'],
      ['Runtime', 'Up to 16 hours'],
      ['Speeds', '3 levels'],
      ['Charging', 'USB-C'],
      ['Fit', 'Adjustable neckband'],
    ],
    color: '#00d4ff',
    icon: 'wind',
  },
  {
    id: 'hdstream',
    slug: 'hd-stream-webcam',
    name: 'HD Stream Webcam',
    brand: 'Dreame',
    category: 'webcams',
    price: 1199,
    badge: null,
    tagline: '1080p · Auto Light Correction',
    desc: '1080p Full HD, auto light correction, plug & play. Reliable clarity for calls and live streams.',
    specs: [
      ['Resolution', '1080p @ 60fps'],
      ['Field of View', '78°'],
      ['Autofocus', 'Yes'],
      ['Microphone', 'Stereo'],
      ['Connection', 'USB 2.0'],
      ['Compatibility', 'Windows / macOS'],
    ],
    color: '#3f484d',
    icon: 'video',
  },
];

const BRANDS = {
  DOOGEE: {
    name: 'DOOGEE',
    tagline: 'Rugged phones and tablets, engineered for extremes.',
    accent: '#3b5bdb',
  },
  JisuLife: {
    name: 'JisuLife',
    tagline: 'Portable comfort. Smart devices for daily life.',
    accent: '#00d4ff',
  },
  Dreame: {
    name: 'Dreame',
    tagline: 'Precision optics for creators and communicators.',
    accent: '#7c3aed',
  },
};

// ============================================
// ICONS — Abstract product marks (SVG)
// ============================================
function productMark(product, size = 180) {
  const c = product.color;
  const maps = {
    tablet: `
      <defs>
        <linearGradient id="g-${product.id}" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stop-color="${c}" stop-opacity="0.9"/>
          <stop offset="1" stop-color="#0f172a" stop-opacity="0.9"/>
        </linearGradient>
      </defs>
      <rect x="45" y="20" width="90" height="140" rx="14" fill="url(#g-${product.id})" stroke="#0f172a" stroke-width="2"/>
      <rect x="55" y="34" width="70" height="105" rx="4" fill="#0f172a"/>
      <rect x="60" y="40" width="60" height="8" rx="2" fill="${c}" opacity="0.6"/>
      <rect x="60" y="54" width="42" height="4" rx="2" fill="${c}" opacity="0.4"/>
      <rect x="60" y="64" width="52" height="4" rx="2" fill="${c}" opacity="0.4"/>
      <rect x="60" y="80" width="60" height="30" rx="4" fill="${c}" opacity="0.2"/>
      <circle cx="90" cy="152" r="3" fill="${c}"/>
    `,
    fan: `
      <defs>
        <radialGradient id="g-${product.id}" cx="0.5" cy="0.5" r="0.5">
          <stop offset="0" stop-color="${c}" stop-opacity="1"/>
          <stop offset="1" stop-color="#007a99" stop-opacity="0.4"/>
        </radialGradient>
      </defs>
      <circle cx="90" cy="90" r="70" fill="url(#g-${product.id})" opacity="0.15"/>
      <circle cx="90" cy="90" r="58" fill="none" stroke="${c}" stroke-width="1.5" opacity="0.4"/>
      <g transform="translate(90,90)">
        <path d="M 0,-50 Q 20,-30 15,-10 Q 0,-5 0,0 Z" fill="${c}" opacity="0.85"/>
        <path d="M 0,-50 Q 20,-30 15,-10 Q 0,-5 0,0 Z" fill="${c}" opacity="0.85" transform="rotate(120)"/>
        <path d="M 0,-50 Q 20,-30 15,-10 Q 0,-5 0,0 Z" fill="${c}" opacity="0.85" transform="rotate(240)"/>
      </g>
      <circle cx="90" cy="90" r="10" fill="#0f172a"/>
      <circle cx="90" cy="90" r="4" fill="${c}"/>
    `,
    camera: `
      <defs>
        <linearGradient id="g-${product.id}" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stop-color="#334155"/>
          <stop offset="1" stop-color="#0f172a"/>
        </linearGradient>
        <radialGradient id="lens-${product.id}" cx="0.5" cy="0.5" r="0.5">
          <stop offset="0" stop-color="${c}"/>
          <stop offset="0.6" stop-color="#0f172a"/>
          <stop offset="1" stop-color="#000"/>
        </radialGradient>
      </defs>
      <rect x="30" y="55" width="120" height="80" rx="16" fill="url(#g-${product.id})"/>
      <circle cx="90" cy="95" r="30" fill="url(#lens-${product.id})"/>
      <circle cx="90" cy="95" r="20" fill="none" stroke="${c}" stroke-width="1" opacity="0.6"/>
      <circle cx="82" cy="87" r="4" fill="white" opacity="0.7"/>
      <rect x="130" y="62" width="8" height="8" rx="2" fill="${c}"/>
      <rect x="42" y="62" width="20" height="4" rx="2" fill="${c}" opacity="0.5"/>
    `,
    shield: `
      <defs>
        <linearGradient id="g-${product.id}" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stop-color="#334155"/>
          <stop offset="1" stop-color="#0f172a"/>
        </linearGradient>
      </defs>
      <rect x="45" y="15" width="90" height="150" rx="16" fill="url(#g-${product.id})" stroke="#00d4ff" stroke-width="2" opacity="0.95"/>
      <rect x="55" y="30" width="70" height="120" rx="6" fill="#0f172a"/>
      <path d="M 90 50 L 105 60 L 105 85 Q 105 100 90 110 Q 75 100 75 85 L 75 60 Z" fill="none" stroke="#00d4ff" stroke-width="2"/>
      <path d="M 82 78 L 88 84 L 100 70" fill="none" stroke="#00d4ff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      <rect x="65" y="120" width="50" height="4" rx="2" fill="#00d4ff" opacity="0.4"/>
      <rect x="65" y="130" width="35" height="4" rx="2" fill="#00d4ff" opacity="0.3"/>
      <circle cx="90" cy="158" r="3" fill="#00d4ff"/>
    `,
    wind: `
      <defs>
        <linearGradient id="g-${product.id}" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0" stop-color="${c}" stop-opacity="0.9"/>
          <stop offset="1" stop-color="#007a99" stop-opacity="0.6"/>
        </linearGradient>
      </defs>
      <path d="M 40 90 Q 90 40 140 90 Q 90 140 40 90 Z" fill="none" stroke="url(#g-${product.id})" stroke-width="14" stroke-linecap="round" opacity="0.9"/>
      <circle cx="60" cy="90" r="10" fill="${c}"/>
      <circle cx="120" cy="90" r="10" fill="${c}"/>
      <path d="M 55 110 Q 90 130 125 110" fill="none" stroke="${c}" stroke-width="3" stroke-linecap="round" opacity="0.6"/>
      <path d="M 30 70 Q 40 70 45 60" fill="none" stroke="${c}" stroke-width="2" stroke-linecap="round" opacity="0.5"/>
      <path d="M 135 60 Q 145 60 150 70" fill="none" stroke="${c}" stroke-width="2" stroke-linecap="round" opacity="0.5"/>
    `,
    video: `
      <defs>
        <linearGradient id="g-${product.id}" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stop-color="#475569"/>
          <stop offset="1" stop-color="#1e293b"/>
        </linearGradient>
      </defs>
      <rect x="25" y="60" width="110" height="70" rx="10" fill="url(#g-${product.id})"/>
      <path d="M 135 80 L 165 65 L 165 125 L 135 110 Z" fill="url(#g-${product.id})"/>
      <circle cx="55" cy="95" r="14" fill="#0f172a"/>
      <circle cx="55" cy="95" r="8" fill="${c}" opacity="0.4"/>
      <rect x="80" y="80" width="40" height="30" rx="4" fill="#0f172a"/>
      <rect x="86" y="86" width="6" height="6" rx="1" fill="#10b981"/>
      <rect x="96" y="86" width="18" height="3" rx="1" fill="${c}" opacity="0.5"/>
      <rect x="96" y="93" width="14" height="3" rx="1" fill="${c}" opacity="0.4"/>
    `,
  };
  return `<svg viewBox="0 0 180 180" width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">${maps[product.icon] || maps.tablet}</svg>`;
}

// ============================================
// CART — localStorage
// ============================================
const CART_KEY = 'unishop-cart';

function getCart() {
  try {
    return JSON.parse(localStorage.getItem(CART_KEY) || '[]');
  } catch {
    return [];
  }
}
function setCart(cart) {
  localStorage.setItem(CART_KEY, JSON.stringify(cart));
  updateCartBadges();
  window.dispatchEvent(new CustomEvent('cart:change'));
}
function addToCart(id, qty = 1) {
  const cart = getCart();
  const existing = cart.find(i => i.id === id);
  if (existing) existing.qty += qty;
  else cart.push({ id, qty });
  setCart(cart);
  showToast(`Added to cart`);
}
function updateCartItem(id, qty) {
  const cart = getCart();
  const item = cart.find(i => i.id === id);
  if (!item) return;
  if (qty <= 0) {
    setCart(cart.filter(i => i.id !== id));
  } else {
    item.qty = qty;
    setCart(cart);
  }
}
function removeFromCart(id) {
  setCart(getCart().filter(i => i.id !== id));
}
function clearCart() {
  setCart([]);
}
function cartCount() {
  return getCart().reduce((sum, i) => sum + i.qty, 0);
}
function cartTotal() {
  return getCart().reduce((sum, i) => {
    const p = PRODUCTS.find(x => x.id === i.id);
    return sum + (p ? p.price * i.qty : 0);
  }, 0);
}
function updateCartBadges() {
  const count = cartCount();
  document.querySelectorAll('[data-cart-count]').forEach(el => {
    el.textContent = count;
    el.style.display = count > 0 ? 'grid' : 'none';
  });
}

function fmtPrice(n) {
  return 'EGP ' + n.toLocaleString('en-US');
}

// ============================================
// TOAST
// ============================================
function showToast(msg) {
  let t = document.getElementById('toast');
  if (!t) {
    t = document.createElement('div');
    t.id = 'toast';
    t.style.cssText = `
      position: fixed; bottom: 32px; left: 50%; transform: translateX(-50%) translateY(80px);
      background: #0f172a; color: white; padding: 14px 24px; border-radius: 999px;
      font-size: 14px; font-weight: 500; z-index: 1000;
      box-shadow: 0 20px 60px -20px rgba(15,23,42,.4); opacity: 0;
      transition: all .3s cubic-bezier(.4,0,.2,1); pointer-events: none;
      display: inline-flex; align-items: center; gap: 10px; border: 1px solid rgba(0,212,255,.3);
    `;
    document.body.appendChild(t);
  }
  t.innerHTML = `<span style="width:6px;height:6px;border-radius:50%;background:#00d4ff;box-shadow:0 0 8px #00d4ff"></span>${msg}`;
  requestAnimationFrame(() => {
    t.style.opacity = '1';
    t.style.transform = 'translateX(-50%) translateY(0)';
  });
  clearTimeout(t._t);
  t._t = setTimeout(() => {
    t.style.opacity = '0';
    t.style.transform = 'translateX(-50%) translateY(80px)';
  }, 2000);
}

// ============================================
// NAV & FOOTER TEMPLATES
// ============================================
function navHTML(active = 'home') {
  return `
  <div class="nav-wrap">
    <nav class="nav">
      <a href="index.html" class="logo">
        <span class="logo-mark"><span>U</span></span>
        <span>Uni<strong>-Shop</strong></span>
      </a>
      <div class="nav-links">
        <a href="index.html" class="nav-link ${active==='home'?'active':''}">Home</a>
        <a href="index.html#about" class="nav-link ${active==='about'?'active':''}">About</a>
        <a href="products.html" class="nav-link ${active==='products'?'active':''}">
          Products
          <svg width="10" height="10" viewBox="0 0 12 12" fill="none"><path d="M3 4.5L6 7.5L9 4.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </a>
        <a href="index.html#contact" class="nav-link ${active==='contact'?'active':''}">Contact</a>
      </div>
      <div class="nav-actions">
        <button class="search-btn" onclick="showToast('Search coming soon')">
          <svg width="14" height="14" viewBox="0 0 16 16" fill="none"><circle cx="7" cy="7" r="5" stroke="currentColor" stroke-width="1.5"/><path d="M14 14L11 11" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
          <span>Search products</span>
          <kbd>⌘K</kbd>
        </button>
        <a href="cart.html" class="icon-btn primary" aria-label="Cart">
          <svg width="18" height="18" viewBox="0 0 20 20" fill="none"><path d="M5 6h11l-1 8H6L5 6zM5 6L4 3H2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/><circle cx="7" cy="17" r="1.5" fill="currentColor"/><circle cx="14" cy="17" r="1.5" fill="currentColor"/></svg>
          <span class="cart-badge" data-cart-count style="display:none">0</span>
        </a>
        <button class="lang-toggle">EN</button>
        <button class="mobile-menu-btn" onclick="toggleMobileMenu()" aria-label="Menu">
          <svg width="18" height="18" viewBox="0 0 20 20" fill="none"><path d="M3 6h14M3 10h14M3 14h14" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
        </button>
      </div>
    </nav>
    <div class="mobile-menu" id="mobile-menu">
      <a href="index.html" class="mobile-menu-link">Home</a>
      <a href="index.html#about" class="mobile-menu-link">About</a>
      <a href="products.html" class="mobile-menu-link">Products</a>
      <a href="index.html#contact" class="mobile-menu-link">Contact</a>
    </div>
  </div>`;
}

function toggleMobileMenu() {
  const m = document.getElementById('mobile-menu');
  if (m) m.classList.toggle('open');
}

function footerHTML() {
  return `
  <footer class="footer">
    <div class="footer-inner container">
      <div class="footer-grid">
        <div class="footer-brand">
          <a href="index.html" class="logo">
            <span class="logo-mark"><span>U</span></span>
            <span>Uni<strong style="color:var(--accent)">-Shop</strong></span>
          </a>
          <p>Your first destination for the latest technology in Egypt — engineered for professionals, curated for enthusiasts.</p>
          <div class="footer-social">
            <a href="#" aria-label="Facebook"><svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor"><path d="M9 6h2V4H9c-1.7 0-3 1.3-3 3v1H4v2h2v6h2v-6h2l1-2H8V7c0-.6.4-1 1-1z"/></svg></a>
            <a href="#" aria-label="Instagram"><svg width="14" height="14" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="2" width="12" height="12" rx="3"/><circle cx="8" cy="8" r="3"/><circle cx="11.5" cy="4.5" r=".5" fill="currentColor"/></svg></a>
            <a href="#" aria-label="WhatsApp"><svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor"><path d="M8 2C4.7 2 2 4.7 2 8c0 1 .3 2 .8 2.8L2 14l3.3-.8c.8.4 1.7.7 2.7.7 3.3 0 6-2.7 6-6s-2.7-6-6-6zm3.4 8.2c-.1.4-.7.7-1.1.8-.3 0-.7.1-2.2-.5-1.8-.7-3-2.5-3.1-2.6-.1-.1-.7-1-.7-1.9s.5-1.3.6-1.5c.2-.2.4-.2.5-.2h.4c.1 0 .3 0 .5.4l.6 1.4c.1.1.1.3 0 .4l-.2.3c-.1.1-.2.2-.1.4.1.2.5.9 1.1 1.4.8.7 1.4.9 1.6 1 .2.1.3.1.4-.1l.6-.7c.1-.2.3-.1.4-.1l1.3.6c.1.1.2.1.3.2 0 .2 0 .5-.1.7z"/></svg></a>
          </div>
        </div>
        <div>
          <h4>Explore</h4>
          <ul class="footer-links">
            <li><a href="index.html">Home</a></li>
            <li><a href="index.html#about">About</a></li>
            <li><a href="products.html">Products</a></li>
            <li><a href="index.html#contact">Contact</a></li>
          </ul>
        </div>
        <div>
          <h4>Brands</h4>
          <ul class="footer-links">
            <li><a href="products.html?brand=DOOGEE">DOOGEE</a></li>
            <li><a href="products.html?brand=JisuLife">JisuLife</a></li>
            <li><a href="products.html?brand=Dreame">Dreame</a></li>
          </ul>
        </div>
        <div>
          <h4>Support</h4>
          <ul class="footer-links">
            <li><a href="index.html#contact">Contact</a></li>
            <li><a href="#">Warranty</a></li>
            <li><a href="#">Shipping</a></li>
            <li><a href="#">Returns</a></li>
          </ul>
        </div>
      </div>
      <div class="footer-bottom">
        <p>© 2026 Uni-Shop · Alfajr For Trade. All rights reserved.</p>
        <div class="status"><span class="dot"></span>All systems operational · Cairo, EGY</div>
      </div>
    </div>
  </footer>`;
}

// ============================================
// INJECT NAV/FOOTER
// ============================================
function mountShell(activePage = 'home') {
  const navSlot = document.getElementById('nav-slot');
  const footSlot = document.getElementById('footer-slot');
  if (navSlot) navSlot.innerHTML = navHTML(activePage);
  if (footSlot) footSlot.innerHTML = footerHTML();
  updateCartBadges();
}

// ============================================
// REVEAL ON SCROLL
// ============================================
function initReveal() {
  const els = document.querySelectorAll('.reveal');
  if (!('IntersectionObserver' in window)) {
    els.forEach(el => el.classList.add('in'));
    return;
  }
  const io = new IntersectionObserver((entries) => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        e.target.classList.add('in');
        io.unobserve(e.target);
      }
    });
  }, { threshold: 0.05, rootMargin: '0px 0px 200px 0px' });
  els.forEach(el => io.observe(el));
  // Safety: reveal everything after 3s regardless
  setTimeout(() => els.forEach(el => el.classList.add('in')), 3000);
}

// Auto-init
document.addEventListener('DOMContentLoaded', () => {
  updateCartBadges();
  initReveal();
});
