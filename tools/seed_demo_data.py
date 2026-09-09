#!/usr/bin/env python3
"""Fills the local Sqlite database with a realistic-sized demo catalogue.

Six seeded products are enough to prove a page renders, but not to see how the
storefront, the admin panel and the mobile API actually behave: pagination,
filter facets, sort correctness, list scrolling and query cost only become
visible at a few hundred rows. This script generates that volume with coherent
Arabic and English content so the result is usable for judging the UI rather
than a wall of "Product 47".

Only ever writes to the Sqlite development database. It refuses to touch
anything else, because generated data must never reach a real catalogue.

Usage:
    python3 tools/seed_demo_data.py            # add data (keeps existing rows)
    python3 tools/seed_demo_data.py --reset    # remove previously generated rows first

The generated rows are marked: products carry a `demo-` slug prefix and orders
an `ORD-DEMO-` number prefix, so --reset can remove exactly what this script
created and leave the original seeds intact.
"""

from __future__ import annotations

import argparse
import random
import sqlite3
import sys
from datetime import datetime, timedelta, timezone
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
DB = REPO / "unishop.local.db"

# Deterministic: the same catalogue every run, so a screenshot or a measured
# page timing can be compared across runs and between the site and the app.
SEED = 20260908

DEMO_SLUG_PREFIX = "demo-"
DEMO_ORDER_PREFIX = "ORD-DEMO-"

# ── Catalogue definition ─────────────────────────────────────────────────────
# Categories beyond the three that ship seeded, chosen to look like a plausible
# consumer-electronics shop. (ar, en, slug, emoji)
CATEGORIES = [
    ("سماعات", "Headphones", "headphones", "🎧"),
    ("ساعات ذكية", "Smart Watches", "smart-watches", "⌚"),
    ("شواحن وبطاريات", "Chargers & Power", "chargers-power", "🔌"),
    ("لوحات مفاتيح", "Keyboards", "keyboards", "⌨️"),
    ("ماوس وألعاب", "Mice & Gaming", "mice-gaming", "🖱️"),
    ("مكبرات صوت", "Speakers", "speakers", "🔊"),
    ("إضاءة ذكية", "Smart Lighting", "smart-lighting", "💡"),
    ("منظفات منزلية", "Home Cleaning", "home-cleaning", "🧹"),
    ("ملحقات موبايل", "Phone Accessories", "phone-accessories", "📲"),
    ("أجهزة تخزين", "Storage", "storage", "💾"),
]

BRANDS = [
    ("Anker", "anker"), ("Baseus", "baseus"), ("Xiaomi", "xiaomi"),
    ("Logitech", "logitech"), ("JBL", "jbl"), ("Redragon", "redragon"),
    ("Samsung", "samsung"), ("SanDisk", "sandisk"), ("Havit", "havit"),
    ("Ugreen", "ugreen"), ("Tronsmart", "tronsmart"), ("Yeelight", "yeelight"),
]

