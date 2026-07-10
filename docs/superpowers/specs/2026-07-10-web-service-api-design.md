# مستند تصميم الـ Web Service (FTD.Api Project Design)

* **التاريخ:** 2026-07-10
* **الحالة:** بانتظار مراجعة المستخدم وعقد خطة التنفيذ
* **الهدف:** إضافة مشروع Web API منفصل لتقديم الخدمات البرمجية لتطبيقات الموبايل أو التطبيقات الخارجية مستقبلاً مع تطبيق أمان JWT.

---

## 📌 1. الأهداف والمتطلبات (Goal & Requirements)
* **إنشاء مشروع API مستقل:** فصل كامل عن الكود المسؤول عن واجهة الويب الحالية (MVC) لضمان سهولة النشر والصيانة والتوسع بشكل منفصل.
* **توفير واجهات عامة (Public APIs):** تتيح لتطبيقات العميل جلب المنتجات، التصنيفات، العلامات التجارية، وتفاصيل الصفحات الديناميكية، بالإضافة إلى إتمام الطلب (Checkout) دون الحاجة لجلسة خادم (Session-Free).
* **توفير واجهات محمية (Secured APIs):** حماية لوحة تحكم المشرف والعمليات الحساسة بـ JWT Tokens بصلاحية الأدمن.
* **تجهيز إعدادات CORS:** السماح بالطلبات الخارجية الواردة من نطاقات أخرى (Cross-Origin).

---

## 🏗️ 2. الهيكلية المقترحة ومخطط المكونات (Proposed Architecture)

المشروع الجديد **`FTD.Api`** سيكون مشروعاً فرعياً خامساً في الـ Solution وسيعتمد على الطبقات المشتركة كالتالي:

```text
FTD.Api (Presentation)
  └── FTD.Application (Logic & interfaces)
  └── FTD.Infrastructure (Data access & implementations)
  └── FTD.Domain (Core entities)
```

### حزم NuGet المطلوبة في مشروع `FTD.Api`:
1. `Microsoft.AspNetCore.Authentication.JwtBearer` (الإصدار `9.0.0`)
2. `System.IdentityModel.Tokens.Jwt` (الإصدار `8.0.0` أو متوافق)
3. `Microsoft.EntityFrameworkCore.Design` (لتمكين تشغيل الميجريشنز إذا لزم الأمر)

---

## 🛣️ 3. تفاصيل الواجهات البرمجية (API Endpoints Map)

### أ. واجهات الحماية والتوثيق (Auth API)
* `POST /api/auth/login`
  * **المدخلات:** `LoginDto` (البريد الإلكتروني، كلمة المرور)
  * **المخرجات:** رمز الـ JWT، تاريخ الانتهاء، البريد الإلكتروني.

### ب. واجهات الاستعراض والتسوق العامة (Public APIs)
* `GET /api/products` -> جلب المنتجات مع دعم فلتر `brandId` و `categoryId` ونصوص البحث `query`.
* `GET /api/products/{slug}` -> جلب تفاصيل المنتج بالكامل مع الصور والخصائص.
* `GET /api/categories` -> قائمة التصنيفات.
* `GET /api/brands` -> قائمة العلامات التجارية.
* `GET /api/pages/{slug}` -> تفاصيل الصفحات الحرة وأقسامها.
* `POST /api/contact` -> استقبال رسائل اتصل بنا.
* `POST /api/orders/checkout`
  * **المدخلات:** معلومات المشتري الشخصية وقائمة ببيانات المنتجات المشتراة (ProductId، الكمية).
  * **آلية العمل:** يقوم المتحكم باستدعاء `IOrderService` مباشرة لإنشاء الطلب وحساب التكلفة الفورية، متجنباً استخدام Session الخاص بالمتصفح لتسهيل العمل على الموبايل.

### ج. واجهات الإدارة المحمية (Secured Admin APIs)
*جميع هذه الواجهات تتطلب ترويسة `Authorization: Bearer <TOKEN>` وصلاحية `Admin`.*
* `GET /api/admin/dashboard` -> مؤشرات الأداء والإحصائيات.
* `GET /api/admin/orders` -> قائمة الطلبات مع فرزها بحسب الحالة.
* `PUT /api/admin/orders/{id}/status` -> تحديث حالة الطلب وإضافة ملاحظات المشرف.
* `POST/PUT/DELETE /api/admin/products` -> إدارة المنتجات وتعديل خصائصها.
* `POST/PUT/DELETE /api/admin/categories` -> إدارة تصنيفات المنتجات.

---

## 🔒 4. أمن ومصادقة JWT (JWT Configuration)

تتم حماية الواجهات الخاصة بـ Admin بفلتر تفويض موحد:
```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
```

### ملف الإعدادات `appsettings.json` في مشروع `FTD.Api`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=FTD_TechZone;..."
  },
  "JwtSettings": {
    "Secret": "A_VERY_LONG_AND_SECURE_JWT_SECRET_KEY_FOR_FTD_TECHZONE_2026",
    "Issuer": "FTD.Api",
    "Audience": "FTD.Client",
    "ExpiryMinutes": 120
  }
}
```

---

## 🧪 5. خطة التحقق والضمان (Verification Plan)

### أ. التحقق الآلي والتجميعي:
1. التأكد من نجاح تجميع الـ Solution بالكامل بعد إضافة المشروع الجديد عبر `dotnet build`.
2. التأكد من معالجة الاعتماديات وحقنها بشكل سليم في ملف `Program.cs` الخاص بـ `FTD.Api`.

### ب. التحقق اليدوي (Manual Testing):
1. تشغيل الـ Web API واختبار طلب تسجيل الدخول لإنشاء التوكن (`POST /api/auth/login`).
2. محاولة استدعاء أحد واجهات المشرف المؤمنة بدون توكن والتأكد من إرجاع `410 Unauthorized` أو `401 Unauthorized`.
3. استدعاء واجهات المشرف باستخدام توكن JWT صحيح والتأكد من إرجاع البيانات بنجاح (`200 OK`).
4. اختبار استدعاء تصفح المنتجات وإجراء طلب شراء بالـ API والتأكد من تسجيل الطلب في قاعدة البيانات.
