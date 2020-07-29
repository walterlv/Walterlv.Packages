using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Walterlv.IO
{
    /// <summary>
    /// 为文件名提供辅助方法。
    /// </summary>
    public static class FileNameHelper
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// 生成安全的文件名。字符串 <paramref name="text"/> 中的不合法字符将被替换成指定字符。
        /// </summary>
        /// <param name="text">要生成安全文件名的原始文件名。</param>
        /// <param name="replacement">当遇到不能成为文件名的字符的时候应该替换的字符。</param>
        /// <returns>安全的文件名。（不包含不合法的字符，但如果你的 <paramref name="text"/> 是空格，可能需要检查最终文件名是否是空白字符串。）</returns>
        public static string MakeSafeFileName(string text, char replacement = ' ')
        {
            var chars = text.ToCharArray();
            var invalidChars = InvalidFileNameChars;
            for (var i = 0; i < chars.Length; i++)
            {
                for (var j = 0; j < invalidChars.Length; j++)
                {
                    if (chars[i] == invalidChars[j])
                    {
                        chars[i] = replacement;
                        break;
                    }
                }
            }
            return new string(chars);
        }

        /// <summary>
        /// 从 URL 中猜文件名。
        /// </summary>
        /// <param name="url">要猜测文件名的 URL 来源字符串。</param>
        /// <param name="limitedFileNameLength">如果需要，可以限制最终生成文件名的长度。</param>
        /// <param name="fallbackName">当无法猜出文件名，或文件名长度过长时，将取此名字。</param>
        /// <returns>猜出的文件名。</returns>
#if NETCOREAPP3_0 || NETCOREAPP3_1 || NETCOREAPP5_0 || NET5_0 || NET6_0
        [return: NotNullIfNotNull("fallbackName")]
#endif
        public static string? GuessFileNameFromUrl(string url, int? limitedFileNameLength = null, string? fallbackName = null)
        {
            var lastSlash = url.LastIndexOf('/') + 1;
            var lastQuery = url.IndexOf('?');
            if (lastSlash < 0)
            {
                return fallbackName;
            }

            // 取 URL 中可能是文件名的部分。
            var name = lastQuery < 0 ? url.Substring(lastSlash) : url.Substring(lastSlash, lastQuery - lastSlash);

            // 对 URL 反转义。
            var unescapedName = Uri.UnescapeDataString(name);

            // 限制文件名长度。
            string? limitedFileName = limitedFileNameLength is null
                ? unescapedName
                : unescapedName.Length <= limitedFileNameLength.Value ? unescapedName : fallbackName;

            // 确保文件名字符是安全的。
            string? safeFileName = limitedFileName is null
                ? limitedFileName
                : FileNameHelper.MakeSafeFileName(limitedFileName);
            return safeFileName;
        }
    }
}
