/// Translation tables.
///
/// Kept as plain maps instead of generated ARB classes: the app already avoids
/// build_runner (see pubspec rationale), and a map lookup lets the *server*
/// language and the *UI* language stay in sync trivially — the same `ar`/`en`
/// code sent in `Accept-Language` selects the table here.
///
/// Only UI chrome lives here. Product names, category names and order-status
/// labels are NOT translated client-side: the backend already resolves its
/// paired Ar/En columns from `Accept-Language`, so duplicating them would risk
/// showing a stale translation.
abstract final class AppStrings {
  static const Map<String, Map<String, String>> values = {
    // ── App / general ────────────────────────────────────────────────────────
    'appName': {'ar': 'يونى شوب', 'en': 'Uni-Shop'},
    'ok': {'ar': 'حسناً', 'en': 'OK'},
    'cancel': {'ar': 'إلغاء', 'en': 'Cancel'},
    'confirm': {'ar': 'تأكيد', 'en': 'Confirm'},
    'save': {'ar': 'حفظ', 'en': 'Save'},
    'delete': {'ar': 'حذف', 'en': 'Delete'},
    'edit': {'ar': 'تعديل', 'en': 'Edit'},
    'close': {'ar': 'إغلاق', 'en': 'Close'},
    'back': {'ar': 'رجوع', 'en': 'Back'},
    'next': {'ar': 'التالي', 'en': 'Next'},
    'done': {'ar': 'تم', 'en': 'Done'},
    'retry': {'ar': 'إعادة المحاولة', 'en': 'Retry'},
    'seeAll': {'ar': 'عرض الكل', 'en': 'See all'},
    'more': {'ar': 'المزيد', 'en': 'More'},
    'apply': {'ar': 'تطبيق', 'en': 'Apply'},
    'clear': {'ar': 'مسح', 'en': 'Clear'},
    'reset': {'ar': 'إعادة تعيين', 'en': 'Reset'},
    'search': {'ar': 'بحث', 'en': 'Search'},
    'loading': {'ar': 'جارٍ التحميل…', 'en': 'Loading…'},
    'currency': {'ar': 'ج.م', 'en': 'EGP'},
    'optional': {'ar': '(اختياري)', 'en': '(optional)'},

    // ── Navigation ───────────────────────────────────────────────────────────
    'navHome': {'ar': 'الرئيسية', 'en': 'Home'},
    'navCatalog': {'ar': 'المنتجات', 'en': 'Shop'},
    'navCart': {'ar': 'السلة', 'en': 'Cart'},
    'navOrders': {'ar': 'طلباتي', 'en': 'Orders'},
    'navAccount': {'ar': 'حسابي', 'en': 'Account'},

    // ── Home ─────────────────────────────────────────────────────────────────
    'homeGreeting': {'ar': 'أهلاً بك 👋', 'en': 'Welcome 👋'},
    'homeSearchHint': {
      'ar': 'ابحث عن منتج، ماركة، أو قسم…',
      'en': 'Search products, brands, categories…',
    },
    'homeCategories': {'ar': 'الأقسام', 'en': 'Categories'},
    'homeFeatured': {'ar': 'منتجات مميزة', 'en': 'Featured'},
    'homeNewArrivals': {'ar': 'وصل حديثاً', 'en': 'New arrivals'},
    'homeBestSellers': {'ar': 'الأكثر مبيعاً', 'en': 'Best sellers'},
    'homeBrands': {'ar': 'الماركات', 'en': 'Brands'},
    'homeOffers': {'ar': 'العروض', 'en': 'Offers'},

    // ── Catalog ──────────────────────────────────────────────────────────────
    'catalogTitle': {'ar': 'المنتجات', 'en': 'Products'},
    'catalogFilters': {'ar': 'تصفية', 'en': 'Filters'},
    'catalogSort': {'ar': 'ترتيب', 'en': 'Sort'},
    'catalogResultsCount': {'ar': '{n} منتج', 'en': '{n} products'},
    'catalogNoResults': {
      'ar': 'لا توجد منتجات مطابقة',
      'en': 'No matching products',
    },
    'catalogNoResultsHint': {
      'ar': 'جرّب تعديل التصفية أو كلمات البحث.',
      'en': 'Try adjusting your filters or search terms.',
    },
    'sortNewest': {'ar': 'الأحدث', 'en': 'Newest'},
    'sortPriceAsc': {'ar': 'الأقل سعراً', 'en': 'Price: low to high'},
    'sortPriceDesc': {'ar': 'الأعلى سعراً', 'en': 'Price: high to low'},
    'sortNameAsc': {'ar': 'الاسم أ–ي', 'en': 'Name A–Z'},
    'sortBestSelling': {'ar': 'الأكثر مبيعاً', 'en': 'Best selling'},
    'sortDefault': {'ar': 'الافتراضي', 'en': 'Default'},
    'filterPriceRange': {'ar': 'نطاق السعر', 'en': 'Price range'},
    'filterCategory': {'ar': 'القسم', 'en': 'Category'},
    'filterBrand': {'ar': 'الماركة', 'en': 'Brand'},
    'filterInStockOnly': {'ar': 'المتوفر فقط', 'en': 'In stock only'},
    'filterOnSaleOnly': {'ar': 'العروض فقط', 'en': 'On sale only'},
    'filterMin': {'ar': 'من', 'en': 'Min'},
    'filterMax': {'ar': 'إلى', 'en': 'Max'},

    // ── Product ──────────────────────────────────────────────────────────────
    'productInStock': {'ar': 'متوفر', 'en': 'In stock'},
    'productLowStock': {'ar': 'كمية محدودة', 'en': 'Low stock'},
    'productOutOfStock': {'ar': 'غير متوفر', 'en': 'Out of stock'},
    'productDescription': {'ar': 'الوصف', 'en': 'Description'},
    'productFeatures': {'ar': 'المميزات', 'en': 'Features'},
    'productSpecs': {'ar': 'المواصفات', 'en': 'Specifications'},
    'productRelated': {'ar': 'منتجات مشابهة', 'en': 'Related products'},
    'productAddToCart': {'ar': 'أضف إلى السلة', 'en': 'Add to cart'},
    'productAddedToCart': {'ar': 'تمت الإضافة إلى السلة', 'en': 'Added to cart'},
    'productQuantity': {'ar': 'الكمية', 'en': 'Quantity'},
    'productSku': {'ar': 'كود المنتج', 'en': 'SKU'},
    'productDiscountBadge': {'ar': 'خصم {n}%', 'en': '{n}% off'},
    'productShare': {'ar': 'مشاركة', 'en': 'Share'},
    'productNotFound': {'ar': 'المنتج غير موجود', 'en': 'Product not found'},

    // ── Cart ─────────────────────────────────────────────────────────────────
    'cartTitle': {'ar': 'سلة الشراء', 'en': 'Cart'},
    'cartEmpty': {'ar': 'سلتك فارغة', 'en': 'Your cart is empty'},
    'cartEmptyHint': {
      'ar': 'تصفّح المنتجات وأضف ما يعجبك.',
      'en': 'Browse products and add what you like.',
    },
    'cartStartShopping': {'ar': 'ابدأ التسوق', 'en': 'Start shopping'},
    'cartSubtotal': {'ar': 'المجموع الفرعي', 'en': 'Subtotal'},
    'cartShipping': {'ar': 'الشحن', 'en': 'Shipping'},
    'cartShippingFree': {'ar': 'مجاني', 'en': 'Free'},
    'cartTotal': {'ar': 'الإجمالي', 'en': 'Total'},
    'cartCheckout': {'ar': 'إتمام الشراء', 'en': 'Checkout'},
    'cartRemoveItem': {'ar': 'إزالة المنتج', 'en': 'Remove item'},
    'cartItemRemoved': {'ar': 'تم إزالة المنتج', 'en': 'Item removed'},
    'cartClearConfirm': {
      'ar': 'هل تريد إفراغ السلة بالكامل؟',
      'en': 'Empty the whole cart?',
    },
    'cartFreeShippingHint': {
      'ar': 'أضف {amount} للحصول على شحن مجاني',
      'en': 'Add {amount} more for free shipping',
    },
    'cartStockAdjusted': {
      'ar': 'تم تعديل الكمية حسب المتوفر بالمخزون',
      'en': 'Quantity adjusted to available stock',
    },

    // ── Checkout ─────────────────────────────────────────────────────────────
    'checkoutTitle': {'ar': 'إتمام الطلب', 'en': 'Checkout'},
    'checkoutContactInfo': {'ar': 'بيانات التواصل', 'en': 'Contact details'},
    'checkoutShippingInfo': {'ar': 'عنوان الشحن', 'en': 'Shipping address'},
    'checkoutOrderSummary': {'ar': 'ملخص الطلب', 'en': 'Order summary'},
    'checkoutPlaceOrder': {'ar': 'تأكيد الطلب', 'en': 'Place order'},
    'fieldFullName': {'ar': 'الاسم بالكامل', 'en': 'Full name'},
    'fieldPhone': {'ar': 'رقم الهاتف', 'en': 'Phone number'},
    'fieldEmail': {'ar': 'البريد الإلكتروني', 'en': 'Email'},
    'fieldAddress': {'ar': 'العنوان', 'en': 'Address'},
    'fieldCity': {'ar': 'المدينة', 'en': 'City'},
    'fieldGovernorate': {'ar': 'المحافظة', 'en': 'Governorate'},
    'fieldNotes': {'ar': 'ملاحظات', 'en': 'Notes'},
    'checkoutPaymentCod': {
      'ar': 'الدفع عند الاستلام',
      'en': 'Cash on delivery',
    },
    'checkoutSuccessTitle': {'ar': 'تم استلام طلبك 🎉', 'en': 'Order placed 🎉'},
    'checkoutSuccessBody': {
      'ar': 'سنتواصل معك لتأكيد الطلب في أقرب وقت.',
      'en': 'We will contact you shortly to confirm.',
    },
    'checkoutOrderNumber': {'ar': 'رقم الطلب', 'en': 'Order number'},
    'checkoutCopyNumber': {'ar': 'نسخ الرقم', 'en': 'Copy number'},
    'checkoutNumberCopied': {'ar': 'تم نسخ الرقم', 'en': 'Number copied'},
    'checkoutContinueShopping': {
      'ar': 'متابعة التسوق',
      'en': 'Continue shopping',
    },
    'checkoutSaveNumberHint': {
      'ar': 'احتفظ برقم الطلب لتتبّعه لاحقاً.',
      'en': 'Keep the order number to track it later.',
    },

    // ── Orders ───────────────────────────────────────────────────────────────
    'ordersTitle': {'ar': 'طلباتي', 'en': 'My orders'},
    'ordersEmpty': {'ar': 'لا توجد طلبات بعد', 'en': 'No orders yet'},
    'ordersEmptyHint': {
      'ar': 'طلباتك ستظهر هنا بعد أول عملية شراء.',
      'en': 'Your orders will appear here after your first purchase.',
    },
    'orderDetailTitle': {'ar': 'تفاصيل الطلب', 'en': 'Order details'},
    'orderItems': {'ar': 'المنتجات', 'en': 'Items'},
    'orderTimeline': {'ar': 'مراحل الطلب', 'en': 'Order timeline'},
    'orderPlacedAt': {'ar': 'تاريخ الطلب', 'en': 'Placed on'},
    'orderCancel': {'ar': 'إلغاء الطلب', 'en': 'Cancel order'},
    'orderCancelConfirm': {
      'ar': 'هل تريد إلغاء هذا الطلب؟ لا يمكن التراجع.',
      'en': 'Cancel this order? This cannot be undone.',
    },
    'orderCancelled': {'ar': 'تم إلغاء الطلب', 'en': 'Order cancelled'},
    'orderCancelNotAllowed': {
      'ar': 'لا يمكن إلغاء الطلب في مرحلته الحالية',
      'en': 'This order can no longer be cancelled',
    },
    'orderTrackTitle': {'ar': 'تتبّع الطلب', 'en': 'Track order'},
    'orderTrackHint': {
      'ar': 'أدخل رقم الطلب وآخر ٤ أرقام من هاتفك.',
      'en': 'Enter your order number and the last 4 digits of your phone.',
    },
    'orderTrackNumberField': {'ar': 'رقم الطلب', 'en': 'Order number'},
    'orderTrackPhoneField': {
      'ar': 'آخر ٤ أرقام من الهاتف',
      'en': 'Last 4 phone digits',
    },
    'orderTrackSubmit': {'ar': 'تتبّع', 'en': 'Track'},
    'orderTrackNotFound': {
      'ar': 'لم نجد طلباً بهذه البيانات. تأكّد من رقم الطلب والهاتف.',
      'en': 'No order matches those details. Check the number and phone.',
    },

    // ── Auth ─────────────────────────────────────────────────────────────────
    'authLogin': {'ar': 'تسجيل الدخول', 'en': 'Sign in'},
    'authRegister': {'ar': 'إنشاء حساب', 'en': 'Create account'},
    'authLogout': {'ar': 'تسجيل الخروج', 'en': 'Sign out'},
    'authLogoutConfirm': {
      'ar': 'هل تريد تسجيل الخروج؟',
      'en': 'Sign out of your account?',
    },
    'authPassword': {'ar': 'كلمة المرور', 'en': 'Password'},
    'authConfirmPassword': {'ar': 'تأكيد كلمة المرور', 'en': 'Confirm password'},
    'authForgotPassword': {
      'ar': 'نسيت كلمة المرور؟',
      'en': 'Forgot password?',
    },
    'authNoAccount': {'ar': 'ليس لديك حساب؟', 'en': "Don't have an account?"},
    'authHaveAccount': {'ar': 'لديك حساب بالفعل؟', 'en': 'Already registered?'},
    'authContinueAsGuest': {'ar': 'متابعة كزائر', 'en': 'Continue as guest'},
    'authGuestPrompt': {
      'ar': 'سجّل الدخول لحفظ طلباتك وعناوينك.',
      'en': 'Sign in to save your orders and addresses.',
    },
    'authResetSent': {
      'ar': 'إن كان البريد مسجلاً لدينا فستصلك رسالة لإعادة التعيين.',
      'en': "If that email is registered, a reset link is on its way.",
    },

    // ── Account ──────────────────────────────────────────────────────────────
    'accountTitle': {'ar': 'حسابي', 'en': 'My account'},
    'accountProfile': {'ar': 'البيانات الشخصية', 'en': 'Profile'},
    'accountWishlist': {'ar': 'المفضّلة', 'en': 'Wishlist'},
    'accountAddresses': {'ar': 'العناوين', 'en': 'Addresses'},
    'accountSettings': {'ar': 'الإعدادات', 'en': 'Settings'},
    'accountLanguage': {'ar': 'اللغة', 'en': 'Language'},
    'accountTheme': {'ar': 'المظهر', 'en': 'Appearance'},
    'themeSystem': {'ar': 'حسب النظام', 'en': 'System'},
    'themeLight': {'ar': 'فاتح', 'en': 'Light'},
    'themeDark': {'ar': 'داكن', 'en': 'Dark'},
    'accountContactUs': {'ar': 'اتصل بنا', 'en': 'Contact us'},
    'accountAbout': {'ar': 'عن التطبيق', 'en': 'About'},
    'accountDeleteAccount': {'ar': 'حذف الحساب', 'en': 'Delete account'},
    'accountDeleteConfirm': {
      'ar': 'سيتم حذف حسابك نهائياً. هذا الإجراء لا يمكن التراجع عنه.',
      'en': 'Your account will be permanently deleted. This cannot be undone.',
    },
    'accountVersion': {'ar': 'الإصدار', 'en': 'Version'},

    // ── Wishlist ─────────────────────────────────────────────────────────────
    'wishlistEmpty': {'ar': 'المفضّلة فارغة', 'en': 'Your wishlist is empty'},
    'wishlistEmptyHint': {
      'ar': 'اضغط على ♡ في أي منتج لإضافته هنا.',
      'en': 'Tap ♡ on any product to save it here.',
    },
    'wishlistAdded': {'ar': 'أُضيف للمفضّلة', 'en': 'Added to wishlist'},
    'wishlistRemoved': {'ar': 'أُزيل من المفضّلة', 'en': 'Removed from wishlist'},
    'wishlistLoginRequired': {
      'ar': 'سجّل الدخول لحفظ منتجاتك المفضّلة.',
      'en': 'Sign in to save your favourite products.',
    },

    // ── Validation ───────────────────────────────────────────────────────────
    'validationRequired': {'ar': 'هذا الحقل مطلوب', 'en': 'This field is required'},
    'validationEmail': {
      'ar': 'أدخل بريداً إلكترونياً صحيحاً',
      'en': 'Enter a valid email address',
    },
    'validationPhone': {
      'ar': 'أدخل رقم هاتف مصري صحيح (11 رقماً يبدأ بـ 01)',
      'en': 'Enter a valid Egyptian phone number (11 digits starting 01)',
    },
    'validationPasswordShort': {
      'ar': 'كلمة المرور 6 أحرف على الأقل',
      'en': 'Password must be at least 6 characters',
    },
    'validationPasswordMismatch': {
      'ar': 'كلمتا المرور غير متطابقتين',
      'en': 'Passwords do not match',
    },
    'validationMinLength': {
      'ar': '{n} أحرف على الأقل',
      'en': 'At least {n} characters',
    },
    'validationDigitsOnly': {'ar': 'أرقام فقط', 'en': 'Digits only'},

    // ── Errors ───────────────────────────────────────────────────────────────
    'errorGeneric': {
      'ar': 'حدث خطأ ما. حاول مرة أخرى.',
      'en': 'Something went wrong. Please try again.',
    },
    'errorNetwork': {
      'ar': 'لا يوجد اتصال بالإنترنت',
      'en': 'No internet connection',
    },
    'errorNetworkHint': {
      'ar': 'تحقّق من الشبكة ثم أعد المحاولة.',
      'en': 'Check your connection and try again.',
    },
    'errorTimeout': {'ar': 'انتهت مهلة الاتصال', 'en': 'Connection timed out'},
    'errorServer': {'ar': 'خطأ في الخادم', 'en': 'Server error'},
    'errorSessionExpired': {
      'ar': 'انتهت صلاحية الجلسة. سجّل الدخول مرة أخرى.',
      'en': 'Your session expired. Please sign in again.',
    },
    'errorMaintenance': {
      'ar': 'التطبيق تحت الصيانة حالياً',
      'en': 'The app is under maintenance',
    },
    'errorRateLimited': {
      'ar': 'طلبات كثيرة جداً. انتظر قليلاً.',
      'en': 'Too many requests. Please wait a moment.',
    },
    'errorUpdateRequired': {'ar': 'تحديث مطلوب', 'en': 'Update required'},
    'errorUpdateRequiredBody': {
      'ar': 'يجب تحديث التطبيق للمتابعة.',
      'en': 'You need to update the app to continue.',
    },
    'errorUpdateNow': {'ar': 'تحديث الآن', 'en': 'Update now'},
  };
}