# Per-category product vocabulary so names read naturally instead of being
# assembled from one generic list.
# category_slug -> (ar_noun, en_noun, [(ar_modifier, en_modifier)], [ar_feature])
CATALOG = {
    "headphones": (
        "سماعة", "Headphones",
        [("لاسلكية", "Wireless"), ("بلوتوث", "Bluetooth"), ("للألعاب", "Gaming"),
         ("عازلة للضوضاء", "Noise Cancelling"), ("رياضية", "Sport"), ("سلكية", "Wired")],
        ["عزل ضوضاء نشط", "بطارية تدوم 40 ساعة", "بلوتوث 5.3", "مقاومة للعرق",
         "ميكروفون مزدوج", "شحن سريع 10 دقائق"],
    ),
    "smart-watches": (
        "ساعة ذكية", "Smart Watch",
        [("بشاشة AMOLED", "AMOLED"), ("رياضية", "Sport"), ("كلاسيكية", "Classic"),
         ("للأطفال", "Kids"), ("مقاومة للماء", "Waterproof")],
        ["قياس معدل ضربات القلب", "تتبع النوم", "GPS مدمج", "مقاومة ماء 5ATM",
         "أكثر من 100 نمط رياضي", "شاشة دائمة العرض"],
    ),
    "chargers-power": (
        "شاحن", "Charger",
        [("سريع 65W", "65W Fast"), ("لاسلكي", "Wireless"), ("للسيارة", "Car"),
         ("متعدد المنافذ", "Multi-Port"), ("بطارية متنقلة", "Power Bank")],
        ["شحن سريع PD 3.0", "حماية من الحرارة", "منفذ USB-C و USB-A",
         "تصميم مدمج", "توافق واسع", "مؤشر LED"],
    ),
    "keyboards": (
        "لوحة مفاتيح", "Keyboard",
        [("ميكانيكية", "Mechanical"), ("لاسلكية", "Wireless"), ("مضيئة RGB", "RGB"),
         ("مدمجة", "Compact"), ("عربي إنجليزي", "Arabic English")],
        ["مفاتيح ميكانيكية أزرق", "إضاءة RGB قابلة للتخصيص", "هيكل ألومنيوم",
         "حروف عربية مطبوعة بالليزر", "مقاومة للرش", "استجابة 1ms"],
    ),
    "mice-gaming": (
        "ماوس", "Mouse",
        [("للألعاب", "Gaming"), ("لاسلكي", "Wireless"), ("صامت", "Silent"),
         ("قابل للبرمجة", "Programmable"), ("مريح للمعصم", "Ergonomic")],
        ["مستشعر 16000 DPI", "6 أزرار قابلة للبرمجة", "إضاءة RGB",
         "أرجل PTFE ناعمة", "بطارية 70 ساعة", "وزن خفيف 65 جرام"],
    ),
    "speakers": (
        "مكبر صوت", "Speaker",
        [("بلوتوث", "Bluetooth"), ("محمول", "Portable"), ("مقاوم للماء", "Waterproof"),
         ("للحفلات", "Party"), ("مكتبي", "Desktop")],
        ["صوت محيطي 360 درجة", "مقاومة ماء IPX7", "بطارية 24 ساعة",
         "ربط مكبرين معًا", "باس عميق", "مايك مدمج للمكالمات"],
    ),
    "smart-lighting": (
        "إضاءة", "Light",
        [("ذكية LED", "Smart LED"), ("شريط RGB", "RGB Strip"), ("لمبة ملونة", "Color Bulb"),
         ("مصباح مكتب", "Desk Lamp"), ("بانل سقف", "Ceiling Panel")],
        ["16 مليون لون", "تحكم بالتطبيق", "متوافقة مع Alexa",
         "جداول زمنية تلقائية", "توفير 85% طاقة", "تعمل بالصوت"],
    ),
    "home-cleaning": (
        "مكنسة", "Vacuum",
        [("لاسلكية", "Cordless"), ("روبوت", "Robot"), ("للسيارة", "Car"),
         ("بخارية", "Steam"), ("عمودية", "Upright")],
        ["قوة شفط 20000Pa", "فلتر HEPA", "بطارية 60 دقيقة",
         "خزان غبار 600 مل", "تشغيل هادئ", "ملحقات متعددة"],
    ),
    "phone-accessories": (
        "حامل", "Holder",
        [("للسيارة", "Car"), ("مغناطيسي", "Magnetic"), ("حلقة", "Ring"),
         ("ستاند مكتبي", "Desk Stand"), ("جراب واقي", "Protective Case")],
        ["تثبيت قوي", "دوران 360 درجة", "خامة سيليكون",
         "متوافق مع MagSafe", "تركيب بدون أدوات", "حماية من السقوط"],
    ),
    "storage": (
        "ذاكرة", "Storage",
        [("فلاش USB 3.2", "USB 3.2 Flash"), ("كارت SD", "SD Card"),
         ("هارد خارجي SSD", "External SSD"), ("قارئ كروت", "Card Reader")],
        ["سرعة قراءة 400MB/s", "مقاومة للصدمات", "تصميم معدني",
         "متوافق مع الكاميرات", "ضمان 5 سنوات", "حجم صغير محمول"],
    ),
    # The three originally-seeded categories, so generated products land there too.
    "tablets": (
        "تابلت", "Tablet",
        [("للأطفال", "Kids"), ("للرسم", "Drawing"), ("بشاشة 10 بوصة", "10 inch"),
         ("مع كيبورد", "With Keyboard"), ("للدراسة", "Study")],
        ["شاشة عالية الدقة", "بطارية طويلة", "ذاكرة قابلة للتوسيع",
         "كاميرا مزدوجة", "معالج ثماني النواة", "وضع حماية العين"],
    ),
    "fans": (
        "مروحة", "Fan",
        [("محمولة", "Portable"), ("مكتبية", "Desk"), ("معلقة بالرقبة", "Neck"),
         ("قابلة للطي", "Foldable"), ("USB", "USB")],
        ["بطارية 10000mAh", "4 سرعات", "تشغيل صامت",
         "شفرات آمنة", "تصميم خفيف", "شحن USB-C"],
    ),
    "webcams": (
        "كاميرا ويب", "Webcam",
        [("بدقة 1080p", "1080p"), ("بدقة 4K", "4K"), ("مع مايك", "With Mic"),
         ("للبث المباشر", "Streaming"), ("بحلقة إضاءة", "Ring Light")],
        ["دقة عالية", "تصحيح إضاءة تلقائي", "مايك مزدوج",
         "زاوية واسعة 90 درجة", "تركيب بلا برامج", "غطاء خصوصية"],
    ),
}

