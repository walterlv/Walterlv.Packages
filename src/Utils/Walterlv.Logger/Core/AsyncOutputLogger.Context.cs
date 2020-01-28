using System;
using System.Runtime.InteropServices;

namespace Walterlv.Logging.Core
{
    partial class AsyncOutputLogger
    {
        [StructLayout(LayoutKind.Auto)]
        protected internal readonly struct Context
        {
            public Context(DateTimeOffset time, string callerMemberName, string text, string? extraInfo, LogLevel currentLevel)
            {
                Time = time;
                CallerMemberName = callerMemberName ?? throw new ArgumentNullException(nameof(callerMemberName));
                Text = text ?? throw new ArgumentNullException(nameof(text));
                ExtraInfo = extraInfo;
                CurrentLevel = currentLevel;
            }

            public string Text { get; }

            public string? ExtraInfo { get; }

            public LogLevel CurrentLevel { get; }

            public DateTimeOffset Time { get; }

            public string CallerMemberName { get; }

            public override bool Equals(object? obj)
            {
                return obj is Context context &&
                       string.Equals(Text, context.Text, StringComparison.Ordinal) &&
                       string.Equals(ExtraInfo, context.ExtraInfo, StringComparison.Ordinal) &&
                       CurrentLevel == context.CurrentLevel &&
                       Time.Equals(context.Time) &&
                       string.Equals(CallerMemberName, context.CallerMemberName, StringComparison.Ordinal);
            }

            public override int GetHashCode()
            {
                var hashCode = 782125786;
                hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(Text);
                hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(ExtraInfo);
                hashCode = hashCode * -1521134295 + CurrentLevel.GetHashCode();
                hashCode = hashCode * -1521134295 + Time.GetHashCode();
                hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(CallerMemberName);
                return hashCode;
            }

            public static bool operator ==(Context left, Context right) => left.Equals(right);

            public static bool operator !=(Context left, Context right) => !(left == right);
        }
    }
}
