using System.Reflection;

namespace Core.Report.Utils
{
    public static class ReportStringExtensions
    {
        public static string ReplaceTemplateValues(this string template, Dictionary<string, string?> replacements)
        {
            foreach (var kvp in replacements)
            {
                template = template.Replace($"${{{kvp.Key}}}", kvp.Value ?? string.Empty);
            }

            return template;
        }
    }
}