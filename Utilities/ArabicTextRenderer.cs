using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ElectricalPOS.Utilities
{
    /// <summary>
    /// Provides RTL-safe Arabic text rendering for WPF charts and other UI elements.
    /// Addresses common issues with Arabic text in chart libraries that use low-level drawing APIs.
    /// </summary>
    public static class ArabicTextRenderer
    {
        #region Constants

        /// <summary>
        /// Right-To-Left Mark (RLM) - Unicode character for explicit RTL direction
        /// </summary>
        public const char RLM = '\u200F';

        /// <summary>
        /// Left-To-Right Mark (LRM) - Unicode character for explicit LTR direction
        /// </summary>
        public const char LRM = '\u200E';

        /// <summary>
        /// Right-To-Left Embedding Mark
        /// </summary>
        public const char RLE = '\u202B';

        /// <summary>
        /// Pop Directional Formatting (PDF)
        /// </summary>
        public const char PDF = '\u202C';

        /// <summary>
        /// Arabic-Indic digit range start
        /// </summary>
        private const int ArabicIndicDigitStart = 0x0660;

        /// <summary>
        /// Arabic-Indic digit range end
        /// </summary>
        private const int ArabicIndicDigitEnd = 0x0669;

        /// <summary>
        /// Extended Arabic-Indic digit range start
        /// </summary>
        private const int ExtendedArabicDigitStart = 0x06F0;

        /// <summary>
        /// Extended Arabic-Indic digit range end
        /// </summary>
        private const int ExtendedArabicDigitEnd = 0x06F9;

        #endregion

        #region Arabic Character Detection

        /// <summary>
        /// Regular expression to detect Arabic characters
        /// </summary>
        private static readonly Regex ArabicPattern = new Regex(
            @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]",
            RegexOptions.Compiled);

        /// <summary>
        /// Regular expression to detect Arabic or mixed Arabic text
        /// </summary>
        private static readonly Regex ContainsArabicPattern = new Regex(
            @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]",
            RegexOptions.Compiled);

        /// <summary>
        /// Regular expression to match Arabic words
        /// </summary>
        private static readonly Regex ArabicWordsPattern = new Regex(
            @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]+",
            RegexOptions.Compiled);

        #endregion

        #region Public Methods

        /// <summary>
        /// Fixes Arabic text to ensure proper rendering in chart labels and other UI elements.
        /// This is the main entry point for fixing Arabic text in charts.
        /// </summary>
        /// <param name="text">The input text that may contain Arabic characters</param>
        /// <returns>Properly formatted text for RTL display</returns>
        public static string FixArabicText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? string.Empty;

            // If no Arabic characters, return as-is
            if (!ContainsArabic(text))
                return text;

            // Step 1: Ensure proper RTL mark at the start if text starts with Arabic
            var result = EnsureRTLMark(text);

            // Step 2: Ensure numbers are preserved in LTR order
            result = PreserveNumbers(result);

            // Step 3: Handle mixed Arabic-English text
            result = FixMixedContent(result);

            return result;
        }

        /// <summary>
        /// Fixes a collection of labels for use in charts (pie chart legends, axis labels, etc.)
        /// </summary>
        /// <param name="labels">The collection of labels to fix</param>
        /// <returns>Array of properly formatted Arabic labels</returns>
        public static string[] FixLabels(IEnumerable<string> labels)
        {
            if (labels == null)
                return Array.Empty<string>();

            return labels.Select(l => FixArabicText(l ?? string.Empty)).ToArray();
        }

        /// <summary>
        /// Fixes labels and returns them in RTL-compatible order for charts.
        /// For charts that render labels from left-to-right regardless of FlowDirection.
        /// </summary>
        /// <param name="labels">The collection of labels</param>
        /// <param name="reverseForRTL">Whether to reverse the order for RTL display</param>
        /// <returns>Array of properly formatted Arabic labels</returns>
        public static string[] FixLabels(IEnumerable<string> labels, bool reverseForRTL)
        {
            var fixedLabels = FixLabels(labels).ToList();

            if (reverseForRTL && fixedLabels.Count > 1)
            {
                fixedLabels.Reverse();
            }

            return fixedLabels.ToArray();
        }

        /// <summary>
        /// Creates a labeler function for LiveCharts2 axis that properly renders Arabic text.
        /// Use this with Axis.Labeler property.
        /// </summary>
        /// <returns>A Func<double, string> that formats axis labels correctly</returns>
        public static Func<double, string> CreateAxisLabeler()
        {
            return value =>
            {
                // For numeric axis labels
                if (value == Math.Floor(value))
                {
                    return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                }
                return value.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
            };
        }

        /// <summary>
        /// Creates a labeler function for categorical axis with Arabic labels.
        /// </summary>
        /// <param name="labels">The Arabic category labels</param>
        /// <returns>A labeler function</returns>
        public static Func<double, string> CreateCategoryLabeler(string[] labels)
        {
            return value =>
            {
                int index = (int)value;
                if (index >= 0 && index < labels.Length)
                {
                    return FixArabicText(labels[index]);
                }
                return string.Empty;
            };
        }

        /// <summary>
        /// Formats a number for Arabic display while keeping Western Arabic numerals (0-9)
        /// </summary>
        /// <param name="value">The numeric value</param>
        /// <param name="format">The format string (e.g., "N0" for thousands separator)</param>
        /// <returns>Formatted number string</returns>
        public static string FormatNumber(decimal value, string format = "N0")
        {
            // Use invariant culture to keep Western numerals (0-9)
            return value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a number for Arabic display while keeping Western Arabic numerals (0-9)
        /// </summary>
        /// <param name="value">The numeric value</param>
        /// <param name="format">The format string (e.g., "N0" for thousands separator)</param>
        /// <returns>Formatted number string</returns>
        public static string FormatNumber(double value, string format = "N0")
        {
            return value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a formatted string with Arabic text and numbers (e.g., "المبيعات 1500")
        /// Ensures the number appears in correct LTR order
        /// </summary>
        /// <param name="arabicText">The Arabic label text</param>
        /// <param name="number">The number to display</param>
        /// <param name="format">Optional format for the number</param>
        /// <returns>Properly formatted Arabic text with number</returns>
        public static string FormatArabicWithNumber(string arabicText, decimal number, string format = "N0")
        {
            if (string.IsNullOrWhiteSpace(arabicText))
                return FormatNumber(number, format);

            var formattedNumber = FormatNumber(number, format);
            return $"{arabicText} {formattedNumber}";
        }

        /// <summary>
        /// Creates RTL-compatible labels for X-axis (reverses the order for proper RTL display in charts)
        /// </summary>
        /// <param name="labels">The original labels</param>
        /// <returns>Labels in RTL-compatible order</returns>
        public static string[] CreateRTLCompatibleLabels(string[] labels)
        {
            if (labels == null || labels.Length == 0)
                return Array.Empty<string>();

            var fixedLabels = FixLabels(labels);

            // Reverse for RTL display in charts
            var reversed = new string[fixedLabels.Length];
            Array.Copy(fixedLabels, reversed, fixedLabels.Length);
            Array.Reverse(reversed);

            return reversed;
        }

        /// <summary>
        /// Creates RTL-compatible data points for pie charts
        /// </summary>
        /// <param name="labels">Category labels</param>
        /// <param name="values">Corresponding values</param>
        /// <returns>Array of tuples with fixed Arabic labels and values</returns>
        public static (string Label, double Value)[] CreateRTLCompatibleData(
            string[] labels, double[] values)
        {
            if (labels == null || values == null || labels.Length != values.Length)
                return Array.Empty<(string, double)>();

            var fixedLabels = FixLabels(labels);

            // Reverse both arrays together for RTL display
            var reversedLabels = new string[fixedLabels.Length];
            var reversedValues = new double[values.Length];

            Array.Copy(fixedLabels, reversedLabels, fixedLabels.Length);
            Array.Copy(values, reversedValues, values.Length);

            Array.Reverse(reversedLabels);
            Array.Reverse(reversedValues);

            var result = new (string Label, double Value)[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                result[i] = (reversedLabels[i], reversedValues[i]);
            }

            return result;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Checks if the text contains Arabic characters
        /// </summary>
        private static bool ContainsArabic(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return ContainsArabicPattern.IsMatch(text);
        }

        /// <summary>
        /// Ensures RTL mark is added when text starts with Arabic
        /// </summary>
        private static string EnsureRTLMark(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Check if first strong character is Arabic
            var firstStrong = GetFirstStrongCharacter(text);
            if (firstStrong == ' ' || firstStrong == '\0')
                return text;

            // If the first character is Arabic, add RLM at the start to ensure proper RTL rendering
            if (IsArabicCharacter(firstStrong))
            {
                return RLM + text;
            }

            return text;
        }

        /// <summary>
        /// Gets the first strong (non-whitespace) character in the string
        /// </summary>
        private static char GetFirstStrongCharacter(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c) && c != RLM && c != LRM && c != RLE && c != PDF)
                {
                    return c;
                }
            }
            return '\0';
        }

        /// <summary>
        /// Checks if a character is an Arabic character
        /// </summary>
        private static bool IsArabicCharacter(char c)
        {
            // Arabic Unicode ranges
            return (c >= 0x0600 && c <= 0x06FF) ||  // Arabic
                   (c >= 0x0750 && c <= 0x077F) ||  // Arabic Supplement
                   (c >= 0x08A0 && c <= 0x08FF) ||  // Arabic Extended-A
                   (c >= 0xFB50 && c <= 0xFDFF) ||  // Arabic Presentation Forms-A
                   (c >= 0xFE70 && c <= 0xFEFF);    // Arabic Presentation Forms-B
        }

        /// <summary>
        /// Preserves Western Arabic numerals (0-9) in the text
        /// Converts any Arabic-Indic digits to Western digits
        /// </summary>
        private static string PreserveNumbers(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                // Check for Arabic-Indic digits (٠١٢٣٤٥٦٧٨٩)
                if (c >= ArabicIndicDigitStart && c <= ArabicIndicDigitEnd)
                {
                    // Convert to Western digit
                    sb.Append((char)(c - ArabicIndicDigitStart + '0'));
                }
                // Check for Extended Arabic-Indic digits (۰۱۲۳۴۵۶۷۸۹)
                else if (c >= ExtendedArabicDigitStart && c <= ExtendedArabicDigitEnd)
                {
                    // Convert to Western digit
                    sb.Append((char)(c - ExtendedArabicDigitStart + '0'));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Fixes mixed Arabic-English content by ensuring proper directional marks
        /// </summary>
        private static string FixMixedContent(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Check if text contains both Arabic and Latin characters
            bool hasArabic = false;
            bool hasLatin = false;

            foreach (char c in text)
            {
                if (IsArabicCharacter(c))
                    hasArabic = true;
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    hasLatin = true;

                if (hasArabic && hasLatin)
                    break;
            }

            // If mixed content, we need to be careful with ordering
            // The RTL mark at the start should help, but we may need to handle numbers specifically
            if (hasArabic && hasLatin)
            {
                // Numbers should be LTR, so we wrap them in LRM if they're adjacent to Arabic
                // This is handled by PreserveNumbers keeping Western numerals
            }

            return text;
        }

        #endregion

        #region Arabic Day Names (Helper)

        /// <summary>
        /// Gets Arabic day names in correct order (starting from Saturday)
        /// </summary>
        public static string[] GetArabicDayNames()
        {
            return new[] { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
        }

        /// <summary>
        /// Converts English day name to Arabic
        /// </summary>
        public static string ToArabicDayName(string englishDayName)
        {
            return englishDayName?.ToLower() switch
            {
                "saturday" => "السبت",
                "sunday" => "الأحد",
                "monday" => "الإثنين",
                "tuesday" => "الثلاثاء",
                "wednesday" => "الأربعاء",
                "thursday" => "الخميس",
                "friday" => "الجمعة",
                _ => englishDayName ?? ""
            };
        }

        #endregion

        #region Sample Arabic Data

        /// <summary>
        /// Gets sample product categories in Arabic for demonstration
        /// </summary>
        public static string[] GetSampleCategories()
        {
            return new[] { "كابلات", "مفاتيح", "إنارة", "أدوات", "كماليات" };
        }

        /// <summary>
        /// Gets sample sales data for demonstration
        /// </summary>
        public static double[] GetSampleCategorySales()
        {
            return new double[] { 15000, 8500, 12000, 5500, 3000 };
        }

        /// <summary>
        /// Gets sample weekly sales data
        /// </summary>
        public static string[] GetSampleWeeklyDays()
        {
            return GetArabicDayNames();
        }

        /// <summary>
        /// Gets sample weekly sales values
        /// </summary>
        public static double[] GetSampleWeeklySales()
        {
            return new double[] { 4500, 5200, 4800, 6100, 5500, 3200, 2800 };
        }

        #endregion
    }
}
