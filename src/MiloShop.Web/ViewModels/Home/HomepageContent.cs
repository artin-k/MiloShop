namespace MiloShop.Web.ViewModels.Home;

public record HomepageField(string Group, string Key, string Label, string Default, bool IsIcon = false, bool IsVisibility = false);

public static class HomepageContent
{
    public const int FeatureCount = 6;
    public static readonly IReadOnlyList<HomepageField> Fields = [
        new("راهنمای خرید", "FeaturesVisibility", "نمایش بخش", "visible", IsVisibility: true),
        new("محصولات", "ProductsVisibility", "نمایش بخش", "visible", IsVisibility: true),
        new("میز مطالعه", "DeskVisibility", "نمایش بخش", "visible", IsVisibility: true),
        new("پایان صفحه", "ClosingVisibility", "نمایش بخش", "visible", IsVisibility: true),
        new("دیدگاه‌ها", "ReviewsVisibility", "نمایش بخش", "visible", IsVisibility: true),
        new("بنر", "HeroTitle", "عنوان", "ایده‌هایت را بنویس."),
        new("بنر", "HeroAccent", "ادامه عنوان", "رویاهایت را شکل بده."),
        new("بنر", "HeroShop", "دکمه فروشگاه", "کشف نوشت‌افزار ←"),
        new("بنر", "HeroCollections", "دکمه مجموعه‌ها", "دیدن مجموعه‌ها"),
        new("محصولات", "ProductsLabel", "زیرعنوان", "تازه روی میز میلو"),
        new("محصولات", "ProductsTitle", "عنوان", "انتخاب بعدی شما، همین‌جاست."),
        new("میز مطالعه", "DeskLabel", "زیرعنوان", "COMPLETE THE DESK"),
        new("میز مطالعه", "DeskTitle", "عنوان", "میزت را، به سبک خودت کامل کن."),
        new("میز مطالعه", "DeskIntro", "توضیحات", "به جای انتخاب یک ست از پیش تعیین‌شده، ابزارهای میز مطالعه یا استودیو را جداگانه و متناسب با نیازت کنار هم بگذار."),
        new("میز مطالعه", "DeskTip", "راهنمای انتخاب", "جنس و برند ثبت‌شده را روی کارت‌ها ببین و تا سه محصول را کنار هم مقایسه کن."),
        new("پایان صفحه", "ClosingLabel", "زیرعنوان", "میلو / ابزار ایده‌های تازه"),
        new("پایان صفحه", "ClosingTitle", "عنوان", "صفحه بعدی را، خودت بنویس."),
        new("پایان صفحه", "ClosingShop", "دکمه فروشگاه", "رفتن به فروشگاه ←"),
        new("پایان صفحه", "ClosingCollections", "دکمه مجموعه‌ها", "کشف مجموعه‌های میلو"),
        new("میز مطالعه", "DeskQuery1", "جستجوی 1", "دفتر"),
        new("میز مطالعه", "DeskLink1", "پیوند 1", "یک دفتر برای فکرها"),
        new("میز مطالعه", "DeskHint1", "توضیح 1", "یادداشت و برنامه‌ریزی"),
        new("میز مطالعه", "DeskQuery2", "جستجوی 2", "قلم"),
        new("میز مطالعه", "DeskLink2", "پیوند 2", "یک قلم برای شروع"),
        new("میز مطالعه", "DeskHint2", "توضیح 2", "نوشتن و طراحی"),
        new("میز مطالعه", "DeskQuery3", "جستجوی 3", "جامدادی"),
        new("میز مطالعه", "DeskLink3", "پیوند 3", "جایی برای ابزارها"),
        new("میز مطالعه", "DeskHint3", "توضیح 3", "نظم و دسترسی"),
        new("مجموعه‌ها", "CollectionsTitle", "عنوان", "مجموعه‌های نوشت‌افزار"),
        new("دیدگاه‌ها", "ReviewsTitle", "عنوان", "از نگاه مشتریان میلو"),
        new("راهنمای خرید", "FeaturesTitle", "عنوان", "هر میز، یک دنیای متفاوت."),
        new("راهنمای خرید", "FeaturesIntro", "توضیحات", "از یادداشت روزانه تا اولین طرح، از کاری شروع کن که دوستش داری."),
        new("کارت 1", "Feature1Icon", "نماد", "notebook", true),
        new("کارت 1", "Feature1Title", "عنوان", "نوشتن و یادداشت"),
        new("کارت 1", "Feature1Description", "توضیحات", "دفترها را برای کلاس، ژورنال یا یادداشت‌های روزانه‌ات ببین."),
        new("کارت 1", "Feature1Query", "عبارت جستجو", "دفتر"),
        new("کارت 2", "Feature2Icon", "نماد", "pen", true),
        new("کارت 2", "Feature2Title", "عنوان", "طراحی و ایده‌پردازی"),
        new("کارت 2", "Feature2Description", "توضیحات", "برای اتودهای سریع و طرح‌های دقیق، ابزار طراحی انتخاب کن."),
        new("کارت 2", "Feature2Query", "عبارت جستجو", "طراحی"),
        new("کارت 3", "Feature3Icon", "نماد", "desk", true),
        new("کارت 3", "Feature3Title", "عنوان", "میز منظم، فکر آزاد"),
        new("کارت 3", "Feature3Description", "توضیحات", "با یک جامدادی، قلم‌ها و خرده‌ریزهای میزت را کنار هم نگه دار."),
        new("کارت 3", "Feature3Query", "عبارت جستجو", "جامدادی"),
        new("کارت 4", "Feature4Icon", "نماد", "planner", true),
        new("کارت 4", "Feature4Title", "عنوان", "برنامه‌ریزی"),
        new("کارت 4", "Feature4Description", "توضیحات", "برای قرارها، کارهای هفته و پیگیری هدف‌هایت پلنر پیدا کن."),
        new("کارت 4", "Feature4Query", "عبارت جستجو", "پلنر"),
        new("کارت 5", "Feature5Icon", "نماد", "art", true),
        new("کارت 5", "Feature5Title", "عنوان", "رنگ و نقاشی"),
        new("کارت 5", "Feature5Description", "توضیحات", "مدادرنگی‌های مجموعه را برای رنگ‌آمیزی و تجربه با رنگ ببین."),
        new("کارت 5", "Feature5Query", "عبارت جستجو", "مدادرنگی"),
        new("کارت 6", "Feature6Icon", "نماد", "gift", true),
        new("کارت 6", "Feature6Title", "عنوان", "هدیه‌های کوچک"),
        new("کارت 6", "Feature6Description", "توضیحات", "برای کسی که کاغذ و قلم را دوست دارد، هدیه انتخاب کن."),
        new("کارت 6", "Feature6Query", "عبارت جستجو", "هدیه"),
    ];

    public static async Task<Dictionary<string, string>> LoadAsync(ISiteSettingsService settings)
    {
        var stored = await settings.GetManyAsync(Fields.Select(field => "Home." + field.Key).ToArray());
        return Fields.ToDictionary(field => field.Key, field => stored["Home." + field.Key] ?? field.Default);
    }
}
