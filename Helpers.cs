using System.Text.RegularExpressions;

namespace ObenFind
{
    internal static class Helpers
    {
        internal static string ColorizedMatch(string searchTerm, string text, RichtextColor color)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return text;

            var pattern = Regex.Escape(searchTerm); // literal match, no regex injection

            return Regex.Replace(
                text,
                pattern,
                m => $"<color={color}>{m.Value}</color>",
                RegexOptions.IgnoreCase
            );
        }
    }
}