# Emoji per category. Products have no images in this dataset, so the emoji is
# what both the site and the app render as the placeholder — using the
# category's own keeps a grid visually coherent instead of randomly speckled.
CATEGORY_EMOJI = {
    "headphones": "🎧", "smart-watches": "⌚", "chargers-power": "🔌",
    "keyboards": "⌨️", "mice-gaming": "🖱️", "speakers": "🔊",
    "smart-lighting": "💡", "home-cleaning": "🧹", "phone-accessories": "📲",
    "storage": "💾", "tablets": "📱", "fans": "🌀", "webcams": "📷",
}

BADGES = [None, None, None, "NEW", "HOT", "SALE", "BEST"]

CITIES = [
    ("القاهرة", "القاهرة"), ("الجيزة", "الجيزة"), ("الإسكندرية", "الإسكندرية"),
    ("المنصورة", "الدقهلية"), ("طنطا", "الغربية"), ("أسيوط", "أسيوط"),
    ("الزقازيق", "الشرقية"), ("بورسعيد", "بورسعيد"), ("سوهاج", "سوهاج"),
    ("الفيوم", "الفيوم"), ("شبين الكوم", "المنوفية"), ("دمنهور", "البحيرة"),
]

FIRST_NAMES = ["أحمد", "محمد", "محمود", "مصطفى", "خالد", "عمر", "يوسف", "كريم",
               "فاطمة", "مريم", "سارة", "نور", "هبة", "أمل", "ياسمين", "دينا"]
LAST_NAMES = ["حسن", "علي", "إبراهيم", "السيد", "عبد الله", "فتحي", "رمضان",
              "شريف", "صلاح", "زكي", "منصور", "الشافعي"]


def table_columns(cur: sqlite3.Cursor, table: str) -> set[str]:
    return {row[1] for row in cur.execute(f"PRAGMA table_info({table})")}


def insert(cur: sqlite3.Cursor, table: str, values: dict) -> None:
    """Inserts `values`, dropping keys the table does not have.

    Written against the live schema rather than a hard-coded column list. The
    first version of this script assumed the columns and failed on
    `Categories.ShowOnHomepage` — a NOT NULL column with no default that the
    entity gained after the original seed migration. Filtering by the actual
    table definition means adding or renaming a column cannot silently break
    seeding, and a column this script does not know about is simply left to its
    own default.
    """
    cols = table_columns(cur, table)
    used = {k: v for k, v in values.items() if k in cols}
    missing = set(values) - cols
    if missing:
        print(f"  note: {table} has no column(s) {sorted(missing)} — skipped")
    cur.execute(
        f"INSERT INTO {table} ({','.join(used)}) "
        f"VALUES ({','.join('?' * len(used))})",
        list(used.values()))


