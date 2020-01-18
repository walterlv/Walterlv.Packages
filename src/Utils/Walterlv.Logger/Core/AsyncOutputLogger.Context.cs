using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Walterlv.Logging.Core
{
    partial class AsyncOutputLogger
    {
        [StructLayout(LayoutKind.Auto)]
        protected readonly struct Context
        {
            private readonly bool _isExtraInfoSelfFormatted;

            public Context(DateTimeOffset time, string callerMemberName, string text, string? extraInfo, bool isExtraInfoSelfFormatted, LogLevel currentLevel)
            {
                Time = time;
                CallerMemberName = callerMemberName ?? throw new ArgumentNullException(nameof(callerMemberName));
                Text = text ?? throw new ArgumentNullException(nameof(text));
                ExtraInfo = extraInfo;
                _isExtraInfoSelfFormatted = isExtraInfoSelfFormatted;
                CurrentLevel = currentLevel;
            }

            public string Text { get; }

            public string? ExtraInfo { get; }

            public LogLevel CurrentLevel { get; }

            public DateTimeOffset Time { get; }

            public string CallerMemberName { get; }

            public string BuildLogText(string lineEnd = "\n", bool containsExtraInfo = true, string? extraInfoFormatIfNotFormatted = null)
            {
                lineEnd = VerifyLineEnd(lineEnd);
                var time = Time.ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                var member = CallerMemberName;
                var text = CurrentLevel switch
                {
                    LogLevel.Detail => Text,
                    LogLevel.Message => Text,
                    LogLevel.Warning => $"**{Text}**",
                    LogLevel.Error => $"**{Text}**",
                    LogLevel.Fatal => $"**{Text}** *致命错误，异常中止*",
                    _ => Text,
                };
                string? extraInfo = null;
                if (containsExtraInfo && ExtraInfo != null)
                {
                    extraInfo = _isExtraInfoSelfFormatted || extraInfoFormatIfNotFormatted is null
                        ? ExtraInfo
                        : string.Format(extraInfoFormatIfNotFormatted, ExtraInfo);
                }
                return extraInfo is null
                    ? $@"[{time}][{member}] {text}"
                    : $@"[{time}][{member}] {text}{lineEnd}{extraInfo}";
            }

            public override bool Equals(object? obj)
            {
                return obj is Context context &&
                       string.Equals(Text, context.Text, StringComparison.Ordinal) &&
                       CurrentLevel == context.CurrentLevel &&
                       Time.Equals(context.Time) &&
                       string.Equals(CallerMemberName, context.CallerMemberName, StringComparison.Ordinal);
            }

            public override int GetHashCode()
            {
                var hashCode = 299888417;
                hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(Text);
                hashCode = hashCode * -1521134295 + CurrentLevel.GetHashCode();
                hashCode = hashCode * -1521134295 + Time.GetHashCode();
                hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(CallerMemberName);
                return hashCode;
            }

            public static bool operator ==(Context left, Context right) => left.Equals(right);

            public static bool operator !=(Context left, Context right) => !(left == right);

            /// <summary>
            /// 检查字符串是否是行尾符号，如果不是则抛出异常。
            /// </summary>
            /// <param name="lineEnd">行尾符号。</param>
            /// <returns>行尾符号。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static string VerifyLineEnd(string lineEnd) => lineEnd switch
            {
                null => throw new ArgumentNullException(nameof(lineEnd)),
                "\n" => "\n",
                "\r" => "\r",
                "\r\n" => "\r\n",
                _ => throw new ArgumentException("虽然你可以指定行尾符号，但也只能是 \\n、\\r 或者 \\r\\n。", nameof(lineEnd))
            };
        }
    }
}
