# 📱 خطة بناء تطبيق موبايل (Android + iOS) لمشروع Uni-Shop / FTD TechZone

> **تاريخ الإعداد:** 2026-09-06
> **الإصدار:** 1.0
> **نطاق التحليل:** `FTD.Domain` · `FTD.Application` · `FTD.Infrastructure` · `FTD.Web` · `FTD.Api`
> **الهدف:** تحليل المشروع القائم بالكامل، تقييم جاهزية الـ Web Service الحالي، اختيار التقنية الأنسب للموبايل (Flutter؟)، وتقديم خطة تنفيذ تفصيلية قابلة للتطبيق مرحلة بمرحلة.

---

## 📖 فهرس المحتويات

| # | القسم |
|---|-------|
| 1 | [الملخص التنفيذي (اقرأ هذا أولاً)](#1-الملخص-التنفيذي) |
| 2 | [تحليل المشروع الحالي — ما وجدناه فعلياً في الكود](#2-تحليل-المشروع-الحالي) |
| 3 | [تحليل الـ Web Service الحالي (FTD.Api) — الجاهزية والفجوات](#3-تحليل-الـ-web-service-الحالي-ftdapi) |
| 4 | [هل Flutter هو الأفضل؟ — مقارنة تقنية محسومة بالأرقام](#4-هل-flutter-هو-الأفضل) |
| 5 | [الفجوات الحرجة في الـ API قبل بدء الموبايل (Blockers)](#5-الفجوات-الحرجة-في-الـ-api-blockers) |
| 6 | [معمارية الـ Backend المستهدفة (API v2)](#6-معمارية-الـ-backend-المستهدفة-api-v2) |
| 7 | [عقد الـ API الكامل المطلوب (Endpoint Contract)](#7-عقد-الـ-api-الكامل-المطلوب) |
| 8 | [معمارية تطبيق Flutter المقترحة](#8-معمارية-تطبيق-flutter-المقترحة) |
| 9 | [خريطة شاشات التطبيق (Screen Map)](#9-خريطة-شاشات-التطبيق) |
| 10 | [خطة التنفيذ بالمراحل والأسابيع (Roadmap)](#10-خطة-التنفيذ-بالمراحل-والأسابيع) |
| 11 | [الأمن والحماية (Security Hardening)](#11-الأمن-والحماية) |
| 12 | [الإشعارات الفورية (Push Notifications)](#12-الإشعارات-الفورية) |
| 13 | [الاختبارات وضمان الجودة](#13-الاختبارات-وضمان-الجودة) |
| 14 | [DevOps والنشر على المتاجر](#14-devops-والنشر-على-المتاجر) |
| 15 | [التقديرات الزمنية والتكاليف والفريق](#15-التقديرات-الزمنية-والتكاليف-والفريق) |
| 16 | [المخاطر وخطط التعامل معها](#16-المخاطر-وخطط-التعامل-معها) |
| 17 | [قائمة المهام التنفيذية النهائية (Checklist)](#17-قائمة-المهام-التنفيذية-النهائية) |

---

# 1. الملخص التنفيذي

## 1.1 الخبر السار ✅

المشروع **مبني بمعمارية نظيفة (Clean Architecture) صحيحة فعلاً وليست شكلية**، وهذا أهم أصل تملكه الآن:

* منطق الأعمال كله يعيش في `FTD.Application` (6 خدمات + 9 عقود Interfaces) ولا يعرف أي شيء عن HTTP.
* `FTD.Api` موجود بالفعل كمشروع مستقل (المشروع الخامس) ويشارك نفس `Application` + `Infrastructure`.
* JWT مُفعّل ومضبوط بشكل سليم (`ValidateIssuer` + `ValidateAudience` + `ValidateLifetime` + `ClockSkew = Zero`).
* الـ DTOs معزولة عن الـ Entities — أي التطبيق لن يستهلك كيانات قاعدة البيانات مباشرة.
* Rate Limiting + Health Checks + CORS مقيّد بالإعدادات — كلها موجودة.
* التجريد الذكي `ICartStorage` يعني أن السلة **ليست مربوطة بجلسة المتصفح** في طبقة المنطق.

**الترجمة العملية:** لن نحتاج إعادة كتابة منطق الأعمال. سنبني **طبقة عرض جديدة (Mobile)** تتحدث مع API موسّع — وهذا بالضبط ما صُممت المعمارية لأجله.

## 1.2 الخبر الذي يحتاج عملاً ⚠️

الـ API الحالي **يغطي ~35% فقط** من احتياج تطبيق موبايل تجاري:

| موجود ✅ | ناقص ❌ |
|---------|--------|
| قائمة المنتجات + فلترة أساسية | **حسابات العملاء** (تسجيل/دخول/OTP) — لا يوجد أي منها |
| تفاصيل منتج بالـ slug | **Pagination** — الـ API يرجّع كل المنتجات دفعة واحدة |
| التصنيفات والماركات | **تتبع الطلبات** — مذكور في التوثيق لكنه **غير مُنفّذ في الكود** |
| Checkout كضيف (Guest) | **سلة على الخادم** + المفضلة + سجل الطلبات |
| رسائل اتصل بنا | **Refresh Token** — التوكن ينتهي بعد 120 دقيقة ويطرد المستخدم |
| لوحة أدمن (Dashboard + Orders) | **الصفحات/المحتوى/الإعدادات/البحث/Push/الدفع** |

## 1.3 القرار التقني الموصى به 🎯

| السؤال | القرار | درجة الثقة |
|--------|--------|-----------|
| **هل Flutter أفضل حاجة؟** | **نعم — Flutter هو الخيار الأمثل لهذا المشروع تحديداً** | 🟢 عالية جداً |
| البديل الأقرب | React Native (لو الفريق خبير JS/TS بالفعل) | 🟡 |
| البديل المرفوض | .NET MAUI (رغم أن الفريق C#) — الأسباب في [القسم 4](#4-هل-flutter-هو-الأفضل) | 🔴 |
| Native منفصل (Kotlin + Swift) | مرفوض — ضعف الجدوى: تكلفة ×2.2 لمكسب هامشي | 🔴 |

**سبب اختيار Flutter في سطر واحد:** التطبيق واجهته RTL عربية غنية بصرياً + كتالوج بصور + تصميم مخصص بالكامل (المشروع أصلاً بلا Bootstrap/Tailwind — CSS يدوي 4464 سطر) — وهذا سيناريو Flutter المثالي، لأن Flutter يرسم كل بكسل بنفسه فيعطي **تطابقاً 100% بين Android و iOS** ودعم RTL من الدرجة الأولى.

## 1.4 المدة والتكلفة (خلاصة)

| المسار | المدة | الفريق | ملاحظة |
|-------|------|--------|--------|
| **MVP قابل للنشر** | **10–12 أسبوع** | 1 Backend + 1 Flutter + نصف QA | متجر كامل بحسابات وطلبات وإشعارات |
| النسخة الكاملة (V1.0) | 16–18 أسبوع | نفس الفريق + مصمم | + دفع إلكتروني + مراجعات + كوبونات |
| **الـ Backend وحده (المرحلة 0+1)** | **3–4 أسابيع** | 1 Backend | ⚠️ **إلزامي قبل أي كود Flutter** |

> 🔴 **أهم توصية في المستند كله:** لا تبدأ كتابة كود Flutter قبل إنهاء **المرحلة 1 (توسيع الـ API)**. بناء التطبيق على API ناقص = إعادة كتابة 40% منه لاحقاً.

---

# 2. تحليل المشروع الحالي

## 2.1 هوية المشروع

**Uni-Shop (يوني شوب) / FTD TechZone** — منصة تجارة إلكترونية ثنائية اللغة (عربي/إنجليزي، RTL/LTR) موجّهة للسوق المصري، تبيع أجهزة تقنية (تابلتات، مراوح محمولة، كاميرات ويب) من ماركات مثل DOOGEE و JisuLife و Dreame.

## 2.2 المكدس التقني الفعلي (مُتحقق منه من الكود)

| الطبقة | التقنية | الإصدار | مصدر التحقق |
|--------|---------|---------|-------------|
| إطار العمل | ASP.NET Core MVC + Web API | **net9.0** | `FTD.Api.csproj` |
| ORM | Entity Framework Core | **9.0.0** | `*.csproj` |
| قاعدة البيانات | SQL Server | — | `UseSqlServer(...)` في `Program.cs` |
| الهوية | ASP.NET Core Identity (`IdentityUser`/`IdentityRole`) | 9.0 | `AddIdentity<>` |
| توثيق الـ API | JWT Bearer (HMAC-SHA256) | 9.0.0 | `AuthController.GenerateJwtToken` |
| الواجهة | Vanilla CSS + Vanilla JS (بلا أي framework) | — | `site.css` 4464 سطر، `site.js` 291 سطر |
| السلة | Session-based عبر `ICartStorage` | — | `SessionCartStorage.cs` |
| الكاش | `IMemoryCache` + `ResponseCompression` | — | `FTD.Web/Program.cs` |
| البريد | SMTP عبر `EmailService` | — | `FTD.Infrastructure/Services` |

## 2.3 الهيكل المعماري (5 مشاريع)

```text
FTD.Web  (MVC — المتجر + لوحة الأدمن)  ┐
                                        ├──> FTD.Application (المنطق + DTOs + العقود)
FTD.Api  (REST — الويب سيرفيس)         ┘            │
                                                     ▼
FTD.Infrastructure (AppDbContext + Migrations + Email)
                                                     │
                                                     ▼
                                        FTD.Domain (17 كياناً — القلب النقي)
```

**قاعدة الاعتماديات محفوظة بشكل صحيح:** `Domain` لا يعتمد على أي شيء، و`Application` لا تعرف EF Core إلا عبر `IAppDbContext` (تجريد `DbSet` خلف عقد) — وهذا يعني أن **إضافة عميل موبايل لا تمس المنطق إطلاقاً**.

## 2.4 نموذج البيانات — 17 كياناً

| المجموعة | الكيانات | الأهمية للموبايل |
|---------|----------|-----------------|
| **الكتالوج** | `Product`, `Category`, `Brand`, `ProductImage` | 🔴 حرجة — قلب التطبيق |
| **المواصفات/الفلترة** | `ProductAttribute`, `AttributeValue`, `ProductAttributeValue` | 🟠 مهمة — شاشة الفلاتر |
| **الطلبات** | `SalesOrder`, `SalesOrderDetail`, `OrderStatus` | 🔴 حرجة — الشراء والتتبع |
| **المحتوى** | `ContentBlock`, `ContentPage`, `PageSection`, `NavigationItem` | 🟡 متوسطة — صفحات ثابتة داخل التطبيق |
| **التواصل والإعدادات** | `ContactInfo`, `ContactMessage`, `SiteSetting` | 🟡 متوسطة — شاشة "تواصل معنا" + الإعدادات |

### تفاصيل حرجة اكتشفناها في نموذج البيانات

1. **ثنائية اللغة مبنية في الأعمدة نفسها** (`NameAr`/`NameEn`, `DescAr`/`DescEn`, `TitleAr`/`TitleEn`).
   → **تأثير على الموبايل:** الـ API يجب أن يرجّع الحقلين معاً (والتطبيق يختار)، **أو** يستقبل ترويسة `Accept-Language` ويرجّع حقلاً واحداً موحداً. **التوصية: الثاني** — أخف على الشبكة وأنظف في كود Flutter (تفاصيل في [7.1](#71-قاعدة-التوطين-localization-contract)).

2. **`ProductDto` ثقيل جداً** — يحتوي `Category` + `Brand` + `AttributeValues` (وكل واحدة تحمل `Attribute` و `AttributeValue` بداخلها) + `Images` + `FeaturesJson` + حقول SEO (`MetaTitle`, `MetaDesc`).
   → **تأثير على الموبايل:** إرسال هذا الـ DTO في قائمة من 100 منتج = **حِمل شبكة هائل** على 3G/4G مصري. **نحتاج `ProductListItemDto` مبسّط** (تفاصيل في [5.2](#52-الفجوة-2-حجم-الحمولة-payload-bloat)).

3. **السعر `decimal(18,2)`** → في JSON يخرج رقماً. **يجب أن يُقرأ في Dart كـ `Decimal` (حزمة `decimal`) لا `double`** لتجنّب أخطاء التقريب في الحسابات المالية.

4. **`FeaturesJson`** حقل نصي يحمل JSON حراً → التطبيق يحتاج parsing متسامح (fail-safe) لأن المحتوى يحرره الأدمن يدوياً.

5. **`SalesOrder` لا يحتوي `UserId`** — أي الطلبات حالياً **مجهولة الهوية (Guest orders)** ولا يمكن ربطها بحساب عميل.
   → 🔴 **هذه أكبر فجوة في نموذج البيانات** بالنسبة للموبايل: بدونها لا يوجد "طلباتي". تحتاج **Migration جديدة** (تفاصيل في [5.1](#51-الفجوة-1-لا-يوجد-نظام-حسابات-للعملاء)).

## 2.5 طبقة المنطق — الخدمات الستة

| الخدمة | الحجم | جاهزة للموبايل؟ | الملاحظة |
|-------|-------|----------------|---------|
| `ProductService` | ~907 سطر / 32 طريقة | 🟢 90% | تحتاج **Pagination** فقط |
| `OrderService` | 5 طرق | 🟠 70% | تحتاج `GetByOrderNumberAsync` (التتبع) + ربط `UserId` |
| `CartService` | 6 طرق | 🟢 100% | التجريد `ICartStorage` عبقري — سنكتب تنفيذاً جديداً للموبايل |
| `ContentService` | 30+ طريقة | 🟢 95% | تحتاج فقط تعريضها في الـ API |
| `DashboardService` | 1 طريقة | 🟢 100% | تعمل للأدمن |
| `MessageService` | 6 طرق | 🟢 100% | تعمل |

### نقطة قوة معمارية نادرة — `ICartStorage`

```csharp
public interface ICartStorage {
    string? GetRaw();
    void SetRaw(string json);
    void Clear();
}
```

`CartService` لا يعرف الجلسة (Session) إطلاقاً — يطلب "مخزناً" مجرداً فقط. الويب يحقن `SessionCartStorage`.
→ **للموبايل نكتب `DbCartStorage`** (سلة محفوظة في قاعدة البيانات مرتبطة بـ `UserId`) **بدون لمس سطر واحد** من منطق السلة. هذا يوفر ~أسبوع عمل.

## 2.6 مستوى الجودة والنضج

المشروع مرّ بجولات تدقيق حقيقية موثقة:

* **`AUDIT_REPORT.md`** — 26 ملاحظة (7 حرجة) **تم إصلاحها كلها** وموثقة في Resolution Log:
  * `DeleteBehavior.Restrict` على `SalesOrderDetail → Product` (حماية التاريخ المالي)
  * إزالة N+1 في الـ Checkout (استعلام مجمّع `WHERE Id IN`)
  * `.AsNoTracking()` على كل استعلامات القراءة
  * DataAnnotations كاملة + فحص `ModelState.IsValid`
  * فحص تفرّد الـ Slug مسبقاً
* **جولة Pentest & Performance** (commit `0ba8cab`) — إغلاق XSS، تقييد CORS، تقوية JWT، تحسين الأداء 55×.
* **`CODE_REVIEW_AR.md`** (75 ألف حرف) + **`UNISHOP_MASTER_GUIDE.md`** (161 ألف حرف) — توثيق استثنائي.

**التقييم:** 🟢 **قاعدة كود ناضجة ونظيفة وموثقة بشكل ممتاز.** هذا يقلل مخاطر مشروع الموبايل بدرجة كبيرة — لن نبني على رمال.

### الثغرات المتبقية التي تخص الموبايل

| # | الملاحظة | الخطورة | الموقع |
|---|---------|---------|--------|
| M-01 | `JwtSettings:Secret` قيمة نصية `"REPLACE_THIS_WITH_ENV_VAR..."` في `appsettings.json` | 🔴 حرجة | `FTD.Api/appsettings.json` |
| M-02 | `Encoding.ASCII.GetBytes(secret)` — ASCII يقلّص الإنتروبيا الفعلية للمفتاح | 🟠 متوسطة | `Program.cs` + `AuthController` |
| M-03 | لا يوجد Swagger/OpenAPI في `FTD.Api` (التوثيق يذكره لكنه **غير موجود في الكود** — تحققنا: صفر نتائج) | 🟠 متوسطة | `FTD.Api.csproj` |
| M-04 | `AuthController` يمنع كل من ليس `Admin` (`403`) → **لا يمكن لعميل عادي تسجيل الدخول أبداً** | 🔴 حرجة | `AuthController.cs` L45-47 |
| M-05 | لا يوجد Refresh Token → المستخدم يُطرد كل ساعتين | 🔴 حرجة | `AuthController.cs` |
| M-06 | لا Pagination في أي endpoint للقوائم | 🔴 حرجة | `ProductsController.cs` |
| M-07 | `/api/orders/track/{number}` **موثّق لكنه غير منفّذ** | 🟠 متوسطة | مفقود |
| M-08 | مسارات الصور نسبية (`/images/products/x.jpg`) بدون Base URL | 🟠 متوسطة | `MappingExtensions.cs` |
| M-09 | لا يوجد `GlobalExceptionHandler` في الـ API → الاستثناءات غير المتوقعة تخرج 500 بجسم HTML | 🟠 متوسطة | `FTD.Api/Program.cs` |
| M-10 | الـ API بلا Cache/ETag → التطبيق سيعيد تحميل كل شيء في كل مرة | 🟡 منخفضة | `FTD.Api/Program.cs` |

---

# 3. تحليل الـ Web Service الحالي (FTD.Api)

## 3.1 نعم — المشروع يعمل بـ Web Service فعلاً ✅

تحققنا من الكود: `FTD.Api` مشروع **مستقل بالكامل** (`Microsoft.NET.Sdk.Web`, `net9.0`) يعمل على المنفذ `5100`، ويشارك `FTD.Application` و `FTD.Infrastructure` عبر `ProjectReference`. هذا يعني أن الـ Backend **جاهز معمارياً** لاستقبال عميل موبايل — نحتاج فقط توسيعه.

## 3.2 جرد كامل لنقاط النهاية الموجودة فعلياً

| # | Endpoint | Method | الحماية | الحالة | جاهز للموبايل؟ |
|---|----------|--------|---------|--------|----------------|
| 1 | `/` | GET | عام | ✅ يعمل | 🟡 معلومات فقط |
| 2 | `/health` | GET | عام | ✅ يعمل | 🟢 نعم |
| 3 | `/api/auth/login` | POST | عام + RateLimit(5/30s) | ✅ يعمل | 🔴 **للأدمن فقط!** |
| 4 | `/api/products` | GET | عام | ✅ يعمل | 🟠 **بلا Pagination** |
| 5 | `/api/products/{slug}` | GET | عام | ✅ يعمل | 🟢 نعم |
| 6 | `/api/products/categories` | GET | عام | ✅ يعمل | 🟢 نعم |
| 7 | `/api/products/brands` | GET | عام | ✅ يعمل | 🟢 نعم |
| 8 | `/api/orders/checkout` | POST | عام | ✅ يعمل | 🟠 ضيف فقط |
| 9 | `/api/contact` | POST | عام | ✅ يعمل | 🟢 نعم |
| 10 | `/api/admin/dashboard` | GET | JWT + Admin | ✅ يعمل | 🟢 (للأدمن) |
| 11 | `/api/admin/orders` | GET | JWT + Admin | ✅ يعمل | 🟢 (للأدمن) |
| 12 | `/api/admin/orders/{id}` | GET | JWT + Admin | ✅ يعمل | 🟢 (للأدمن) |
| 13 | `/api/admin/orders/{id}/status` | POST | JWT + Admin | ✅ يعمل | 🟢 (للأدمن) |

**الإجمالي: 13 نقطة نهاية.** تطبيق موبايل تجاري كامل يحتاج **~45 نقطة**.

### ⚠️ تنبيه مهم: فجوة بين التوثيق والكود

`UNISHOP_MASTER_GUIDE.md` (القسم 9.1) و `docs/superpowers/specs/2026-07-10-web-service-api-design.md` يذكران نقاط نهاية **غير موجودة في الكود**:

| مذكور في التوثيق | الحقيقة في الكود |
|-----------------|-----------------|
| `api/Orders/track/{number}` | ❌ **غير منفّذ** — لا يوجد أي `track` في `OrdersController` |
| `api/Categories` (مستقل) | ❌ الموجود فعلاً `api/Products/categories` |
| `api/Brands` (مستقل) | ❌ الموجود فعلاً `api/Products/brands` |
| `api/pages/{slug}` | ❌ **غير منفّذ** — لا يوجد `PagesController` في الـ API |
| `PUT` لتحديث حالة الطلب | ⚠️ منفّذ كـ **`POST`** لا `PUT` |
| Swagger مفعّل | ❌ **صفر نتائج** عند البحث عن `Swagger` في كل الحل |
| `POST/PUT/DELETE /api/admin/products` | ❌ **غير منفّذ** — الأدمن لا يستطيع إدارة المنتجات من الـ API |

> 🔴 **لا تعتمد على التوثيق كعقد API.** المصدر الوحيد للحقيقة هو الكود. لهذا نوصي بشدة بإضافة **Swagger/OpenAPI** كأول مهمة في المرحلة 0 — ليصبح العقد مولَّداً من الكود تلقائياً ولا يمكن أن يكذب.

## 3.3 تقييم أمن الـ API الحالي

| البند | الحالة | التقييم |
|------|--------|---------|
| JWT `ValidateIssuer` | ✅ مفعّل | 🟢 ممتاز |
| JWT `ValidateAudience` | ✅ مفعّل | 🟢 ممتاز |
| JWT `ValidateLifetime` + `ClockSkew = Zero` | ✅ مفعّل | 🟢 ممتاز (صارم صح) |
| `RequireHttpsMetadata` خارج التطوير | ✅ مفعّل | 🟢 ممتاز |
| CORS مقيّد بالإعدادات (لا `AllowAnyOrigin` في الإنتاج) | ✅ مفعّل | 🟢 ممتاز |
| Rate Limiting على الدخول (5/30 ثانية) | ✅ مفعّل | 🟢 جيد |
| Health Checks + DbContextCheck | ✅ مفعّل | 🟢 جيد |
| السر في متغير بيئة | ❌ **نص صريح في `appsettings.json`** | 🔴 **حرج** |
| `Encoding.ASCII` للمفتاح | ⚠️ يجب `UTF8` | 🟠 متوسط |
| Refresh Token | ❌ غير موجود | 🔴 حرج للموبايل |
| Global Exception Handler | ❌ غير موجود | 🟠 متوسط |
| Rate Limiting عام (كل الـ API) | ❌ الدخول فقط | 🟠 متوسط |
| قفل الحساب (Lockout) في الـ API | ⚠️ `Web` مفعّل، `Api` لا (`CheckPasswordSignInAsync(..., false)`) | 🟠 متوسط |

**الخلاصة:** الأساس الأمني **قوي ومدروس** — أفضل من معظم المشاريع المشابهة. الفجوات محددة وقابلة للإغلاق في أيام.

## 3.4 لماذا لا يصلح الـ API الحالي لتطبيق موبايل كما هو؟

خمسة أسباب قاطعة:

1. **`AuthController` يرفض غير الأدمن صراحةً:**
   ```csharp
   if (!roles.Contains("Admin"))
       return StatusCode(403, "غير مصرح بالدخول لغير المسؤولين");
   ```
   → أي عميل يحاول تسجيل الدخول من التطبيق سيُرفض. **لا يوجد نظام حسابات عملاء أصلاً.**

2. **لا Pagination:** `GetFilteredByIdAsync` ترجّع **كل المنتجات المطابقة**. مع 500 منتج والـ `ProductDto` الثقيل → استجابة قد تتجاوز عدة ميجابايت. على شبكة موبايل مصرية = تطبيق يبدو "معلّقاً".

3. **الطلبات غير مرتبطة بمستخدم:** `SalesOrder` بلا `UserId` → شاشة "طلباتي" مستحيلة تقنياً.

4. **لا تتبع طلب:** حتى الضيف لا يستطيع معرفة حالة طلبه (الـ endpoint موثّق وغير منفّذ).

5. **التوكن ينتهي بعد 120 دقيقة بلا تجديد:** تجربة موبايل غير مقبولة — المستخدم يتوقع بقاء الجلسة أسابيع.

---

# 4. هل Flutter هو الأفضل؟

## 4.1 الجواب المباشر

> ## ✅ **نعم — Flutter هو الخيار الأمثل لهذا المشروع تحديداً.**

لكن ليس لأنه "التقنية الرائجة" — بل لأسباب مرتبطة مباشرة بخصائص **هذا** المشروع. سأشرح بالأرقام.

## 4.2 المقارنة الشاملة بين الخيارات الخمسة

| المعيار (الوزن) | 🥇 Flutter | React Native | .NET MAUI | Native (Kotlin+Swift) | PWA |
|-----------------|-----------|--------------|-----------|----------------------|-----|
| **جودة دعم RTL/العربية** (وزن 20%) | 🟢 **10/10** ممتاز أصلاً | 🟢 8/10 جيد | 🟡 6/10 مشاكل معروفة | 🟢 9/10 | 🟢 9/10 |
| **مطابقة تصميم مخصص 100%** (20%) | 🟢 **10/10** يرسم كل بكسل | 🟡 7/10 مكونات نظام | 🟡 6/10 | 🟢 9/10 (×2 عمل) | 🟢 8/10 |
| **قاعدة كود واحدة** (15%) | 🟢 **10/10** | 🟢 10/10 | 🟢 9/10 | 🔴 2/10 | 🟢 10/10 |
| **الأداء والسلاسة (60fps)** | 🟢 **9/10** AOT + Impeller | 🟡 7/10 JS Bridge | 🟡 7/10 | 🟢 10/10 | 🔴 5/10 |
| **حجم/جودة النظام البيئي** | 🟢 **9/10** pub.dev غني | 🟢 10/10 npm الأكبر | 🔴 4/10 محدود | 🟢 10/10 | 🟢 9/10 |
| **سوق المطورين في مصر/الخليج** | 🟢 **9/10** وفرة عالية | 🟢 8/10 | 🔴 3/10 نادر جداً | 🟡 6/10 | 🟢 8/10 |
| **جاهزية Push Notifications** | 🟢 9/10 FCM ممتاز | 🟢 9/10 | 🟡 6/10 | 🟢 10/10 | 🔴 4/10 (iOS ضعيف) |
| **استقرار وطول عمر التقنية** | 🟢 9/10 (Google، ناضج) | 🟢 9/10 (Meta) | 🟡 5/10 ⚠️ | 🟢 10/10 | 🟢 8/10 |
| **سرعة التطوير (Hot Reload)** | 🟢 **10/10** | 🟢 9/10 | 🟡 6/10 | 🔴 5/10 | 🟢 9/10 |
| **إعادة استخدام خبرة C# للفريق** | 🔴 2/10 | 🔴 3/10 | 🟢 **10/10** | 🔴 2/10 | 🟡 5/10 |
| **حجم التطبيق النهائي** | 🟡 7/10 (~15-20MB) | 🟡 7/10 | 🔴 5/10 (~30MB+) | 🟢 10/10 | 🟢 10/10 |
| **الظهور في App Store / Play** | 🟢 10/10 | 🟢 10/10 | 🟢 10/10 | 🟢 10/10 | 🔴 2/10 |
| **📊 النتيجة المرجّحة** | **🥇 9.2/10** | 🥈 8.1/10 | 🥉 6.0/10 | 7.4/10 | 6.6/10 |

## 4.3 لماذا Flutter تحديداً لهذا المشروع — 6 أسباب مرتبطة بالكود الفعلي

### السبب 1: مشروعكم أصلاً "تصميم مخصص بالكامل" — وهذا ملعب Flutter
اكتشفنا في الكود: **`site.css` = 4464 سطر يدوي، بلا Bootstrap ولا Tailwind.** الفريق اختار التحكم الكامل في كل بكسل.
Flutter يعمل بنفس الفلسفة تماماً: يرسم الواجهة بمحرّكه الخاص (Impeller/Skia) ولا يستخدم مكونات النظام.
→ **النتيجة:** التطبيق سيبدو **متطابقاً 100%** على Android و iOS، ومطابقاً لهوية الموقع. مع React Native ستقاتل اختلافات مكونات النظام بين المنصتين.

### السبب 2: RTL العربي — Flutter هو الأقوى بفارق واضح
المشروع ثنائي اللغة في **كل كيان** (`NameAr`/`NameEn`). دعم Flutter للـ RTL مبني في النواة:
`Directionality` widget + `EdgeInsetsDirectional` + `TextDirection.rtl` + خطوط عربية عبر `google_fonts` (Cairo/Tajawal).
→ تبديل اللغة يقلب التخطيط كله تلقائياً بسطر واحد. هذا **أضعف نقاط .NET MAUI** (مشاكل RTL موثقة ومزمنة).

### السبب 3: كتالوج بصور كثيفة → Flutter يتفوق في الـ Scrolling
التطبيق شبكة منتجات بصور (`ProductImage` متعدد لكل منتج). Flutter مع `CachedNetworkImage` + `ListView.builder` يعطي تمريراً بـ 60/120fps حتى مع مئات العناصر — لأنه **AOT-compiled** بلا JS Bridge.

### السبب 4: DTOs معقدة متداخلة → `freezed` + `json_serializable` يحسمها
`ProductDto` يحتوي كائنات متداخلة (`Category`, `Brand`, `AttributeValues[]` وكل منها بداخله كائنان، `Images[]`).
حزمة `freezed` تولّد نماذج **immutable** + `fromJson`/`toJson` + `copyWith` + مقارنة — تلقائياً. هذا يوفر مئات الأسطر ويمنع أخطاء null.

### السبب 5: وفرة المطورين في السوق المصري/العربي
Flutter من أكثر التقنيات انتشاراً في المنطقة → توظيف واستبدال أسهل بكثير من .NET MAUI (نادر جداً).

### السبب 6: مسار مستقبلي مجاني — Flutter Web + Desktop
نفس الكود يمكن نشره كتطبيق ويب أو سطح مكتب (مثلاً **تطبيق أدمن سطح مكتب** لإدارة الطلبات) بجهد إضافي بسيط.

## 4.4 لماذا نرفض .NET MAUI رغم أن الفريق C#؟ 🔴

هذا أهم سؤال، لأن الإجابة "بديهية" وخاطئة. الفريق يعرف C# — فلماذا لا MAUI؟

| السبب | التفصيل |
|------|---------|
| **1. مخاطر استراتيجية** | حجم مجتمع MAUI ووتيرة تبنّيه أقل بكثير من Flutter/RN. الاستثمار في تقنية بنظام بيئي ضعيف = مخاطرة على 3-5 سنوات. |
| **2. دعم RTL ضعيف** | أضعف نقطة في MAUI — ومشروعكم **عربي RTL بالأساس**. هذا وحده كافٍ للرفض. |
| **3. نظام بيئي فقير** | ستكتب بنفسك ما هو جاهز في pub.dev (Carousels، Shimmer، Image caching متقدم، Payment SDKs). |
| **4. جودة iOS أقل** | مشاكل أداء وسلوك موثقة على iOS أكثر من Android. |
| **5. حجم التطبيق أكبر** | ~30MB+ مقابل ~15-20MB لـ Flutter — يهم في السوق المصري. |
| **6. سوق مطورين نادر** | إيجاد مطور MAUI في مصر أصعب بمراتب من Flutter. |

> **المكسب الوحيد لـ MAUI** هو إعادة استخدام خبرة C#. لكن مطور واجهات جيد يتعلم Dart في **1-2 أسبوع** (Dart قريب جداً من C#/Java: أنواع ساكنة، `async/await`، `class`، `null-safety`). **أسبوعان تعلّم مقابل 3-5 سنوات مخاطرة تقنية = مقايضة خاسرة بوضوح.**

## 4.5 لماذا نرفض PWA (تطبيق ويب تقدمي)؟

`UNISHOP_MASTER_GUIDE.md` (سطر 1816) يقترح PWA كمسار. تقييمنا: **مسار مكمّل جيد، لكنه ليس بديلاً** — لأن الطلب كان "تطبيق على Android و iPhone":
* ❌ **لا وجود في App Store / Play Store** → خسارة قناة اكتشاف العملاء الأهم.
* ❌ **Push Notifications على iOS محدودة جداً** — والإشعارات هي محرك المبيعات المتكررة في الإيكومرس.
* ❌ لا وصول كامل للكاميرا/الملفات/Deep Linking بنفس الجودة.
* ✅ **لكن:** تكلفتها منخفضة جداً على مشروعكم (الموقع مسؤول أصلاً). **توصيتنا: نفّذها لاحقاً كمكسب سريع بعد التطبيق الأصلي، لا بدلاً منه.**

## 4.6 التوصية النهائية للمكدس التقني

```yaml
اللغة:              Dart 3.x (null-safe)
الإطار:             Flutter 3.2x+ (آخر Stable)
إدارة الحالة:        Riverpod 2.x  (بديل: BLoC لو الفريق يفضّله)
التنقل:             go_router  (يدعم Deep Links للطلبات والمنتجات)
شبكة:               dio + dio_smart_retry + pretty_dio_logger
النماذج:            freezed + json_serializable
تخزين آمن:          flutter_secure_storage (JWT + Refresh Token)
تخزين محلي:         hive / isar  (كاش الكتالوج + السلة أوف-لاين)
الصور:              cached_network_image
الإشعارات:          firebase_messaging + flutter_local_notifications
التحليلات:          firebase_analytics + firebase_crashlytics
الترجمة:            flutter_localizations + intl (ARB: ar, en)
الأرقام المالية:     decimal  ⚠️ إلزامي — لا تستخدم double للأسعار
الاختبارات:         flutter_test + mocktail + integration_test
CI/CD:              GitHub Actions + Fastlane (أو Codemagic)
```

---

# 5. الفجوات الحرجة في الـ API (Blockers)

> هذه الفجوات **تمنع** بناء تطبيق موبايل تجاري. يجب إغلاقها في المرحلة 1 قبل أي كود Flutter.

## 5.1 الفجوة 1: لا يوجد نظام حسابات للعملاء 🔴🔴🔴

**الحالة:** `AuthController` يمنع أي مستخدم ليس `Admin` (سطر 45-47). `SalesOrder` بلا `UserId`. لا يوجد `Register` ولا `ForgotPassword`.

**التأثير:** بدون هذا **لا توجد** شاشات: طلباتي، المفضلة، العناوين المحفوظة، ملفي الشخصي، إشعارات مخصصة. أي 60% من قيمة التطبيق.

**الحل المطلوب:**

1. **إضافة دور `Customer`** في الـ Seeder (بجانب `Admin` الموجود).
2. **توسيع `IdentityUser` → `AppUser`:**
   ```csharp
   public class AppUser : IdentityUser {
       public string? FullName { get; set; }
       public string? DefaultAddress { get; set; }
       public string? City { get; set; }
       public string? Governorate { get; set; }
       public string PreferredLanguage { get; set; } = "ar";
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   }
   ```
   ⚠️ يتطلب تغيير `AddIdentity<IdentityUser,…>` → `AddIdentity<AppUser,…>` في **كلا** المشروعين (`Web` + `Api`) + Migration.
3. **إضافة `UserId` (nullable) إلى `SalesOrder`** — nullable إلزامي للحفاظ على طلبات الضيوف التاريخية:
   ```csharp
   public string? UserId { get; set; }   // FK → AspNetUsers, nullable
   public AppUser? User { get; set; }
   ```
   مع `.OnDelete(DeleteBehavior.SetNull)` — حذف الحساب لا يمحو التاريخ المالي (متسق مع مبدأ I-01 في `AUDIT_REPORT.md`).
4. **فصل مسارات التوثيق:**
   * `/api/auth/customer/login` + `/register` → دور `Customer`
   * `/api/auth/admin/login` → دور `Admin` فقط (يحفظ السلوك الحالي)
5. **تفعيل الـ Lockout في الـ API:** تغيير `CheckPasswordSignInAsync(user, pwd, false)` → `true` لمطابقة حماية `FTD.Web`.

**تقدير الجهد:** 5-6 أيام عمل Backend (يشمل Migration + اختبارات + تحديث `FTD.Web` للنوع الجديد).

## 5.2 الفجوة 2: حجم الحمولة (Payload Bloat) 🔴🔴

**الحالة:** `/api/products` يرجّع `List<ProductDto>` كامل. كل عنصر يحمل: `DescAr` + `DescEn` (نصوص طويلة) + `Category` كامل + `Brand` كامل + `AttributeValues[]` (كل عنصر بداخله كائنان) + `Images[]` + `MetaTitle` + `MetaDesc` + `FeaturesJson`.

**التأثير المحسوب:** لو الـ DTO الواحد ~4KB ولديك 300 منتج → **~1.2 ميجابايت في استجابة واحدة**. على 3G مصري = 8-15 ثانية انتظار + استهلاك بيانات المستخدم + ضغط ذاكرة.

**الحل المطلوب:**

1. **إنشاء `ProductListItemDto` خفيف** (لشاشات القوائم فقط):
   ```csharp
   public class ProductListItemDto {
       public int Id; public string Slug;
       public string Name;            // مُحلّى حسب Accept-Language
       public string? ShortDesc;      // مقتطف قصير فقط
       public decimal Price; public decimal? OldPrice;
       public int? DiscountPercent;   // محسوب في الخادم — لا تحسبه في التطبيق
       public string? Badge; public string? Emoji;
       public string? ImageUrl;       // URL مطلق كامل
       public string? BrandName; public string? CategoryName;
       public bool InStock;           // بدل تسريب رقم المخزون الفعلي
       public bool IsFeatured;
   }
   ```
   → الحجم المتوقع **~350 بايت** بدل 4KB = **توفير ~90%**.
2. **`ProductDetailDto` الكامل** يُستخدم فقط في `/api/v1/products/{slug}`.
3. **⚠️ ملاحظة أمنية:** لا تُرجع `Stock` الرقمي للعميل (معلومة تجارية) — استخدم `InStock: bool` + `LowStock: bool` فقط.

**تقدير الجهد:** 2-3 أيام.

## 5.3 الفجوة 3: لا Pagination 🔴🔴

**الحالة:** لا يوجد `Skip()`/`Take()` في أي مسار قائمة (تحققنا: `Take` مستخدم فقط في `GetFeaturedAsync` و `GetRelatedAsync` و `SearchAsync(20)`).

**الحل المطلوب:** غلاف موحّد لكل القوائم:
```csharp
public class PagedResult<T> {
    public List<T> Items = new();
    public int Page;         // 1-based
    public int PageSize;     // افتراضي 20، أقصى 50 (منع DoS)
    public int TotalCount;
    public int TotalPages;
    public bool HasNext;
}
```
+ إضافة `GetPagedAsync(filter, page, pageSize)` في `IProductService`.
⚠️ **مهم:** يجب أن يكون الترتيب **حتمياً (deterministic)** — أضف `ThenBy(p => p.Id)` دائماً، وإلا تكرّرت/اختفت عناصر بين الصفحات (نفس درس الملاحظة I-25 في `AUDIT_REPORT.md`).

**تقدير الجهد:** 2 أيام.

## 5.4 الفجوة 4: لا Refresh Token 🔴🔴

**الحالة:** `ExpiryMinutes: 120` بلا تجديد → المستخدم يُطرد كل ساعتين.

**الحل المطلوب (النمط القياسي):**
* **Access Token:** JWT عمره **15 دقيقة** (تقليل نافذة الخطر).
* **Refresh Token:** سلسلة عشوائية 256-bit مخزنة **مُهشّرة (hashed)** في جدول جديد `RefreshTokens`، عمرها **30-60 يوماً**.
* **التدوير (Rotation):** كل استخدام يُبطل القديم ويصدر جديداً + كشف إعادة الاستخدام (reuse detection) → إبطال كل عائلة التوكن عند الاشتباه.
* **جدول جديد:**
  ```csharp
  public class RefreshToken {
      public int Id; public string UserId;
      public string TokenHash;        // ⚠️ SHA-256، لا تخزّن الأصل أبداً
      public DateTime ExpiresAt; public DateTime CreatedAt;
      public DateTime? RevokedAt; public string? ReplacedByHash;
      public string? DeviceInfo;      // لشاشة "الأجهزة النشطة"
  }
  ```
* نقاط نهاية: `POST /api/v1/auth/refresh` + `POST /api/v1/auth/logout`.
* **إصلاح `Encoding.ASCII` → `Encoding.UTF8`** في نفس المهمة.

**تقدير الجهد:** 3-4 أيام.

## 5.5 الفجوة 5: لا سلة على الخادم ولا مفضلة 🟠

**الحالة:** السلة في Session المتصفح فقط. لا يوجد كيان `Wishlist` إطلاقاً.

**الحل المطلوب:**
* **السلة:** استغلال تجريد `ICartStorage` العبقري — نكتب `DbCartStorage : ICartStorage` يقرأ/يكتب في جدول `UserCarts (UserId, CartJson, UpdatedAt)`.
  → **منطق `CartService` لا يتغير بحرف واحد.** هذا أجمل مكسب معماري في الخطة كلها.
* **السلة للضيف:** تبقى محلية في التطبيق (Hive) وتُدمج مع سلة الخادم عند تسجيل الدخول (`POST /api/v1/cart/merge`).
* **المفضلة:** كيان جديد `Wishlist (Id, UserId, ProductId, CreatedAt)` + فهرس فريد `(UserId, ProductId)`.

**تقدير الجهد:** 4 أيام.

## 5.6 الفجوة 6: لا تتبع طلب ولا "طلباتي" 🟠

**الحالة:** `GetOrderByIdAsync` موجودة لكن **معرّضة للأدمن فقط**. لا `GetByOrderNumberAsync`.

**الحل المطلوب:**
* `GET /api/v1/orders/track/{orderNumber}` — **عام**، لكن ⚠️ **يشترط تطابق آخر 4 أرقام من الهاتف** كـ query param، وإلا فهو **ثغرة IDOR**: أرقام الطلبات نمطها متوقع (`FTD{yyyyMMddHHmmss}{3 digits}`) فيمكن تخمينها وقراءة بيانات عملاء آخرين (اسم/هاتف/عنوان).
* `GET /api/v1/orders/my` — يتطلب JWT، مُصفّى بـ `UserId` **في الخادم** (لا تقبل `userId` من العميل أبداً).
* `POST /api/v1/orders/{id}/cancel` — إلغاء مسموح فقط في حالة "جديد"/"مؤكد".

**تقدير الجهد:** 3 أيام.

## 5.7 جدول ملخص الفجوات

| # | الفجوة | الخطورة | الجهد | يحجب الموبايل؟ |
|---|--------|---------|-------|----------------|
| G-1 | حسابات العملاء + `UserId` في الطلب | 🔴🔴🔴 | 5-6 يوم | ✅ نعم |
| G-2 | DTOs خفيفة للقوائم | 🔴🔴 | 2-3 يوم | ✅ نعم |
| G-3 | Pagination | 🔴🔴 | 2 يوم | ✅ نعم |
| G-4 | Refresh Token | 🔴🔴 | 3-4 يوم | ✅ نعم |
| G-5 | سلة خادم + مفضلة | 🟠 | 4 يوم | 🟡 جزئياً |
| G-6 | تتبع الطلب + طلباتي | 🟠 | 3 يوم | ✅ نعم |
| G-7 | Swagger/OpenAPI | 🟠 | 1 يوم | 🟡 (يسرّع كثيراً) |
| G-8 | URLs مطلقة للصور | 🟠 | 1 يوم | ✅ نعم |
| G-9 | Global Exception Handler + ProblemDetails | 🟠 | 1-2 يوم | 🟡 |
| G-10 | البحث + الاقتراحات | 🟡 | 2 يوم | ❌ |
| G-11 | Push Notifications (تسجيل الأجهزة) | 🟡 | 3 يوم | ❌ (المرحلة 4) |
| G-12 | الإعدادات/المحتوى/الصفحات في الـ API | 🟡 | 2 يوم | ❌ |
| | **الإجمالي** | | **~29-33 يوم عمل** | **≈ 6-7 أسابيع لمطور واحد** |

---

# 6. معمارية الـ Backend المستهدفة (API v2)

## 6.1 مبدأ ذهبي — لا نمسّ منطق الأعمال

```text
┌──────────────────────────────────────────────────────────────┐
│                     العملاء (Clients)                        │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐            │
│  │ FTD.Web    │   │ 📱 Flutter │   │ 📱 Flutter │            │
│  │ (MVC حالي) │   │  Android   │   │    iOS     │            │
│  └─────┬──────┘   └──────┬─────┘   └──────┬─────┘            │
└────────┼─────────────────┼────────────────┼──────────────────┘
         │                 └────────┬───────┘
         │                          ▼
         │            ┌──────────────────────────────┐
         │            │   FTD.Api  (REST /api/v1)    │  ← 🔨 نوسّعه
         │            │  Auth · Catalog · Cart ·     │
         │            │  Orders · Profile · Content  │
         │            └──────────────┬───────────────┘
         └───────────────────┬───────┘
                             ▼
              ┌────────────────────────────────┐
              │      FTD.Application          │  ← ✅ نضيف فقط، لا نغيّر
              │  6 خدمات + DTOs + العقود      │
              └───────────────┬────────────────┘
                              ▼
              ┌────────────────────────────────┐
              │     FTD.Infrastructure        │  ← 🔨 Migrations جديدة
              │  AppDbContext · DbCartStorage │
              └───────────────┬────────────────┘
                              ▼
              ┌────────────────────────────────┐
              │        FTD.Domain             │  ← 🔨 3 كيانات جديدة
              │  17 كياناً + AppUser +         │
              │  RefreshToken + Wishlist      │
              └────────────────────────────────┘
```

> ⚠️ **قاعدة حاكمة:** أي منطق جديد يُكتب في `FTD.Application` **حصراً** — لا في `FTD.Api`. لأن `FTD.Web` (المتجر الحالي) يجب أن يستفيد من نفس المنطق دون تكرار. هذا يحفظ Clean Architecture الذي بُني بعناية.

## 6.2 التغييرات المطلوبة لكل مشروع

| المشروع | التغييرات | حجم التغيير |
|--------|-----------|-------------|
| **FTD.Domain** | `AppUser`, `RefreshToken`, `Wishlist`, `UserCart`, `DeviceToken` + `SalesOrder.UserId` | 🟢 إضافات فقط |
| **FTD.Application** | `ProductListItemDto`, `PagedResult<T>`, `IAuthService`, `IWishlistService`, `IDeviceTokenService` + توسيع `IProductService`/`IOrderService` | 🟡 إضافات + 3 توسعات |
| **FTD.Infrastructure** | 4-5 Migrations + `DbCartStorage` + `JwtTokenService` + `RefreshTokenRepository` | 🟡 متوسط |
| **FTD.Api** | 6 متحكمات جديدة + Swagger + Versioning + ExceptionHandler + توسيع RateLimit | 🟠 كبير (الأغلبية هنا) |
| **FTD.Web** | تحديث نوع Identity من `IdentityUser` → `AppUser` فقط | 🟢 صغير لكن **إلزامي** |

## 6.3 قرارات معمارية محسومة (ADRs)

| القرار | الخيار المعتمد | السبب |
|-------|---------------|-------|
| **إصدارات الـ API** | `/api/v1/...` مع `Asp.Versioning.Mvc` | التطبيقات المنشورة في المتاجر **لا تُحدّث فوراً** — نسخة قديمة ستبقى تعمل شهوراً. الإصدارات إلزامية. |
| **مسارات الحالية** | نُبقي `/api/products` القديم يعمل (Deprecated) | عدم كسر أي تكامل قائم. |
| **التوطين** | ترويسة `Accept-Language` + إرجاع حقل موحّد | يقلل الحمولة ~40% وينظّف كود Flutter. |
| **الأخطاء** | `ProblemDetails` (RFC 7807) + `errorCode` ثابت | التطبيق يجب أن يفرّع على **كود ثابت** لا على نص عربي قابل للتغيير. |
| **الصور** | URL مطلق من الخادم + `?w=` للأحجام | التطبيق لا يعرف الـ Base URL؛ والأحجام المتعددة توفّر بيانات المستخدم. |
| **الأسعار** | `decimal` في C# → **string** في JSON | تجنّب فقدان الدقة في IEEE-754 عبر JS/Dart. اقرأها `Decimal.parse()` في Dart. |
| **`Stock`** | لا يُرسل للعميل — فقط `InStock`/`LowStock` | حماية معلومة تجارية. |
| **Idempotency** | ترويسة `Idempotency-Key` على `checkout` | الموبايل ينقطع/يعيد المحاولة → منع طلبات مكررة. **حرج جداً.** |
| **الدفع** | المرحلة 5، عبر Paymob/Fawry | البدء بـ COD (الموجود حالياً) — إدخال الدفع مبكراً يضاعف المخاطر. |
| **الكاش** | `ETag` + `Cache-Control` على الكتالوج | يخفض استهلاك البيانات والزمن بشكل هائل على شبكات الموبايل. |

## 6.4 عقد صيغة الاستجابة الموحد

**نجاح — عنصر واحد:** الكائن مباشرة.
**نجاح — قائمة:**
```json
{ "items": [...], "page": 1, "pageSize": 20,
  "totalCount": 137, "totalPages": 7, "hasNext": true }
```
**خطأ (RFC 7807 + كود ثابت):**
```json
{
  "type": "https://api.unishop.eg/errors/out-of-stock",
  "title": "الكمية غير متوفرة",
  "status": 400,
  "errorCode": "PRODUCT_OUT_OF_STOCK",
  "detail": "الكمية المطلوبة للمنتج (تابلت T30) غير متوفرة. المتاح: 2",
  "traceId": "00-a1b2c3...",
  "errors": { "items[0].quantity": ["الكمية تتجاوز المتاح"] }
}
```

### 📋 كتالوج أكواد الأخطاء الثابتة (يتفرّع التطبيق عليها)

| `errorCode` | HTTP | معنى للتطبيق |
|------------|------|-------------|
| `VALIDATION_FAILED` | 400 | إبراز الأخطاء على حقول النموذج |
| `INVALID_CREDENTIALS` | 401 | "بيانات الدخول غير صحيحة" |
| `TOKEN_EXPIRED` | 401 | 🔄 محاولة Refresh تلقائية ثم إعادة الطلب |
| `TOKEN_INVALID` | 401 | تسجيل خروج قسري |
| `ACCOUNT_LOCKED` | 423 | "الحساب مقفل مؤقتاً — حاول بعد 15 دقيقة" |
| `EMAIL_ALREADY_EXISTS` | 409 | "البريد مستخدم بالفعل" |
| `PRODUCT_NOT_FOUND` | 404 | إزالة العنصر من السلة تلقائياً |
| `PRODUCT_INACTIVE` | 400 | "المنتج غير متاح حالياً" |
| `PRODUCT_OUT_OF_STOCK` | 400 | إظهار المتاح + تعديل الكمية |
| `CART_EMPTY` | 400 | تحويل لشاشة الكتالوج |
| `ORDER_NOT_FOUND` | 404 | "لم نجد هذا الطلب" |
| `ORDER_NOT_CANCELLABLE` | 409 | "لا يمكن إلغاء الطلب في حالته الحالية" |
| `RATE_LIMITED` | 429 | إعادة محاولة بـ backoff + احترام `Retry-After` |
| `SERVER_ERROR` | 500 | شاشة خطأ عامة + إبلاغ Crashlytics |
| `MAINTENANCE` | 503 | شاشة صيانة |

---

# 7. عقد الـ API الكامل المطلوب

## 7.1 قاعدة التوطين (Localization Contract)

كل طلب من التطبيق يحمل: `Accept-Language: ar` أو `en`.
الخادم يقرأها في `LocalizationMiddleware` ويرجّع **حقلاً موحداً** (`name`) بدل حقلين (`nameAr`/`nameEn`).

```json
// بدلاً من:  { "nameAr": "تابلت T30", "nameEn": "T30 Tablet", ... }
// نرجّع:     { "name": "تابلت T30" }        ← مع Accept-Language: ar
```
**المكسب:** تقليل الحمولة ~40% + كود Flutter بلا `if (lang == 'ar')` في كل widget.

## 7.2 الجدول الكامل — 45 نقطة نهاية

### 🔓 أ. التوثيق والحسابات (Auth) — جديد بالكامل

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 1 | POST | `/api/v1/auth/register` | عام + RateLimit | 🆕 جديد | P0 |
| 2 | POST | `/api/v1/auth/login` | عام + RateLimit(5/30s) | 🔨 تعديل (السماح لـ Customer) | P0 |
| 3 | POST | `/api/v1/auth/refresh` | عام (بـ RefreshToken) | 🆕 جديد | P0 |
| 4 | POST | `/api/v1/auth/logout` | JWT | 🆕 جديد | P0 |
| 5 | POST | `/api/v1/auth/forgot-password` | عام + RateLimit | 🆕 جديد | P1 |
| 6 | POST | `/api/v1/auth/reset-password` | عام | 🆕 جديد | P1 |
| 7 | POST | `/api/v1/auth/change-password` | JWT | 🆕 جديد | P1 |
| 8 | DELETE | `/api/v1/auth/account` | JWT | 🆕 جديد | **P0** ⚠️ |

> ⚠️ **البند 8 ليس اختيارياً:** سياسات Apple App Store **تُلزم** بوجود حذف الحساب من داخل التطبيق لأي تطبيق يسمح بإنشاء حساب. غيابه = **رفض مؤكد** من App Review.

### 🛍️ ب. الكتالوج (Catalog)

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 9 | GET | `/api/v1/products?page&pageSize&categoryId&brandId&q&sort&minPrice&maxPrice&av=` | عام | 🔨 + Pagination + DTO خفيف | P0 |
| 10 | GET | `/api/v1/products/{slug}` | عام | ✅ موجود (تحسين طفيف) | P0 |
| 11 | GET | `/api/v1/products/featured` | عام | 🆕 (الخدمة موجودة) | P0 |
| 12 | GET | `/api/v1/products/{id}/related` | عام | 🆕 (الخدمة موجودة) | P1 |
| 13 | GET | `/api/v1/categories` | عام | 🔨 نقل من `/products/categories` | P0 |
| 14 | GET | `/api/v1/brands` | عام | 🔨 نقل من `/products/brands` | P0 |
| 15 | GET | `/api/v1/categories/{id}/attributes` | عام | 🆕 (الخدمة موجودة) | P1 |
| 16 | GET | `/api/v1/search?q=&page=` | عام | 🆕 (الخدمة موجودة) | P1 |
| 17 | GET | `/api/v1/search/suggest?q=` | عام | 🆕 جديد | P2 |
| 18 | GET | `/api/v1/home` | عام | 🆕 **مُجمّع** ⭐ | P0 |

> ⭐ **البند 18 هو أهم تحسين أداء في الخطة كلها.** بدلاً من أن يطلق التطبيق 5 طلبات متسلسلة عند الإقلاع (banner + categories + featured + brands + settings)، يطلق **طلباً واحداً** يرجّع كل شيء. على شبكة موبايل بـ RTT عالي (200-400ms) هذا يوفّر **~1.5 ثانية** من زمن أول شاشة. + مع `ETag` يصبح التحديث اللاحق بلا حمولة تقريباً.

### 🛒 ج. السلة (Cart)

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 19 | GET | `/api/v1/cart` | JWT | 🆕 (عبر `DbCartStorage`) | P1 |
| 20 | POST | `/api/v1/cart/items` | JWT | 🆕 | P1 |
| 21 | PUT | `/api/v1/cart/items/{productId}` | JWT | 🆕 | P1 |
| 22 | DELETE | `/api/v1/cart/items/{productId}` | JWT | 🆕 | P1 |
| 23 | DELETE | `/api/v1/cart` | JWT | 🆕 | P1 |
| 24 | POST | `/api/v1/cart/merge` | JWT | 🆕 (دمج سلة الضيف) | P1 |
| 25 | POST | `/api/v1/cart/validate` | عام | 🆕 ⭐ | P0 |

> ⭐ **البند 25 حرج:** قبل شاشة الدفع، التطبيق يرسل سلته المحلية ليتحقق الخادم من: الأسعار الحالية (قد تغيّرت)، التوفر، حالة `IsActive`، والشحن. يمنع مفاجأة "المنتج غير متاح" **بعد** إدخال العميل بياناته كلها.

### 📦 د. الطلبات (Orders)

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 26 | POST | `/api/v1/orders/checkout` | عام أو JWT + `Idempotency-Key` | 🔨 + ربط `UserId` + Idempotency | P0 |
| 27 | GET | `/api/v1/orders/my?page=` | JWT | 🆕 | P0 |
| 28 | GET | `/api/v1/orders/{id}` | JWT (ملكية مُتحقّقة) | 🆕 | P0 |
| 29 | GET | `/api/v1/orders/track/{orderNumber}?phone4=` | عام (مقيّد) | 🆕 | P0 |
| 30 | POST | `/api/v1/orders/{id}/cancel` | JWT | 🆕 | P1 |
| 31 | POST | `/api/v1/orders/{id}/reorder` | JWT | 🆕 | P2 |
| 32 | GET | `/api/v1/orders/statuses` | عام | 🆕 (الخدمة موجودة) | P1 |

### 👤 هـ. الملف الشخصي والمفضلة

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 33 | GET | `/api/v1/me` | JWT | 🆕 | P0 |
| 34 | PUT | `/api/v1/me` | JWT | 🆕 | P1 |
| 35 | GET | `/api/v1/me/addresses` | JWT | 🆕 | P2 |
| 36 | POST | `/api/v1/me/addresses` | JWT | 🆕 | P2 |
| 37 | GET | `/api/v1/wishlist` | JWT | 🆕 | P1 |
| 38 | POST | `/api/v1/wishlist/{productId}` | JWT | 🆕 | P1 |
| 39 | DELETE | `/api/v1/wishlist/{productId}` | JWT | 🆕 | P1 |

### 📄 و. المحتوى والإعدادات

| # | Method | Endpoint | الحماية | الحالة | الأولوية |
|---|--------|----------|---------|--------|---------|
| 40 | GET | `/api/v1/content/pages/{slug}` | عام | 🆕 (الخدمة موجودة) | P1 |
| 41 | GET | `/api/v1/content/contact-info` | عام | 🆕 (الخدمة موجودة) | P1 |
| 42 | GET | `/api/v1/settings/public` | عام | 🆕 (شحن + ألوان + نسخة) | P0 |
| 43 | POST | `/api/v1/contact` | عام + RateLimit | ✅ موجود | P1 |
| 44 | POST | `/api/v1/devices/register` | JWT أو عام | 🆕 (FCM Token) | P2 |
| 45 | GET | `/api/v1/app/config` | عام | 🆕 ⭐ (نسخة أدنى + صيانة) | P0 |

> ⭐ **البند 45 (Force Update) لا يُستهان به:** يرجّع `minSupportedVersion` + `maintenanceMode` + `updateUrl`. لماذا P0؟ لأنك لو نشرت نسخة بها خطأ حرج (مثلاً حساب سعر خطأ)، هذا الـ endpoint هو **وسيلتك الوحيدة** لإجبار المستخدمين على التحديث. بدونه ستنتظر أسابيع حتى يحدّث الناس طوعاً.

## 7.3 أمثلة عقود JSON فعلية

<details>
<summary><b>POST /api/v1/auth/login</b></summary>

```jsonc
// Request
{ "email": "ahmed@example.com", "password": "P@ssw0rd123",
  "deviceInfo": "Samsung SM-A536 / Android 14" }

// 200 OK
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "9f2b7c1e-...",
  "expiresIn": 900,                     // ثواني (15 دقيقة)
  "tokenType": "Bearer",
  "user": { "id": "a1b2...", "email": "ahmed@example.com",
            "fullName": "أحمد محمد", "roles": ["Customer"],
            "preferredLanguage": "ar" }
}

// 401 → errorCode: "INVALID_CREDENTIALS"
// 423 → errorCode: "ACCOUNT_LOCKED"
// 429 → errorCode: "RATE_LIMITED" + ترويسة Retry-After
```
</details>

<details>
<summary><b>GET /api/v1/products?page=1&pageSize=20&categoryId=3</b></summary>

```jsonc
{
  "items": [{
    "id": 12, "slug": "doogee-t30-ultra",
    "name": "تابلت DOOGEE T30 Ultra",
    "shortDesc": "شاشة 12 بوصة · 12GB RAM",
    "price": "8999.00",              // ⚠️ string للحفاظ على الدقة
    "oldPrice": "10499.00",
    "discountPercent": 14,           // محسوب في الخادم
    "badge": "HOT", "emoji": "📱",
    "imageUrl": "https://cdn.unishop.eg/images/products/x.webp",
    "brandName": "DOOGEE", "categoryName": "تابلتات",
    "inStock": true, "isFeatured": true
  }],
  "page": 1, "pageSize": 20, "totalCount": 137,
  "totalPages": 7, "hasNext": true
}
// ترويسات: ETag: "W/abc123"  ·  Cache-Control: public, max-age=120
```
</details>

<details>
<summary><b>GET /api/v1/home  ⭐ (المُجمّع)</b></summary>

```jsonc
{
  "banners": [{ "imageUrl": "...", "title": "...", "targetType": "category", "targetSlug": "tablets" }],
  "categories": [{ "id": 1, "name": "تابلتات", "slug": "tablets", "emoji": "📱", "imageUrl": "...", "productsCount": 24 }],
  "brands": [{ "id": 1, "name": "DOOGEE", "slug": "doogee", "logoUrl": "..." }],
  "featuredProducts": [ /* ProductListItemDto × 8 */ ],
  "newArrivals": [ /* × 8 */ ],
  "onSale": [ /* × 8 */ ],
  "settings": { "shippingFee": "150.00", "freeShippingAbove": "5000.00",
                "currency": "EGP", "primaryColor": "#1A6BFF" }
}
```
</details>

<details>
<summary><b>POST /api/v1/orders/checkout</b></summary>

```jsonc
// Headers: Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
//          Authorization: Bearer ... (اختياري — الضيف مسموح)
// Request
{
  "customerName": "أحمد محمد", "customerPhone": "01012345678",
  "customerEmail": "ahmed@example.com",
  "address": "12 شارع النيل، المعادي", "city": "القاهرة",
  "governorate": "القاهرة", "notes": "الاتصال قبل التوصيل",
  "paymentMethod": "cod",
  "items": [{ "productId": 12, "quantity": 1 }]
}

// 201 Created
{ "success": true, "orderId": 481, "orderNumber": "FTD20260906143022517",
  "subTotal": "8999.00", "shippingFee": "0.00", "total": "8999.00",
  "estimatedDelivery": "2026-09-09", "status": { "id": 1, "name": "جديد", "colorHex": "#1A6BFF" } }

// 400 → errorCode: "PRODUCT_OUT_OF_STOCK" (المنطق موجود بالفعل في OrderService)
```
</details>

## 7.4 الحد من المعدل (Rate Limiting) الموسّع

الحالي: سياسة واحدة على الدخول. المطلوب:

| السياسة | النطاق | الحد |
|--------|--------|------|
| `auth-login` | لكل IP + بريد | 5 / 30 ثانية *(موجودة)* |
| `auth-register` | لكل IP | 3 / 10 دقائق |
| `auth-forgot` | لكل IP + بريد | 3 / ساعة |
| `checkout` | لكل IP / مستخدم | 10 / دقيقة |
| `contact` | لكل IP | 3 / 10 دقائق |
| `search` | لكل IP | 30 / دقيقة |
| `global-read` | لكل IP | 300 / دقيقة |

## 7.5 معالجة الصور — مطلوب حسمها

**المشكلة:** `MappingExtensions` يرجّع `ImagePath` نسبياً (`/images/products/x.jpg`)، ووسائط الصور تُخزَّن في `wwwroot` الخاص بـ **FTD.Web** لا `FTD.Api`. لو نُشر الـ API على نطاق منفصل، **الصور لن تظهر في التطبيق**.

**الخيارات:**

| # | الحل | التقييم |
|---|------|---------|
| 1 | إعداد `Media:BaseUrl` في `appsettings` والـ API يبني URL مطلقاً | 🟢 **الأسرع — نبدأ به** |
| 2 | خدمة صور (Cloudflare Images / imgproxy) بأحجام متعددة `?w=400` | 🟢 **الأفضل — المرحلة 3** |
| 3 | نقل الوسائط إلى تخزين مشترك (S3/Azure Blob) + CDN | 🟢 الأمثل للتوسع |

**التوصية:** الخيار 1 في المرحلة 1 (يوم عمل) → الخيار 2 في المرحلة 3.
⚠️ لا تتجاهل هذا: صور بحجم كامل (1-2MB) على شبكة موبايل ستقتل التجربة. **`.webp` بعرض 400px للقوائم يقلل الحجم ~85%.**

---

# 8. معمارية تطبيق Flutter المقترحة

## 8.1 نمط المعمارية — Feature-First + Clean

نطبّق **نفس فلسفة الـ Backend** (Clean Architecture) لكن منظّمة بالميزة (Feature-First) — أسهل صيانة وأسهل توزيع مهام بين مطورين.

```text
lib/
├── main.dart
├── app.dart                          # MaterialApp + Router + Theme
│
├── core/                             # ═══ المشترك بين كل الميزات ═══
│   ├── config/
│   │   ├── env.dart                  # dev / staging / prod (--dart-define)
│   │   └── app_config.dart
│   ├── network/
│   │   ├── api_client.dart           # Dio + BaseOptions
│   │   ├── auth_interceptor.dart     # ⭐ إرفاق التوكن + Refresh تلقائي
│   │   ├── error_interceptor.dart    # ProblemDetails → AppException
│   │   ├── locale_interceptor.dart   # Accept-Language تلقائي
│   │   └── connectivity_service.dart
│   ├── error/
│   │   ├── app_exception.dart        # يطابق كتالوج errorCode
│   │   └── failure.dart
│   ├── storage/
│   │   ├── secure_storage.dart       # JWT + Refresh
│   │   └── local_cache.dart          # Hive
│   ├── theme/
│   │   ├── app_theme.dart            # مستخرج من site.css (نفس الألوان)
│   │   ├── app_colors.dart           # #1A6BFF ← من SiteSetting
│   │   └── app_typography.dart       # Cairo / Tajawal
│   ├── l10n/
│   │   ├── app_ar.arb  ·  app_en.arb
│   └── widgets/                      # مكتبة مكونات مشتركة
│       ├── product_card.dart         # مطابق لـ _ProductCard.cshtml
│       ├── app_button.dart · price_tag.dart
│       ├── shimmer_loader.dart · empty_state.dart
│       └── error_view.dart · network_image_x.dart
│
├── features/                         # ═══ الميزات ═══
│   ├── splash/          · onboarding/
│   ├── home/
│   │   ├── data/     (home_repository.dart, home_api.dart)
│   │   ├── domain/   (home_data.dart — freezed)
│   │   └── presentation/ (home_screen.dart, widgets/, home_provider.dart)
│   ├── catalog/         # قائمة + فلاتر + بحث
│   ├── product_detail/  # معرض صور + مواصفات + ذات صلة
│   ├── cart/            # سلة محلية + مزامنة
│   ├── checkout/        # نموذج + تأكيد
│   ├── orders/          # طلباتي + تتبع + تفاصيل
│   ├── auth/            # دخول + تسجيل + استعادة
│   ├── profile/         # الملف + العناوين + الإعدادات
│   ├── wishlist/
│   ├── content/         # الصفحات الحرة + اتصل بنا
│   └── notifications/
│
└── routing/
    ├── app_router.dart               # go_router + حماية المسارات
    └── routes.dart
```

## 8.2 تدفق البيانات (Data Flow)

```text
Widget → ref.watch(provider) → Notifier/Controller
                                     ↓
                              Repository (منطق + كاش)
                                ↓          ↓
                        ApiClient      LocalCache (Hive)
                          (Dio)        ↓
                            ↓      أوف-لاين / تحديث فوري
                        FTD.Api
```

**سياسة الكاش (Stale-While-Revalidate):** اعرض المخزّن **فوراً** (شاشة بلا انتظار) ثم حدّث من الشبكة في الخلفية. هذا يجعل التطبيق يبدو فورياً حتى على شبكة بطيئة.

## 8.3 ⭐ أهم مكوّن تقني: `AuthInterceptor`

هذا الملف هو أكثر مكان تحدث فيه أخطاء في تطبيقات الموبايل. المنطق الصحيح:

```dart
// المنطق (شبه-كود):
onRequest:  إذا كان المسار محمياً → أرفق Bearer {accessToken}
onError(401 && errorCode == 'TOKEN_EXPIRED'):
    1. إذا كانت هناك عملية Refresh جارية → انتظرها (لا تطلق عمليتين)  ⚠️
    2. وإلا: اقفل mutex، نادِ /auth/refresh بالـ refreshToken
    3. نجح؟  → خزّن التوكنين الجديدين، أعد إرسال الطلب الأصلي
    4. فشل؟  → امسح التخزين الآمن، حوّل لشاشة الدخول، أبطل الطلبات المعلّقة
```

⚠️ **الفخ الشائع:** لو 5 طلبات فشلت بـ 401 معاً (شاشة الرئيسية تطلق عدة طلبات)، سيطلق التطبيق 5 عمليات Refresh متزامنة → **تدوير التوكن في الخادم سيبطل العائلة كلها** ويُطرد المستخدم. **الحل إلزامي: mutex/قائمة انتظار حول عملية Refresh.**

## 8.4 استراتيجية العمل بلا اتصال (Offline)

| الوظيفة | السلوك أوف-لاين |
|--------|----------------|
| تصفح الكتالوج | 🟢 من كاش Hive (آخر تحديث) + شعار "بيانات غير محدّثة" |
| تفاصيل منتج | 🟢 مخزّن لو تم زيارته سابقاً |
| السلة | 🟢 محلية بالكامل، تُزامن عند العودة |
| المفضلة | 🟡 قائمة انتظار عمليات (Outbox) تُنفّذ عند العودة |
| إتمام الطلب | 🔴 يتطلب اتصالاً + رسالة واضحة |
| طلباتي | 🟢 آخر نسخة مخزّنة + شعار |

## 8.5 الهوية البصرية — استخراجها من الموقع القائم

المشروع بلا Design System رسمي؛ الهوية موزّعة في `site.css` (4464 سطر) وقابلة للتخصيص من قاعدة البيانات (`SiteSetting: theme.primary = #1A6BFF`).

**المهمة (يوم عمل، في المرحلة 2):**
1. استخراج متغيرات CSS (`:root { --primary: ... }`) → `app_colors.dart`.
2. توحيد المقاسات (المسافات، أنصاف الأقطار، الظلال) → `app_dimens.dart`.
3. **جلب اللون الأساسي من `/api/v1/settings/public` عند الإقلاع** → الأدمن يغيّر لون المتجر فيتغير في التطبيق **بلا تحديث من المتجر**. 🎯 لمسة احترافية بجهد بسيط.
4. تحويل `_ProductCard.cshtml` إلى `ProductCard` widget مطابق.

---

# 9. خريطة شاشات التطبيق

## 9.1 الشاشات — 28 شاشة موزّعة على المراحل

| # | الشاشة | المصدر في الويب | الـ Endpoints | المرحلة |
|---|-------|-----------------|--------------|--------|
| 1 | Splash | — | `/app/config` | M2 |
| 2 | Onboarding (3 صفحات) | — | — | M4 |
| 3 | **الرئيسية** | `Views/Home/Index.cshtml` | `/home` | M2 |
| 4 | كل التصنيفات | Navbar dropdown | `/categories` | M2 |
| 5 | كل الماركات | `_Section` | `/brands` | M2 |
| 6 | **قائمة المنتجات** (Grid + Infinite Scroll) | `Views/Products/Index.cshtml` | `/products` | M2 |
| 7 | ورقة الفلاتر (Bottom Sheet) | Sidebar filters | `/categories/{id}/attributes` | M2 |
| 8 | البحث + السجل + الاقتراحات | `?q=` | `/search`, `/search/suggest` | M2 |
| 9 | **تفاصيل المنتج** (معرض + مواصفات + ذات صلة) | `Views/Products/Detail.cshtml` | `/products/{slug}`, `/related` | M2 |
| 10 | **السلة** | `Views/Cart/Index.cshtml` | `/cart`, `/cart/validate` | M2 |
| 11 | **إتمام الطلب** (نموذج) | `Views/Order/Checkout.cshtml` | `/orders/checkout` | M2 |
| 12 | تأكيد الطلب (نجاح) | `Views/Order/Confirmation.cshtml` | — | M2 |
| 13 | تسجيل الدخول | `Admin/Account/Login` (للأدمن) | `/auth/login` | M3 |
| 14 | إنشاء حساب | — | `/auth/register` | M3 |
| 15 | نسيت كلمة المرور | — | `/auth/forgot-password` | M3 |
| 16 | تعيين كلمة مرور جديدة | — | `/auth/reset-password` | M3 |
| 17 | **طلباتي** (قائمة) | `Admin/Orders/Index` (للأدمن) | `/orders/my` | M3 |
| 18 | تفاصيل الطلب + مُتتبِّع الحالة | `Admin/Orders/Detail` | `/orders/{id}` | M3 |
| 19 | تتبع طلب (للضيف — بلا حساب) | — | `/orders/track/{no}` | M3 |
| 20 | ملفي الشخصي | — | `/me` | M3 |
| 21 | تعديل الملف | — | `PUT /me` | M3 |
| 22 | العناوين المحفوظة | — | `/me/addresses` | M4 |
| 23 | **المفضلة** | — | `/wishlist` | M3 |
| 24 | الإعدادات (اللغة/الثيم/الإشعارات) | — | محلي | M3 |
| 25 | الإشعارات (قائمة) | — | محلي + FCM | M4 |
| 26 | صفحة محتوى (شروط/خصوصية/عنا) | `Views/Page/Show.cshtml` | `/content/pages/{slug}` | M3 |
| 27 | اتصل بنا | `Home` contact form | `/content/contact-info`, `POST /contact` | M3 |
| 28 | حذف الحساب ⚠️ | — | `DELETE /auth/account` | M3 |

## 9.2 هيكل التنقل (Navigation)

```text
Bottom Navigation Bar (5 تبويبات — RTL: من اليمين لليسار)
├── 🏠 الرئيسية      → Home
├── 🗂️ التصنيفات     → Categories → Products
├── 🛒 السلة  (Badge) → Cart → Checkout → Confirmation
├── ❤️ المفضلة       → Wishlist
└── 👤 حسابي         → Profile / Login → Orders · Addresses · Settings

Deep Links (go_router):
  unishop://product/{slug}      ← من إشعار/رابط مشاركة
  unishop://order/{orderNumber} ← من إشعار "طلبك شُحن"
  unishop://category/{slug}
  https://unishop.eg/...        ← App Links / Universal Links
```

## 9.3 أولوية الشاشات لـ MVP

**MVP (11 شاشة قابلة للنشر):** 1، 3، 4، 6، 7، 8، 9، 10، 11، 12، 19
→ متجر كامل قابل للاستخدام **كضيف** بلا حسابات. يمكن نشره في المتاجر وبدء جمع الطلبات.

**V1.0 (المتبقي):** 13-18، 20-28.

---

# 10. خطة التنفيذ بالمراحل والأسابيع

> **مدة إجمالية: 12 أسبوعاً للـ MVP القابل للنشر · 16-18 أسبوعاً للنسخة الكاملة.**
> الفريق المفترض: **1 Backend (.NET) + 1 Flutter + 0.5 QA + 0.5 مصمم**.

## 🔰 المرحلة 0 — التمهيد والحسم (الأسبوع 1)

**الهدف:** إغلاق كل القرارات المفتوحة وتجهيز البنية قبل كتابة كود المنتج.

| # | المهمة | المسؤول | أيام |
|---|-------|---------|------|
| 0.1 | **إضافة Swashbuckle/Swagger + OpenAPI** إلى `FTD.Api` — العقد يصبح مولّداً من الكود | Backend | 1 |
| 0.2 | إضافة `Asp.Versioning.Mvc` وتحويل المسارات إلى `/api/v1` (مع الحفاظ على القديم Deprecated) | Backend | 1 |
| 0.3 | **إخراج `JwtSettings:Secret` إلى متغير بيئة/User-Secrets** + تدوير المفتاح + `ASCII → UTF8` | Backend | 0.5 |
| 0.4 | `GlobalExceptionHandler` + `ProblemDetails` + كتالوج `errorCode` | Backend | 1.5 |
| 0.5 | إعداد بيئة Staging للـ API (نطاق + HTTPS + شهادة) | DevOps | 1 |
| 0.6 | حسم استراتيجية الصور (`Media:BaseUrl`) + إعدادات CORS للتطبيق | Backend | 0.5 |
| 0.7 | إنشاء مشروع Flutter + هيكل المجلدات + CI أولي (build فقط) | Flutter | 1 |
| 0.8 | ورشة تصميم: تحويل هوية `site.css` → Design Tokens | مصمم + Flutter | 1.5 |

**✅ معيار الإنجاز:** Swagger UI يعرض 13 نقطة نهاية على Staging · التطبيق الفارغ يعمل على جهازي Android و iOS · مستند Design Tokens معتمد.

---

## 🔧 المرحلة 1 — توسيع الـ Backend (الأسابيع 2-5) 🔴 **الأهم**

**الهدف:** إغلاق كل الفجوات الحاجبة (G-1 … G-9). **لا كود Flutter منتِج هنا** — Flutter يعمل على الهياكل والمكونات فقط.

### أسبوع 2 — الحسابات والهوية

| # | المهمة | أيام |
|---|-------|------|
| 1.1 | إنشاء `AppUser : IdentityUser` + تحويل `AddIdentity<>` في `Web` و `Api` | 1 |
| 1.2 | Migration `AddAppUserProfile` + Seeder لدور `Customer` | 0.5 |
| 1.3 | Migration `AddUserIdToSalesOrder` (nullable + `DeleteBehavior.SetNull`) | 0.5 |
| 1.4 | `IAuthService` في `FTD.Application` (Register / Login / ChangePassword / DeleteAccount) | 1.5 |
| 1.5 | فصل `AuthController` → `customer/login` + `admin/login` + تفعيل الـ Lockout | 1 |
| 1.6 | اختبارات وحدة للتوثيق | 0.5 |

### أسبوع 3 — التوكن والأمن

| # | المهمة | أيام |
|---|-------|------|
| 1.7 | كيان `RefreshToken` + Migration + فهارس | 0.5 |
| 1.8 | `JwtTokenService` + تدوير التوكن + كشف إعادة الاستخدام | 2 |
| 1.9 | `/auth/refresh` + `/auth/logout` + `/auth/logout-all` | 1 |
| 1.10 | Forgot/Reset Password عبر `IEmailService` الموجود | 1 |
| 1.11 | `DELETE /auth/account` (شرط Apple) — مع إخفاء الهوية لا الحذف الصلب للطلبات ⚠️ | 0.5 |

### أسبوع 4 — الكتالوج والأداء

| # | المهمة | أيام |
|---|-------|------|
| 1.12 | `ProductListItemDto` + `PagedResult<T>` + `GetPagedAsync` (ترتيب حتمي!) | 2 |
| 1.13 | `/api/v1/home` المُجمّع + كاش IMemoryCache (60-120 ثانية) | 1 |
| 1.14 | نقل `categories`/`brands` لمسارات مستقلة + `/categories/{id}/attributes` | 1 |
| 1.15 | `LocalizationMiddleware` (`Accept-Language`) + `ToDto(lang)` | 1 |
| 1.16 | URLs مطلقة للصور + دعم `?w=` للأحجام | 0.5 |
| 1.17 | `ETag` + `Cache-Control` على مسارات الكتالوج | 0.5 |

### أسبوع 5 — الطلبات والسلة

| # | المهمة | أيام |
|---|-------|------|
| 1.18 | `DbCartStorage : ICartStorage` + `UserCart` + Migration + `/cart/*` | 2 |
| 1.19 | `/cart/merge` + `/cart/validate` | 1 |
| 1.20 | `Wishlist` + Migration + `/wishlist/*` | 1 |
| 1.21 | `/orders/my` + `/orders/{id}` (تحقق ملكية) + `/orders/track` (مع `phone4`) | 1.5 |
| 1.22 | `Idempotency-Key` على `checkout` + ربط `UserId` + `/orders/{id}/cancel` | 1.5 |
| 1.23 | توسيع Rate Limiting (7 سياسات) + `/settings/public` + `/app/config` | 1 |
| 1.24 | **اختبارات تكامل للـ API** (WebApplicationFactory) — الحد الأدنى 40 اختباراً | 2 |

**✅ معيار الإنجاز للمرحلة 1:**
* [ ] Swagger يعرض **45 نقطة نهاية** موثقة وقابلة للتجربة
* [ ] `dotnet build` = **0 أخطاء / 0 تحذيرات** (نفس معيار `AUDIT_REPORT.md`)
* [ ] `FTD.Web` (الموقع الحالي) **يعمل بلا أي انحدار** بعد تغيير Identity ⚠️ حرج
* [ ] كل الـ Migrations تعمل صعوداً وهبوطاً (`Up`/`Down`)
* [ ] Postman Collection كامل + مثال لكل حالة خطأ
* [ ] اختبار حمل: `/api/v1/home` تحت 200ms p95

---

## 📱 المرحلة 2 — تطبيق Flutter الأساسي / MVP (الأسابيع 4-9، بالتوازي جزئياً)

> يبدأ Flutter من الأسبوع 4 على الشاشات التي اكتمل الـ API الخاص بها.

### أسبوع 4-5 — الأساس (بالتوازي مع Backend)

| # | المهمة | أيام |
|---|-------|------|
| 2.1 | طبقة الشبكة: Dio + 3 Interceptors + معالجة الأخطاء | 2 |
| 2.2 | توليد النماذج (freezed) من عقد Swagger | 2 |
| 2.3 | الثيم + الخطوط العربية (Cairo/Tajawal) + RTL كامل | 2 |
| 2.4 | التوطين (ARB: ar/en) + مبدّل اللغة | 1 |
| 2.5 | التنقل (go_router) + Bottom Nav + الهياكل الفارغة | 1.5 |
| 2.6 | مكتبة المكونات المشتركة (ProductCard, Shimmer, EmptyState, ErrorView, PriceTag) | 2.5 |

### أسبوع 6-7 — الكتالوج والتسوق

| # | المهمة | أيام |
|---|-------|------|
| 2.7 | الشاشة الرئيسية (كاروسيل + تصنيفات + مميز + عروض) | 3 |
| 2.8 | قائمة المنتجات + **Infinite Scroll** + الترتيب | 2.5 |
| 2.9 | ورقة الفلاتر (تصنيف/ماركة/سعر/مواصفات ديناميكية) | 2.5 |
| 2.10 | البحث + سجل البحث + الاقتراحات | 1.5 |
| 2.11 | تفاصيل المنتج (معرض صور + مواصفات + `FeaturesJson` + ذات صلة) | 3 |
| 2.12 | كاش Hive للكتالوج + Stale-While-Revalidate | 1.5 |

### أسبوع 8-9 — السلة والطلب

| # | المهمة | أيام |
|---|-------|------|
| 2.13 | السلة المحلية (Hive) + إضافة/تعديل/حذف + حساب المجموع | 2.5 |
| 2.14 | حساب الشحن + منطق الشحن المجاني (من `/settings/public`) | 1 |
| 2.15 | `/cart/validate` قبل الدفع + معالجة تغيّر الأسعار/التوفر | 1.5 |
| 2.16 | نموذج إتمام الطلب + التحقق (هاتف مصري، محافظات) + `Idempotency-Key` | 3 |
| 2.17 | شاشة التأكيد + مشاركة رقم الطلب | 1 |
| 2.18 | تتبع الطلب للضيف | 1.5 |
| 2.19 | حالات فراغ/خطأ/انقطاع شبكة لكل شاشة | 2 |

**✅ معيار الإنجاز للمرحلة 2 (MVP):**
* [ ] رحلة كاملة تعمل: الرئيسية → تصنيف → منتج → سلة → دفع → تأكيد → تتبع
* [ ] RTL سليم 100% في كل الشاشات + تبديل ar/en بلا إعادة تشغيل
* [ ] يعمل على Android 8+ و iOS 13+
* [ ] لا تجمّد (jank) في التمرير — ثبات 60fps
* [ ] زمن أول شاشة < 2.5 ثانية على 4G

---

## 👤 المرحلة 3 — الحسابات والولاء (الأسابيع 10-12)

| # | المهمة | أيام |
|---|-------|------|
| 3.1 | شاشات الدخول/التسجيل + التحقق + معالجة الأخطاء | 3 |
| 3.2 | التخزين الآمن + التجديد التلقائي + استعادة الجلسة عند الإقلاع | 2 |
| 3.3 | نسيت/تعيين كلمة المرور | 1.5 |
| 3.4 | دمج السلة عند الدخول (`/cart/merge`) — انتبه للتعارضات | 1.5 |
| 3.5 | طلباتي + تفاصيل + مُتتبِّع حالة بصري (بألوان `OrderStatus.ColorHex`) | 3 |
| 3.6 | إلغاء الطلب + إعادة الطلب | 1.5 |
| 3.7 | المفضلة + مزامنة Outbox | 2 |
| 3.8 | الملف الشخصي + التعديل + **حذف الحساب** ⚠️ | 2 |
| 3.9 | الإعدادات (لغة/ثيم/إشعارات) | 1 |
| 3.10 | صفحات المحتوى (Renderer لأقسام `PageSection` الديناميكية) | 2 |
| 3.11 | اتصل بنا + روابط سوشيال + واتساب + خريطة | 1.5 |

**✅ معيار الإنجاز:** جلسة تبقى صامدة أسبوعاً بلا إعادة دخول · دمج السلة بلا فقدان عناصر · حذف الحساب يعمل (شرط Apple).

---

## 🔔 المرحلة 4 — الإشعارات والتلميع (الأسابيع 13-15)

| # | المهمة | أيام |
|---|-------|------|
| 4.1 | Firebase (Android + iOS APNs) + `firebase_messaging` | 2 |
| 4.2 | `DeviceToken` + `/devices/register` + إلغاء التسجيل عند الخروج | 1.5 |
| 4.3 | إشعار عند تغيّر حالة الطلب (خطّاف في `OrderService.UpdateStatusAsync`) ⭐ | 2 |
| 4.4 | إشعارات ترويجية من لوحة الأدمن (شاشة إرسال جديدة) | 2 |
| 4.5 | Deep Links (App Links + Universal Links) | 2 |
| 4.6 | Onboarding + رسوم توضيحية | 1.5 |
| 4.7 | Crashlytics + Analytics (أحداث: view_item, add_to_cart, purchase) | 1.5 |
| 4.8 | Skeletons + انتقالات (Hero) + تلميع بصري | 2 |
| 4.9 | إمكانية الوصول (أحجام خطوط، تباين، TalkBack/VoiceOver) | 1.5 |
| 4.10 | تحسين الأداء (حجم التطبيق، تحميل مؤجل، ضغط الصور) | 2 |

---

## 🚀 المرحلة 5 — الاختبار والنشر (الأسابيع 15-16)

| # | المهمة | أيام |
|---|-------|------|
| 5.1 | اختبارات Widget للشاشات الحرجة (≥ 60% تغطية للمنطق) | 3 |
| 5.2 | اختبارات تكامل للرحلات الرئيسية (`integration_test`) | 2 |
| 5.3 | اختبار يدوي على مصفوفة أجهزة (5 Android + 3 iOS) | 2 |
| 5.4 | اختبار أمني: SSL Pinning + كشف الجذر + منع لقطات الشاشة للبيانات الحساسة | 2 |
| 5.5 | إعداد المتاجر: أيقونات، لقطات، وصف ar/en، سياسة خصوصية | 2 |
| 5.6 | Play Console (Internal → Closed → Production) | 1.5 |
| 5.7 | App Store Connect + TestFlight + مراجعة Apple | 2.5 |
| 5.8 | مراقبة ما بعد النشر + خطة الإصدار السريع (Hotfix) | 1 |

---

## 🔮 المرحلة 6 — ما بعد V1.0 (اختياري)

| الميزة | القيمة | الجهد |
|-------|--------|------|
| **الدفع الإلكتروني (Paymob/Fawry/ValU)** | 🟢🟢🟢 عالية جداً | 3-4 أسابيع |
| كوبونات وخصومات | 🟢🟢 عالية | 2 أسبوع |
| المراجعات والتقييمات | 🟢🟢 عالية | 2 أسبوع |
| برنامج نقاط الولاء | 🟢 متوسطة | 2-3 أسبوع |
| تنويعات المنتج (لون/مقاس مع مخزون مستقل) | 🟢🟢 عالية | 3 أسابيع ⚠️ يتطلب تغيير نموذج البيانات |
| دخول اجتماعي (Google/Apple) — ⚠️ Apple **إلزامي** لو أضفت Google | 🟢 متوسطة | 1 أسبوع |
| تتبع الشحنة (تكامل شركات الشحن) | 🟢🟢 عالية | 2 أسبوع |
| دردشة مباشرة / بوت واتساب | 🟢 متوسطة | 1-2 أسبوع |
| تطبيق أدمن (Flutter — نفس الكود) | 🟡 | 3-4 أسابيع |
| PWA للموقع الحالي | 🟢 (مكسب سريع) | 1 أسبوع |

## 10.1 مخطط جانت مبسّط

```text
أسبوع:      1    2    3    4    5    6    7    8    9   10   11   12   13   14   15   16
M0 تمهيد   ████
M1 Backend      ████████████████████
M2 Flutter                ████████████████████████████
M3 حسابات                                          ████████████████
M4 إشعارات                                                        ████████████
M5 نشر                                                                      ████████
                                                    ▲                            ▲
                                              MVP جاهز                      V1.0 منشور
```

**نقاط التزامن الحرجة:**
* Flutter يبدأ **أسبوع 4** (بعد استقرار عقد الـ API في Swagger) — ليس قبل.
* الـ Backend ينتهي **أسبوع 5**، ثم يتحول لدعم + الإشعارات.
* **بوابة قرار في نهاية أسبوع 9:** ننشر MVP كضيف فوراً؟ أم ننتظر الحسابات؟
  → **توصيتنا: انشر MVP** — تبدأ في جمع بيانات مستخدمين حقيقية وتحسّن بناءً عليها.

---

# 11. الأمن والحماية

## 11.1 على الخادم (FTD.Api)

| # | الإجراء | الأولوية |
|---|--------|---------|
| S-1 | **إخراج `JwtSettings:Secret` من `appsettings.json`** → متغير بيئة / Azure Key Vault + تدوير المفتاح فوراً (المفتاح الحالي مكشوف في Git) | 🔴 P0 |
| S-2 | `Encoding.ASCII` → `Encoding.UTF8` + مفتاح ≥ 256-bit عشوائي | 🔴 P0 |
| S-3 | Access Token 15 دقيقة + Refresh مُهشّر مع تدوير وكشف إعادة الاستخدام | 🔴 P0 |
| S-4 | HTTPS إلزامي + HSTS + **إيقاف HTTP كلياً** في الإنتاج | 🔴 P0 |
| S-5 | تفعيل الـ Lockout في الـ API (`CheckPasswordSignInAsync(..., true)`) | 🔴 P0 |
| S-6 | **تحقق الملكية على كل مسار `/orders/{id}`** — لا تثق بالـ id من العميل (منع IDOR) | 🔴 P0 |
| S-7 | `/orders/track` يشترط `phone4` — أرقام الطلبات متوقعة النمط | 🔴 P0 |
| S-8 | **لا تُرجع `Stock` الرقمي** — `InStock`/`LowStock` فقط | 🟠 P1 |
| S-9 | Rate Limiting على 7 سياسات (لا الدخول فقط) | 🟠 P1 |
| S-10 | `Idempotency-Key` على `checkout` (منع الطلبات المكررة) | 🟠 P1 |
| S-11 | `ProblemDetails` لا يكشف Stack Trace في الإنتاج | 🟠 P1 |
| S-12 | تدقيق (Audit Log) لعمليات الأدمن الحساسة | 🟡 P2 |
| S-13 | تنظيف الـ Refresh Tokens المنتهية (مهمة خلفية) | 🟡 P2 |
| S-14 | **إعادة استخدام `HtmlSanitizer`** الموجود على أي محتوى يُعرض في التطبيق | 🟠 P1 |

## 11.2 على التطبيق (Flutter)

| # | الإجراء | الأولوية |
|---|--------|---------|
| C-1 | `flutter_secure_storage` للتوكنات (Keychain / EncryptedSharedPreferences) — **لا `SharedPreferences` عادي** | 🔴 P0 |
| C-2 | **لا تخزّن كلمة المرور أبداً** — بصمة/Face ID تحرس التوكن المخزّن فقط | 🔴 P0 |
| C-3 | تعطيل السجلات (logs) في نسخة الإنتاج (لا تسجّل ترويسات Authorization) | 🔴 P0 |
| C-4 | **SSL/Certificate Pinning** على الـ API | 🟠 P1 |
| C-5 | `FLAG_SECURE` / منع لقطات الشاشة في شاشات الدفع | 🟠 P1 |
| C-6 | كشف الجذر/الـ Jailbreak → تحذير (لا حجب كامل) | 🟡 P2 |
| C-7 | **التحقق من الصلاحية في الخادم دائماً** — لا تثق بأي منطق أمان في التطبيق (قابل للهندسة العكسية) | 🔴 P0 |
| C-8 | تشويش الكود (`--obfuscate --split-debug-info`) في نسخة الإصدار | 🟠 P1 |
| C-9 | مسح كل البيانات الحساسة والكاش عند تسجيل الخروج | 🟠 P1 |

> ⚠️ **قاعدة ذهبية:** أي شيء في التطبيق قابل للاستخراج والتعديل. **الخادم هو الحكم الوحيد** على الأسعار والمخزون والصلاحيات. لحسن الحظ `OrderService` الحالي **يفعل ذلك بشكل صحيح** — يعيد جلب أسعار المنتجات من قاعدة البيانات ولا يثق بالسعر المُرسل من العميل. **حافظوا على هذا المبدأ.**

---

# 12. الإشعارات الفورية

## 12.1 المعمارية

```text
حدث في النظام (تغيّر حالة طلب / عرض جديد)
        ↓
FTD.Application → INotificationService.SendAsync(userId, template, data)
        ↓
FTD.Infrastructure → FirebaseNotificationService
        ↓
   Firebase Cloud Messaging (FCM)
        ↓                  ↓
  Android (FCM)      iOS (APNs عبر FCM)
        ↓                  ↓
   📱 التطبيق → deep link → الشاشة المستهدفة
```

## 12.2 كيان جديد

```csharp
public class DeviceToken {
    public int Id; public string? UserId;      // nullable = ضيف
    public string Token;                        // FCM token
    public string Platform;                     // "android" | "ios"
    public string? DeviceModel; public string? AppVersion;
    public string Language = "ar";              // لإرسال الإشعار بلغة المستخدم
    public bool IsActive = true;
    public DateTime CreatedAt; public DateTime LastSeenAt;
}
```

## 12.3 أنواع الإشعارات

| النوع | المُشغّل | الأولوية |
|------|---------|---------|
| **تغيّر حالة الطلب** ⭐ | خطّاف في `OrderService.UpdateStatusAsync` (7 حالات موجودة أصلاً) | 🔴 P0 |
| تأكيد الطلب | بعد `CreateOrderAsync` | 🔴 P0 |
| السلة المتروكة | مهمة مجدولة (بعد 24 ساعة) | 🟠 P1 |
| عرض/خصم جديد | إرسال يدوي من لوحة الأدمن | 🟠 P1 |
| عاد للمخزون | تغيّر `Stock` من 0 → أكثر (لمن طلب تنبيهاً) | 🟡 P2 |

> ⭐ **أفضل نسبة قيمة/جهد في المشروع كله:** الخطّاف في `UpdateStatusAsync`. الأدمن يغيّر حالة الطلب من لوحة التحكم (شيء يفعله أصلاً كل يوم) → العميل يستلم إشعاراً فورياً "طلبك FTD... مع شركة الشحن". يوم عمل واحد لقيمة تجربة ضخمة.

## 12.4 نقاط انتباه

* **iOS يتطلب** شهادة APNs + حساب Apple Developer (99$/سنة) + إذن صريح من المستخدم.
* **اطلب إذن الإشعارات في اللحظة المناسبة** — لا عند أول إقلاع. الأفضل: **بعد أول طلب ناجح** ("هل تريد متابعة حالة طلبك؟") → نسبة القبول تتضاعف.
* أرسل الإشعار **بلغة المستخدم** (`DeviceToken.Language`).
* احترم تفضيلات المستخدم من شاشة الإعدادات.

---

# 13. الاختبارات وضمان الجودة

## 13.1 هرم الاختبارات — Backend

| النوع | الأداة | الهدف | ملاحظة |
|------|-------|-------|--------|
| اختبارات وحدة | xUnit + Moq + `InMemory`/SQLite | ≥ 70% لطبقة `Application` | ⚠️ **المشروع حالياً بلا مشروع اختبارات** — يجب إنشاؤه |
| اختبارات تكامل | `WebApplicationFactory` + Testcontainers | كل الـ 45 endpoint | ≥ 40 اختباراً |
| اختبارات عقد | Swagger Schema Validation | منع كسر العقد | في CI |
| اختبار حمل | k6 / NBomber | `/home` < 200ms p95 · `/products` < 300ms | جولة قبل النشر |
| اختبار أمني | OWASP ZAP + مراجعة يدوية | IDOR / Auth Bypass / Rate Limit | جولة قبل النشر |

> ⚠️ **ملاحظة مهمة:** لا يوجد مشروع اختبارات في الحل حالياً (تحققنا — لا `*.Tests`). جولات التدقيق السابقة كانت مراجعة يدوية + `dotnet build`. **إنشاء `FTD.Tests` هو استثمار أساسي في المرحلة 1** — لأن تغيير `IdentityUser → AppUser` يمسّ `FTD.Web` العامل، وبدون اختبارات لن تكتشف الانحدار إلا في الإنتاج.

## 13.2 هرم الاختبارات — Flutter

| النوع | الأداة | الهدف |
|------|-------|-------|
| Unit | `flutter_test` + `mocktail` | Repositories + Providers + منطق الأسعار |
| Widget | `flutter_test` | ProductCard · CartTile · نماذج التحقق |
| Golden | `golden_toolkit` | لقطات مرجعية للشاشات في **RTL و LTR** ⭐ |
| Integration | `integration_test` | 5 رحلات كاملة (تصفح · شراء · دخول · تتبع · مفضلة) |
| يدوي | مصفوفة أجهزة | 5 Android (8/10/13/14 + شاشة صغيرة) + 3 iOS (13/16/17) |

> ⭐ **Golden Tests للـ RTL فائدتها كبيرة هنا:** المشروع ثنائي اللغة، وأخطاء RTL (هوامش معكوسة، أيقونات مقلوبة، نص محاذاته خطأ) هي **أكثر أنواع الأخطاء البصرية شيوعاً** في التطبيقات العربية. Golden tests تكشفها تلقائياً في CI بدل المراجعة البصرية اليدوية.

## 13.3 قائمة فحص RTL (يدوية — إلزامية لكل شاشة)

- [ ] النص العربي محاذاته يمين، الإنجليزي يسار
- [ ] الأيقونات الاتجاهية (سهم الرجوع، التالي) **منعكسة**
- [ ] الهوامش تستخدم `EdgeInsetsDirectional` لا `EdgeInsets.only(left:)`
- [ ] `Bottom Nav` مرتّب من اليمين
- [ ] الأرقام العربية/الهندية معالجة بشكل متسق (**التوصية: أرقام لاتينية للأسعار** — أوضح تجارياً)
- [ ] `TextField` مؤشره وتحديده يعملان صح في العربية
- [ ] عمليات السحب (Swipe to delete) في الاتجاه الصحيح
- [ ] الأسعار مع رمز العملة (`ج.م`) موضعها صحيح

---

# 14. DevOps والنشر على المتاجر

## 14.1 البيئات

| البيئة | الـ API | قاعدة البيانات | التطبيق |
|-------|--------|---------------|--------|
| Development | `localhost:5100` | SQL محلي | Debug + emulator |
| **Staging** | `api-staging.unishop.eg` | نسخة بيانات مُقنّعة | Internal Testing / TestFlight |
| Production | `api.unishop.eg` | الإنتاج | Play Store / App Store |

⚠️ **بيئة Staging ليست ترفاً** — لا تختبر تطبيقاً على قاعدة إنتاج. مطلوبة من أسبوع 1.

## 14.2 CI/CD

**Backend (GitHub Actions):**
```yaml
on: [push to genspark_ai_developer, PR to master]
jobs:
  - dotnet restore → build (TreatWarningsAsErrors) → test
  - نشر Swagger كأداة (artifact)
  - deploy to Staging (تلقائي)
  - deploy to Production (بموافقة يدوية)
```

**Flutter (GitHub Actions أو Codemagic):**
```yaml
on: [push, PR]
jobs:
  - flutter analyze (0 مشاكل) → flutter test → golden tests
  - build appbundle --release --obfuscate --split-debug-info
  - build ipa --release
  - Fastlane → Play Internal Track + TestFlight
```

## 14.3 قائمة فحص النشر على المتاجر

### Google Play
- [ ] حساب مطوّر (25$ مرة واحدة)
- [ ] AAB موقّع (Play App Signing)
- [ ] أيقونة 512×512 + صورة غلاف 1024×500
- [ ] 4-8 لقطات شاشة (هاتف + تابلت) — **بالعربية والإنجليزية**
- [ ] وصف ar + en + كلمات مفتاحية
- [ ] **سياسة الخصوصية (رابط عام إلزامي)** — استخدم `ContentPage` الموجود في المشروع! ✅
- [ ] Data Safety Form (ما تجمعه: بريد، هاتف، عنوان)
- [ ] تصنيف المحتوى + الفئة (Shopping)
- [ ] Internal → Closed → Production تدريجياً

### Apple App Store ⚠️ (الأصعب)
- [ ] Apple Developer Program (99$/سنة)
- [ ] شهادات + Provisioning Profiles + App ID
- [ ] أيقونة 1024×1024 (بلا شفافية، بلا زوايا مستديرة)
- [ ] لقطات لكل مقاس (6.7" · 6.5" · 5.5" · iPad)
- [ ] **حذف الحساب داخل التطبيق** ← رفض مؤكد بدونه 🔴
- [ ] App Privacy (Nutrition Label)
- [ ] TestFlight (داخلي ثم خارجي)
- [ ] **إن أضفت دخول Google/Facebook ← Sign in with Apple إلزامي** 🔴

### 🍎 أسباب الرفض الشائعة من Apple (احترسوا منها)

| السبب | الوقاية |
|------|---------|
| لا يوجد حذف حساب | نفّذ `DELETE /auth/account` — **في MVP لا لاحقاً** |
| "التطبيق مجرد موقع مُغلَّف" (Guideline 4.2) | ميزات أصلية: إشعارات، أوف-لاين، بصمة، كاميرا |
| لا Sign in with Apple مع وجود دخول اجتماعي آخر | إما أضفهما معاً، أو ابدأ ببريد/كلمة مرور فقط |
| بيانات وهمية في لقطات الشاشة | استخدم بيانات المتجر الحقيقية |
| رابط سياسة خصوصية معطّل | استخدم `ContentPage` الموجود ✅ |
| أخطاء/تعليق في المراجعة | اختبر على iOS 13 وأجهزة قديمة |
| ذكر منصات أخرى ("متوفر على Android") | نظّف نصوص المتجر |

## 14.4 إصدار النسخ (Versioning)

```text
pubspec.yaml:  version: 1.0.0+1
               MAJOR.MINOR.PATCH + BUILD_NUMBER
```
* `/api/v1/app/config` يرجّع `minSupportedVersion` → شاشة تحديث إلزامي.
* **Staged Rollout في Play:** 10% → 50% → 100% مع مراقبة Crashlytics.

---

# 15. التقديرات الزمنية والتكاليف والفريق

## 15.1 الفريق

| الدور | التخصيص | المراحل | ملاحظة |
|------|---------|--------|--------|
| **Backend .NET** | 100% أسابيع 1-5، 30% بعدها | M0, M1, دعم | يعرف المشروع بالفعل ← ميزة كبيرة |
| **Flutter Developer** | 100% أسابيع 4-16 | M2-M5 | خبرة RTL **مطلوبة صراحةً** |
| Flutter Developer #2 *(اختياري)* | 100% أسابيع 6-14 | M2-M4 | يقلص المدة ~3-4 أسابيع |
| **QA** | 50% | M2-M5 | يبدأ من أسبوع 6 |
| **UI/UX Designer** | 50% أسابيع 1-8 | M0-M3 | تكييف هوية الويب للموبايل |
| DevOps | 25% | M0, M5 | البيئات + CI/CD |

## 15.2 ملخص الجهد

| المرحلة | أيام عمل | أسابيع تقويمية |
|--------|---------|---------------|
| M0 التمهيد | 8 | 1 |
| **M1 Backend** | **33** | **4** |
| M2 Flutter MVP | 38 | 6 (بالتوازي جزئياً) |
| M3 الحسابات | 21 | 3 |
| M4 الإشعارات | 18 | 2.5 |
| M5 النشر | 16 | 2 |
| **الإجمالي** | **~134 يوم عمل** | **16 أسبوعاً** |

## 15.3 التكاليف الثابتة والمتكررة

| البند | التكلفة | التكرار |
|------|---------|--------|
| Apple Developer Program | 99$ | سنوياً |
| Google Play Developer | 25$ | مرة واحدة |
| Firebase (FCM + Crashlytics + Analytics) | 0$ | مجاني في حدود سخية |
| استضافة الـ API (Staging + Prod) | 30-80$ | شهرياً |
| CDN للصور (Cloudflare) | 0-20$ | شهرياً |
| شهادة SSL | 0$ | Let's Encrypt |
| Codemagic *(اختياري)* | 0-50$ | شهرياً (GitHub Actions مجاني بديل) |
| **الإجمالي التشغيلي** | **~50-150$** | **شهرياً** |

## 15.4 مسارات بديلة للمدة

| السيناريو | المدة | الملاحظة |
|----------|------|---------|
| **الموصى به** | 16 أسبوعاً | 1 Backend + 1 Flutter |
| مُسرّع | 12 أسبوعاً | + مطور Flutter ثانٍ |
| **MVP فقط (ضيف بلا حسابات)** | **10 أسابيع** | 🎯 **أسرع طريق لمتجر منشور** |
| اقتصادي | 20-24 أسبوعاً | مطور واحد بدوام جزئي |

---

# 16. المخاطر وخطط التعامل معها

| # | المخاطرة | الاحتمال | الأثر | خطة التعامل |
|---|---------|---------|------|-------------|
| R-1 | **تغيير `IdentityUser → AppUser` يكسر `FTD.Web` العامل** | 🟠 متوسط | 🔴 عالي | فرع منفصل + اختبارات تكامل للموقع + نسخة احتياطية للقاعدة + اختبار كل شاشات الأدمن يدوياً قبل الدمج |
| R-2 | Migrations على قاعدة إنتاج فيها بيانات حقيقية | 🟠 متوسط | 🔴 عالي | تجربة على نسخة من الإنتاج أولاً · كل الحقول الجديدة nullable · `Down()` مُختبرة · نافذة صيانة |
| R-3 | **رفض Apple** (حذف حساب / 4.2 / Sign in with Apple) | 🟠 متوسط | 🟠 متوسط | نفّذ حذف الحساب في MVP · ميزات أصلية واضحة · مراجعة القائمة قبل الإرسال بأسبوعين |
| R-4 | مشاكل RTL تظهر متأخرة | 🟢 منخفض | 🟠 متوسط | Golden tests للاتجاهين من أسبوع 5 · مراجعة RTL في كل PR |
| R-5 | الصور الكبيرة تقتل الأداء على 3G | 🟠 متوسط | 🟠 متوسط | CDN + WebP + أحجام متعددة (`?w=400`) + placeholder + تحميل مؤجل |
| R-6 | زحف النطاق (Scope Creep) — "أضف الدفع/الكوبونات الآن" | 🔴 **عالي** | 🟠 متوسط | **نطاق MVP مُجمّد كتابياً** · كل طلب جديد → M6 · بوابة قرار أسبوع 9 |
| R-7 | تباعد عقد الـ API عن التطبيق | 🟠 متوسط | 🟠 متوسط | Swagger مصدر الحقيقة · توليد نماذج Dart منه · اختبار عقد في CI |
| R-8 | Refresh Token متزامن يطرد المستخدمين | 🟠 متوسط | 🟠 متوسط | mutex في `AuthInterceptor` · فترة سماح للتوكن القديم (grace period) · اختبار الحالة صراحةً |
| R-9 | عدم توفر مطور Flutter بخبرة RTL | 🟢 منخفض | 🟠 متوسط | ابدأ التوظيف في أسبوع 0 · اختبار عملي في المقابلة (شاشة RTL) |
| R-10 | تسريب سر JWT الحالي (موجود في Git) | 🔴 **مؤكد** | 🔴 عالي | 🚨 **دوّر المفتاح فوراً** — قبل أي شيء آخر |
| R-11 | حمل زائد على الـ API بعد النشر | 🟢 منخفض | 🟠 متوسط | اختبار حمل قبل النشر · Rate limiting · كاش · Staged rollout 10% |
| R-12 | فرق في حساب السعر بين التطبيق والخادم | 🟠 متوسط | 🔴 عالي | **الخادم هو الحكم دائماً** · `decimal` لا `double` · `/cart/validate` قبل الدفع · اختبارات وحدة للحسابات |

## 16.1 المخاطر الثلاثة التي تستحق انتباهك الفوري

1. 🚨 **R-10 — سر JWT مكشوف:** `"REPLACE_THIS_WITH_ENV_VAR_OR_USER_SECRETS_IN_PRODUCTION"` موجود في `appsettings.json` **داخل Git**. لو استُخدمت هذه القيمة في الإنتاج، يمكن لأي شخص لديه الريبو **تزوير توكن أدمن**. **دوّر المفتاح اليوم** — قبل أي عمل على الموبايل.

2. ⚠️ **R-1 — تغيير Identity:** هذا التغيير يمسّ موقعاً **يعمل حالياً في الإنتاج**. الموقع يمرّ أصلاً بـ `AddIdentity<IdentityUser, IdentityRole>` في مكانين. أي خطأ هنا = تعطّل لوحة الأدمن. **إلزامي: فرع منفصل + اختبارات + اختبار يدوي كامل.**

3. 🔴 **R-6 — زحف النطاق:** أكبر قاتل لمشاريع الموبايل. ستُطلب منك ميزات ("والكوبونات؟ والدفع؟ والولاء؟") في منتصف الطريق. **جمّد نطاق MVP كتابياً الآن** ووجّه كل إضافة إلى M6.

---

# 17. قائمة المهام التنفيذية النهائية

## ✅ ابدأ الأسبوع القادم (P0 — لا تتقدم بدونها)

- [ ] **1. 🚨 دوّر سر JWT** وأخرجه إلى متغير بيئة (`FTD.Api/appsettings.json`)
- [ ] **2. أضف Swagger/OpenAPI** إلى `FTD.Api` → العقد يصبح مولّداً من الكود
- [ ] **3. أضف API Versioning** (`/api/v1`) مع إبقاء المسارات القديمة Deprecated
- [ ] **4. أنشئ مشروع `FTD.Tests`** (xUnit) — شبكة أمان لتغيير Identity
- [ ] **5. جهّز بيئة Staging** للـ API بنطاق و HTTPS
- [ ] **6. `GlobalExceptionHandler` + `ProblemDetails` + كتالوج `errorCode`**
- [ ] **7. جمّد نطاق MVP كتابياً** ووقّع عليه مع أصحاب القرار
- [ ] **8. ابدأ توظيف/تخصيص مطور Flutter** بخبرة RTL مثبتة
- [ ] **9. أنشئ مشروع Flutter** + هيكل المجلدات + CI أولي
- [ ] **10. ورشة Design Tokens** — استخراج هوية `site.css`

## 🔧 المرحلة 1 — Backend (الأسابيع 2-5)

<details>
<summary><b>الحسابات والهوية</b></summary>

- [ ] `AppUser : IdentityUser` + تحويل `AddIdentity<>` في `FTD.Web` **و** `FTD.Api`
- [ ] Migration `AddAppUserProfile`
- [ ] Migration `AddUserIdToSalesOrder` (nullable + `SetNull`)
- [ ] Seeder لدور `Customer`
- [ ] `IAuthService` في `FTD.Application`
- [ ] فصل `customer/login` عن `admin/login`
- [ ] تفعيل الـ Lockout في الـ API
- [ ] ⚠️ **اختبار يدوي كامل للوحة الأدمن الحالية** بعد التغيير
</details>

<details>
<summary><b>التوكن</b></summary>

- [ ] كيان `RefreshToken` + Migration + فهارس
- [ ] `JwtTokenService` + التدوير + كشف إعادة الاستخدام
- [ ] `/auth/refresh` · `/auth/logout` · `/auth/logout-all`
- [ ] Forgot / Reset / Change Password
- [ ] `DELETE /auth/account` (إخفاء هوية لا حذف صلب للطلبات)
- [ ] `ASCII → UTF8`
</details>

<details>
<summary><b>الكتالوج</b></summary>

- [ ] `ProductListItemDto` + `PagedResult<T>`
- [ ] `GetPagedAsync` بترتيب **حتمي** (`ThenBy(Id)`)
- [ ] `/api/v1/home` المُجمّع + كاش
- [ ] `/categories` · `/brands` · `/categories/{id}/attributes` مستقلة
- [ ] `LocalizationMiddleware` (`Accept-Language`)
- [ ] URLs مطلقة للصور + `?w=`
- [ ] `ETag` + `Cache-Control`
- [ ] `/search` + `/search/suggest`
</details>

<details>
<summary><b>السلة والطلبات</b></summary>

- [ ] `UserCart` + `DbCartStorage : ICartStorage` (بلا لمس `CartService`!)
- [ ] `/cart/*` (6 نقاط) + `/cart/merge` + `/cart/validate`
- [ ] `Wishlist` + `/wishlist/*`
- [ ] `/orders/my` + `/orders/{id}` + تحقق ملكية
- [ ] `/orders/track/{no}?phone4=`
- [ ] `Idempotency-Key` على `checkout` + ربط `UserId`
- [ ] `/orders/{id}/cancel` + `/orders/statuses`
- [ ] 7 سياسات Rate Limiting
- [ ] `/settings/public` + `/app/config`
- [ ] `/content/pages/{slug}` + `/content/contact-info`
- [ ] ≥ 40 اختبار تكامل
</details>

## 📱 المرحلة 2 — Flutter MVP (الأسابيع 4-9)

<details>
<summary><b>الأساس</b></summary>

- [ ] Dio + `AuthInterceptor` (⚠️ **مع mutex**) + `ErrorInterceptor` + `LocaleInterceptor`
- [ ] نماذج freezed من Swagger
- [ ] الثيم + Cairo/Tajawal + RTL
- [ ] التوطين ARB (ar/en)
- [ ] go_router + Bottom Nav
- [ ] مكتبة المكونات المشتركة
- [ ] `flutter_secure_storage`
- [ ] Hive للكاش
</details>

<details>
<summary><b>الشاشات</b></summary>

- [ ] Splash + Force Update
- [ ] الرئيسية (من `/home`)
- [ ] التصنيفات + الماركات
- [ ] قائمة المنتجات + Infinite Scroll
- [ ] ورقة الفلاتر (مواصفات ديناميكية)
- [ ] البحث + السجل
- [ ] تفاصيل المنتج (معرض + مواصفات + ذات صلة)
- [ ] السلة (محلية + Hive)
- [ ] إتمام الطلب + التحقق + `Idempotency-Key`
- [ ] التأكيد + المشاركة
- [ ] تتبع طلب (ضيف)
- [ ] حالات فراغ/خطأ/أوف-لاين لكل شاشة
</details>

## 👤 المرحلة 3-5

<details>
<summary><b>الحسابات · الإشعارات · النشر</b></summary>

- [ ] دخول/تسجيل/نسيت كلمة المرور
- [ ] تجديد تلقائي + استعادة جلسة
- [ ] دمج السلة عند الدخول
- [ ] طلباتي + التفاصيل + مُتتبِّع الحالة
- [ ] إلغاء + إعادة الطلب
- [ ] المفضلة + Outbox
- [ ] الملف + **حذف الحساب** ⚠️
- [ ] صفحات المحتوى + اتصل بنا
- [ ] Firebase + FCM + APNs
- [ ] خطّاف الإشعار في `UpdateStatusAsync` ⭐
- [ ] Deep Links
- [ ] Crashlytics + Analytics
- [ ] اختبارات Widget + Golden (RTL) + Integration
- [ ] SSL Pinning + التشويش
- [ ] أصول المتاجر (ar + en)
- [ ] Play: Internal → Closed → Production
- [ ] App Store: TestFlight → مراجعة
</details>

---

# 📌 الخلاصة النهائية

## الأجوبة على أسئلتك الثلاثة

### 1️⃣ "اقرأ المشروع كله وحلله"
✅ **تم.** المشروع **متجر إيكومرس ناضج ونظيف** بـ ASP.NET Core 9 + Clean Architecture حقيقية (5 مشاريع، 17 كياناً، 6 خدمات)، ثنائي اللغة RTL، مع لوحة أدمن كاملة ومحرك محتوى ديناميكي. مرّ بجولات تدقيق موثقة (26 ملاحظة مُصلحة + جولة Pentest) وتوثيقه استثنائي (200 ألف حرف). **التقييم: 🟢 قاعدة كود قوية — أفضل نقطة انطلاق ممكنة لمشروع موبايل.**

### 2️⃣ "هل فلاتر أحسن حاجة؟"
✅ **نعم — Flutter هو الأفضل لهذا المشروع، بنتيجة مرجّحة 9.2/10.** والأسباب مستمدة من كودكم لا من الرأي العام:
* تصميمكم مخصص بالكامل (CSS يدوي 4464 سطر بلا framework) → Flutter يرسم كل بكسل بنفسه = تطابق 100% بين المنصتين.
* مشروعكم عربي RTL في كل كيان → Flutter الأقوى في RTL، و**.NET MAUI الأضعف** (لهذا رفضناه رغم خبرتكم في C#).
* كتالوج بصور كثيفة → أداء Flutter في التمرير متفوّق (AOT، بلا JS Bridge).
* وفرة مطوري Flutter في السوق المصري/العربي.

### 3️⃣ "حلله هتلاقي إنه شغال بويب سيرفيس"
✅ **صحيح تماماً — `FTD.Api` موجود ويعمل** على المنفذ 5100 مع JWT وCORS وRate Limiting. لكن التحليل التفصيلي كشف أنه **يغطي 13 نقطة نهاية من ~45 مطلوبة (≈35%)**، وأن **التوثيق يذكر نقاطاً غير موجودة في الكود** (`track`، `pages`، Swagger). أهم 4 فجوات حاجبة:
1. 🔴 **لا حسابات عملاء** — `AuthController` يرفض كل من ليس أدمن.
2. 🔴 **لا Pagination** — يرجّع كل المنتجات بـ DTO ثقيل جداً.
3. 🔴 **لا Refresh Token** — الجلسة تنتهي كل ساعتين.
4. 🔴 **الطلبات بلا `UserId`** — "طلباتي" مستحيلة تقنياً.

## 🎯 التوصية التنفيذية في ثلاث نقاط

1. **استثمر 4-5 أسابيع في الـ Backend أولاً** (المرحلة 0+1). هذه ليست تأخيراً — هي **أهم قرار في المشروع**. البدء بـ Flutter على API ناقص يعني إعادة كتابة ~40% منه.
2. **انشر MVP كضيف في الأسبوع 10** — متجر كامل بلا حسابات. ابدأ في جمع مستخدمين حقيقيين وبيانات حقيقية، ثم أضف الحسابات والإشعارات على أساس واقعي.
3. **حافظ على Clean Architecture:** كل منطق جديد في `FTD.Application` حصراً. مثال ذهبي: `DbCartStorage` يعطي سلة على الخادم **بلا لمس حرف واحد** في `CartService`. هذا العائد المجاني هو مكافأة المعمارية الجيدة التي بنيتموها — استفيدوا منها.

---

<div align="center">

**📄 المستند:** خطة تطبيق الموبايل — Uni-Shop / FTD TechZone
**📅 التاريخ:** 2026-09-06 · **الإصدار:** 1.0
**🔍 نطاق التحليل:** 5 مشاريع · 17 كياناً · 6 خدمات · 13 نقطة نهاية قائمة · 45 نقطة مستهدفة

</div>
