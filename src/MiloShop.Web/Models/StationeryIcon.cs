namespace MiloShop.Web.Models;

public static class StationeryIcon
{
    public static string ForCategory(string? name)
    {
        var title = name?.ToLowerInvariant() ?? "";
        if (Matches(title, "هدیه", "کادو", "gift")) return "gift";
        if (Matches(title, "برنامه", "پلنر", "تقویم", "planner")) return "planner";
        if (Matches(title, "طراحی", "نقاشی", "هنر", "sketch", "paint")) return "art";
        if (Matches(title, "میز", "نظم", "جامدادی", "desk", "office")) return "desk";
        if (Matches(title, "دفتر", "کاغذ", "notebook", "paper")) return "notebook";
        if (Matches(title, "قلم", "خودکار", "مداد", "نوشت", "pen")) return "pen";
        return "notebook";
    }

    private static bool Matches(string title, params string[] words) =>
        words.Any(word => title.Contains(word, StringComparison.Ordinal));
}
