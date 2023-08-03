using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Walterlv.ConsoleExtensions.Utils;
/// <summary>
/// 提供 <see cref="string"/> 的控制台相关扩展方法。
/// </summary>
public static class ConsoleStringExtensions
{
    /// <summary>
    /// 获取字符串在控制台中的长度。
    /// </summary>
    /// <param name="str">要获取长度的字符串。</param>
    /// <returns>字符串在控制台中的长度。</returns>
    public static int GetConsoleLength(this string str)
    {
        if (str is null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        var totalLength = 0;
        for (var i = 0; i < str.Length; i++)
        {
            totalLength += str[i].GetConsoleLength();
        }
        return totalLength;
    }

    /// <summary>
    /// 在控制台中，将字符串填充到指定长度，如果超过则截断，如果不足则填充。
    /// </summary>
    /// <param name="str">要填充或截断的字符串。</param>
    /// <param name="totalWidth">要填充到的长度。</param>
    /// <param name="paddingChar">填充的字符。</param>
    /// <param name="trimEndIfExceeds">如果超过长度，是否截断。</param>
    /// <returns>填充或截断后的字符串。</returns>
    public static string ConsolePadRight(this string str, int totalWidth, char paddingChar, bool trimEndIfExceeds)
    {
        if (str is null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (totalWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalWidth));
        }

        var consoleLength = str.GetConsoleLength();
        if (consoleLength > totalWidth)
        {
            if (trimEndIfExceeds)
            {
                var sb = new StringBuilder(str);
                for (var i = str.Length - 1; i >= 0; i--)
                {
                    consoleLength -= sb[i].GetConsoleLength();
                    sb.Length = i;
                    if (consoleLength <= totalWidth)
                    {
                        var paddingCount = totalWidth - consoleLength;
                        sb.Append(paddingChar, paddingCount);
                        return sb.ToString();
                    }
                }
            }
        }
        else if (consoleLength < totalWidth)
        {
            var sb = new StringBuilder(str);
            var paddingCount = totalWidth - consoleLength;
            sb.Append(paddingChar, paddingCount);
            return sb.ToString();
        }

        return str;
    }

    /// <summary>
    /// 获取字符在控制台中的长度。
    /// </summary>
    /// <param name="c">要获取长度的字符。</param>
    /// <returns>字符在控制台中的长度。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetConsoleLength(this char c)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.OtherLetter)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
}
