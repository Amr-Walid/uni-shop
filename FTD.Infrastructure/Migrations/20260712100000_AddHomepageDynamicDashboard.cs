using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FTD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomepageDynamicDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnHomepage",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.InsertData(
                table: "ContentBlocks",
                columns: new[] { "Id", "BodyAr", "BodyEn", "ImagePath", "Key", "TitleAr", "TitleEn", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 22, "مباشر · الأكثر مبيعاً", "LIVE · Best Seller Q3", null, "hero.chip1", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 23, "وصل حديثاً · ٢٤", "New Arrivals · 24", null, "hero.chip2", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 24, "منصة التحكم", "The command deck", null, "hero.title.line1", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 25, "للتكنولوجيا العصرية.", "for modern tech.", null, "hero.title.line2", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 26, "لوحة تحكم ومنصة منسقة لأجهزة DOOGEE و JisuLife و Dreame — صُممت للمحترفين وتمت معايرتها للحياة اليومية. توصيل لكافة محافظات مصر بضمان معتمد.", "A curated instrument panel of DOOGEE, JisuLife and Dreame hardware — engineered for professionals, calibrated for daily life. Delivered across Egypt with certified warranty.", null, "hero.subtitle", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 27, "اكتشف الكتالوج", "Explore Catalog", null, "hero.btn1.text", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 28, "/Products", null, null, "hero.btn1.url", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 29, "المميز هذا الأسبوع", "Featured this week", null, "hero.btn2.text", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 30, "#featured", null, null, "hero.btn2.url", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 31, "سنوات خبرة", "Years Experience", null, "hero.stat1.label", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 32, "عميل سعيد", "Delivered Orders", null, "hero.stat2.label", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 33, "منتج فريد", "Curated SKUs", null, "hero.stat3.label", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 34, "ضمان معتمد", "Certified Warranty", null, "value.prop1.title", "bi-shield-check", null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 35, "على جميع المنتجات", "On every product", null, "value.prop1.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 36, "توصيل سريع", "Fast Delivery", null, "value.prop2.title", "bi-truck", null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 37, "لكل محافظات مصر", "To all governorates", null, "value.prop2.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 38, "دعم فني 24/7", "24/7 Support", null, "value.prop3.title", "bi-headset", null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 39, "فريق متخصص دائماً", "Expert team, always on", null, "value.prop3.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 40, "استرجاع 14 يوم", "14-Day Returns", null, "value.prop4.title", "bi-arrow-counterclockwise", null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 41, "بدون أي تعقيدات", "Hassle-free policy", null, "value.prop4.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 42, "تسوق حسب الفئة · ٠١", "Shop by Category · 01", null, "cat.showcase.eyebrow", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 43, "اختر فئتك، واكتشف الأفضل.", "Pick a category, discover the best.", null, "cat.showcase.title", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 44, "كل فئة منسقة بعناية — أجهزة مختارة من أفضل العلامات العالمية بمواصفات مؤكدة وأسعار تنافسية.", "Every category is hand-curated — devices from top global brands with verified specs and competitive prices.", null, "cat.showcase.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 45, "كتالوج المنتجات · ٠٢", "Product Catalog · 02", null, "feat.catalog.eyebrow", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 46, "أجهزة منسقة لكل أسلوب عمل.", "Curated hardware for every workflow.", null, "feat.catalog.title", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 47, "قم بالتصفية حسب الفئة لاستكشاف المنتجات الهامة فقط. تأتي جميع الوحدات مع ضمان معتمد ودعم فني متاح دائماً.", "Filter by category to explore only what matters. Every unit ships with certified warranty and 24/7 technical support.", null, "feat.catalog.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 48, "عرض جميع المنتجات", "View All Products", null, "feat.catalog.btn.text", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 49, "جاهز ترتقي بأجهزتك؟ ابدأ التسوق الآن.", "Ready to upgrade your gear? Start shopping now.", null, "cta.title", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 50, "منتجات أصلية 100% بضمان معتمد، توصيل سريع لباب البيت، وتقسيط بدون فوائد. تجربة تسوق عالمية بمعايير مصرية.", "100% authentic products with certified warranty, fast door-to-door delivery, and 0% interest installments.", null, "cta.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 51, "تسوق الآن", "Shop Now", null, "cta.btn.text", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 52, "/Products", null, null, "cta.btn.url", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 53, "من نحن · ٠٣", "About Us · 03", null, "about.eyebrow", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 54, "تقديم أفضل التكنولوجيا لحياة أكثر سهولة وذكاءً.", "Delivering the best technology for a better life.", null, "about.headline", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 55, "رؤيتنا ورسالتنا · ٠٤", "Mission & Vision · 04", null, "mission.eyebrow", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 56, "نبني مستقبل التكنولوجيا في مصر.", "Building the tech future in Egypt.", null, "mission.headline", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 57, "تواصل معنا · ٠٥", "Contact · 05", null, "contact.eyebrow", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 58, "نحن هنا لمساعدتك دائماً.", "We're always here to help.", null, "contact.title", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 59, "تواصل مع فريق الدعم الفني والمبيعات مباشرة. نقوم بالرد على كافة الاستفسارات خلال ساعات العمل الرسمية.", "Reach our support team directly. We respond to all requests within official working hours.", null, "contact.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 60, "خلّيك أول من يعرف 🔥", "Be the first to know 🔥", null, "newsletter.title", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 61, "اشترك ليصلك كل جديد عن المنتجات والعروض الحصرية قبل أي حد.", "Subscribe for new arrivals and exclusive deals before anyone else.", null, "newsletter.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 62, "اشترك", "Subscribe", null, "newsletter.btn.text", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 63, "بوابتك المعتمدة لأحدث الأجهزة التكنولوجية من DOOGEE و JisuLife و Dreame. نوفر جودة فائقة وضمان حقيقي.", "Your certified gateway for the latest tech hardware from DOOGEE, JisuLife, and Dreame. Delivering premium quality and certified warranty.", null, "footer.desc", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 64, "يونى شوب. جميع الحقوق محفوظة.", "Uni-Shop. All Rights Reserved.", null, "footer.credits", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 65, "DOOGEE,JISULIFE,DREAME,ضمان معتمد,منتجات أصلية,توصيل سريع", "DOOGEE,JISULIFE,DREAME,CERTIFIED WARRANTY,ORIGINAL PRODUCTS,FAST DELIVERY", null, "marquee.items", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Description", "Key", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 10, "منتجات سلايدر الهيرو (IDs مرتبة)", "homepage.hero.products", "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1,3,5" },
                    { 11, "منتجات الكتالوج المميز (IDs مرتبة)", "homepage.featured.products", "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1,2,3,4,5,6" },
                    { 12, "إظهار قسم الهيرو", "homepage.show.hero", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 13, "إظهار شريط المزايا", "homepage.show.values", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 14, "إظهار شريط البراندات المتحرك", "homepage.show.marquee", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 15, "إظهار معرض الفئات", "homepage.show.categories", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 16, "إظهار الكتالوج المميز", "homepage.show.featured", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 17, "إظهار قسم من نحن", "homepage.show.about", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 18, "إظهار الرؤية والرسالة", "homepage.show.mission", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 19, "إظهار شريط CTA", "homepage.show.cta", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 20, "إظهار قسم التواصل", "homepage.show.contact", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 21, "إظهار النشرة البريدية بالفوتر", "homepage.show.newsletter", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 22, "إظهار شارات الدفع بالفوتر", "homepage.show.payments", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" },
                    { 23, "ترتيب أقسام الرئيسية", "homepage.sections.order", "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "hero,values,marquee,categories,featured,about,mission,cta,contact" },
                    { 24, "عدد بلاطات الفئات بالرئيسية", "homepage.categories.count", "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 22);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 23);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 24);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 25);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 26);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 27);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 28);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 29);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 30);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 31);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 32);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 33);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 34);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 35);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 36);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 37);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 38);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 39);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 40);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 41);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 42);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 43);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 44);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 45);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 46);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 47);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 48);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 49);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 50);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 51);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 52);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 53);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 54);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 55);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 56);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 57);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 58);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 59);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 60);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 61);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 62);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 63);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 64);
            migrationBuilder.DeleteData(table: "ContentBlocks", keyColumn: "Id", keyValue: 65);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 10);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 11);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 12);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 13);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 14);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 15);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 16);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 17);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 18);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 19);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 20);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 21);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 22);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 23);
            migrationBuilder.DeleteData(table: "SiteSettings", keyColumn: "Id", keyValue: 24);

            migrationBuilder.DropColumn(
                name: "ShowOnHomepage",
                table: "Categories");
        }
    }
}