def price_for(category_slug: str, rng: random.Random) -> tuple[float, float | None]:
    """Plausible price for a category, and an optional higher 'was' price."""
    bands = {
        "headphones": (350, 4500), "smart-watches": (600, 7000),
        "chargers-power": (150, 2200), "keyboards": (400, 3800),
        "mice-gaming": (200, 2500), "speakers": (450, 6000),
        "smart-lighting": (120, 1800), "home-cleaning": (900, 14000),
        "phone-accessories": (80, 900), "storage": (200, 5500),
        "tablets": (2500, 15000), "fans": (250, 1900),
        "webcams": (600, 5000),
    }
    lo, hi = bands.get(category_slug, (200, 3000))
    # Round to a retail-looking value rather than a uniform float.
    price = rng.randint(lo, hi)
    price = round(price / 10) * 10 - 1  # e.g. 1299, 899
    if price < 50:
        price = 99

    old = None
    if rng.random() < 0.32:  # roughly a third on sale
        old = round(price * rng.uniform(1.15, 1.6) / 10) * 10 - 1
    return float(price), (float(old) if old else None)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--products", type=int, default=420,
                        help="how many products to generate (default 420)")
    parser.add_argument("--orders", type=int, default=180,
                        help="how many orders to generate (default 180)")
    parser.add_argument("--reset", action="store_true",
                        help="delete previously generated demo rows first")
    args = parser.parse_args()

    if not DB.exists():
        sys.exit(f"Database not found: {DB}\n"
                 "Start the site once with Database:Provider=Sqlite so the schema is created.")

    rng = random.Random(SEED)
    conn = sqlite3.connect(DB)
    conn.execute("PRAGMA foreign_keys = ON")
    cur = conn.cursor()

    if args.reset:
        # Delete order details before orders, and orders before products, so the
        # foreign keys enabled above stay satisfied at every step.
        cur.execute(
            "DELETE FROM SalesOrderDetails WHERE OrderId IN "
            "(SELECT Id FROM SalesOrders WHERE OrderNumber LIKE ?)",
            (DEMO_ORDER_PREFIX + "%",))
        cur.execute("DELETE FROM SalesOrders WHERE OrderNumber LIKE ?",
                    (DEMO_ORDER_PREFIX + "%",))
        cur.execute("DELETE FROM Products WHERE Slug LIKE ?", (DEMO_SLUG_PREFIX + "%",))
        conn.commit()
        print("Removed previously generated demo rows.")

    stamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")

    # ── Categories ───────────────────────────────────────────────────────────
    existing_cats = {row[0]: row[1] for row in cur.execute("SELECT Slug, Id FROM Categories")}
    next_cat_sort = (cur.execute("SELECT COALESCE(MAX(SortOrder),0) FROM Categories")
                     .fetchone()[0])
    added_cats = 0
    for ar, en, slug, emoji in CATEGORIES:
        if slug in existing_cats:
            continue
        next_cat_sort += 1
        values = {
            "NameAr": ar, "NameEn": en, "Slug": slug, "Emoji": emoji,
            "IsActive": 1, "SortOrder": next_cat_sort,
            # Only a few categories are surfaced on the homepage; flooding it
            # with thirteen tiles is not what the design intends.
            "ShowOnHomepage": 1 if added_cats < 3 else 0,
            "CreatedAt": stamp,
        }
        insert(cur, "Categories", values)
        existing_cats[slug] = cur.lastrowid
        added_cats += 1

    # ── Brands ───────────────────────────────────────────────────────────────
    existing_brands = {row[0]: row[1] for row in cur.execute("SELECT Slug, Id FROM Brands")}
    added_brands = 0
    for idx, (name, slug) in enumerate(BRANDS):
        if slug in existing_brands:
            continue
        insert(cur, "Brands", {
            "NameAr": name, "NameEn": name, "Slug": slug,
            "IsActive": 1, "SortOrder": idx, "CreatedAt": stamp,
        })
        existing_brands[slug] = cur.lastrowid
        added_brands += 1

    conn.commit()

    # ── Products ─────────────────────────────────────────────────────────────
    cat_slugs = [s for s in CATALOG if s in existing_cats]
    brand_ids = list(existing_brands.values())
    brand_names = {v: k for k, v in existing_brands.items()}
    now = datetime.now(timezone.utc)

    taken_slugs = {r[0] for r in cur.execute("SELECT Slug FROM Products")}
    product_rows = []
    for i in range(args.products):
        cat_slug = cat_slugs[i % len(cat_slugs)]  # even spread across categories
        cat_id = existing_cats[cat_slug]
        ar_noun, en_noun, modifiers, features = CATALOG[cat_slug]
        ar_mod, en_mod = rng.choice(modifiers)

        brand_id = rng.choice(brand_ids)
        brand_slug = brand_names[brand_id]
        brand_display = next((n for n, s in BRANDS if s == brand_slug), brand_slug.upper())

        model = f"{rng.choice('ABCEGHKLMNPRSTXZ')}{rng.randint(10, 99)}"
        name_ar = f"{ar_noun} {brand_display} {model} {ar_mod}"
        name_en = f"{brand_display} {model} {en_mod} {en_noun}"

        slug = f"{DEMO_SLUG_PREFIX}{brand_slug}-{cat_slug}-{model.lower()}-{i}"
        if slug in taken_slugs:
            continue
        taken_slugs.add(slug)

        price, old_price = price_for(cat_slug, rng)
        picked = rng.sample(features, k=min(3, len(features)))
        short_ar = "، ".join(picked)
        short_en = f"{en_mod} {en_noun.lower()} by {brand_display}"
        desc_ar = (f"{name_ar} — {short_ar}. منتج أصلي بضمان معتمد، "
                   f"مناسب للاستخدام اليومي وتوصيل لكافة محافظات مصر.")
        desc_en = (f"{name_en}. Genuine product with warranty, "
                   f"suitable for daily use, delivered across Egypt.")

        # A realistic mix rather than everything in stock: some low, some out.
        roll = rng.random()
        stock = 0 if roll < 0.07 else (rng.randint(1, 4) if roll < 0.2 else rng.randint(5, 180))

        product_rows.append((
            cat_id, brand_id, name_ar, name_en, slug,
            short_ar, short_en, desc_ar, desc_en,
            price, old_price,
            rng.choice(BADGES), None, brand_display,
            CATEGORY_EMOJI.get(cat_slug, "📦"),
            1 if rng.random() > 0.05 else 0,           # IsActive
            1 if rng.random() < 0.12 else 0,           # IsFeatured
            rng.randint(0, 999),                       # SortOrder
            stock,
            (now - timedelta(days=rng.randint(0, 400))).strftime("%Y-%m-%d %H:%M:%S"),
        ))

    cur.executemany(
        "INSERT INTO Products (CategoryId, BrandId, NameAr, NameEn, Slug, "
        "ShortDescAr, ShortDescEn, DescAr, DescEn, Price, OldPrice, Badge, "
        "ImagePath, BrandName, Emoji, IsActive, IsFeatured, SortOrder, Stock, CreatedAt) "
        "VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
        product_rows)
    conn.commit()

    # ── Orders ───────────────────────────────────────────────────────────────
    sellable = list(cur.execute(
        "SELECT Id, NameAr, Price FROM Products WHERE IsActive=1 AND Stock > 0"))
    status_ids = [r[0] for r in cur.execute("SELECT Id FROM OrderStatuses")]

    orders_made = 0
    details_made = 0
    for n in range(args.orders):
        created = now - timedelta(days=rng.randint(0, 120),
                                  hours=rng.randint(0, 23),
                                  minutes=rng.randint(0, 59))
        city, gov = rng.choice(CITIES)
        name = f"{rng.choice(FIRST_NAMES)} {rng.choice(LAST_NAMES)}"
        phone = f"01{rng.choice('0125')}{rng.randint(10**7, 10**8 - 1)}"

        items = rng.sample(sellable, k=rng.randint(1, 4))
        lines = []
        subtotal = 0.0
        for pid, pname, pprice in items:
            qty = rng.randint(1, 3)
            line_total = round(pprice * qty, 2)
            subtotal += line_total
            lines.append((pid, pname, qty, pprice, line_total))

        # Free shipping over 5000, matching a common storefront rule.
        shipping = 0.0 if subtotal >= 5000 else float(rng.choice([50, 60, 75]))
        total = round(subtotal + shipping, 2)

        cur.execute(
            "INSERT INTO SalesOrders (OrderNumber, StatusId, UserId, CustomerName, "
            "CustomerPhone, CustomerEmail, Address, City, Governorate, SubTotal, "
            "ShippingFee, TotalAmount, Notes, AdminNotes, CreatedAt, UpdatedAt) "
            "VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
            (f"{DEMO_ORDER_PREFIX}{10000 + n}", rng.choice(status_ids), None, name,
             phone, None, f"{rng.randint(1, 200)} شارع {rng.choice(LAST_NAMES)}",
             city, gov, round(subtotal, 2), shipping, total, None, None,
             created.strftime("%Y-%m-%d %H:%M:%S"), None))
        order_id = cur.lastrowid
        orders_made += 1

        cur.executemany(
            "INSERT INTO SalesOrderDetails (OrderId, ProductId, ProductName, "
            "Quantity, UnitPrice, SubTotal) VALUES (?,?,?,?,?,?)",
            [(order_id, pid, pname, qty, price, line) for pid, pname, qty, price, line in lines])
        details_made += len(lines)

    conn.commit()

    # ── Summary ──────────────────────────────────────────────────────────────
    def count(table: str) -> int:
        return cur.execute(f"SELECT COUNT(*) FROM {table}").fetchone()[0]

    print(f"Added  : {added_cats} categories, {added_brands} brands, "
          f"{len(product_rows)} products, {orders_made} orders ({details_made} lines)")
    print(f"Totals : Categories={count('Categories')} Brands={count('Brands')} "
          f"Products={count('Products')} Orders={count('SalesOrders')} "
          f"OrderLines={count('SalesOrderDetails')}")

    conn.close()


if __name__ == "__main__":
    main()
