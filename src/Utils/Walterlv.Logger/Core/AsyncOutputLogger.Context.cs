using System;
using System.Runtime.InteropServices;

namespace Walterlv.Logging.Core
{
    partial class AsyncOutputLogger
    {
        /// <summary>
        /// 包含一条日志的所有信息。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        protected internal readonly struct Context
        {
            /// <summary>
            /// 创建一条日志上下文。
            /// </summary>
            /// <param name="time">当前日志的时间。</param>
            /// <param name="callerMemberName">当前日志所在的方法名称。</param>
            /// <param name="text">当前日志信息的文本。</param>
            /// <param name="extraInfo">当前日志的额外信息。</param>
            /// <param name="currentLevel">当前日志的记录等级。</param>
            internal Context(DateTimeOffset time, string callerMemberName, string text, string? extraInfo, LogLevel currentLevel)
            {
                Time = time;
                CallerMemberName = callerMemberName ?? throw new ArgumentNullException(nameof(callerMemberName));
                Text = text ?? throw new ArgumentNullException(nameof(text));
                ExtraInfo = extraInfo;
                CurrentLevel = currentLevel;
            }

            /// <summary>
            /// 获取此条日志的文本。
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// 获取此条日志的额外信息。如果不存在额外信息，则返回 null。
            /// </summary>
            public string? ExtraInfo { get; }

            /// <summary>
            /// 获取此条日志的记录等级。
            /// </summary>
            public LogLevel CurrentLevel { get; }

            /// <summary>
            /// 获取此条日志记录时的时间。
            /// </summary>
            public DateTimeOffset Time { get; }

            /// <summary>
            /// 获取此条日志记录所在的方法名称。
            /// </summary>
            public string CallerMemberName { get; }

            /// <summary>
            /// 比较另一个对象是否表示此对象同一个日志。
            /// </summary>
            /// <param name="obj">要比较的另一个对象。</param>
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

            /// <summary>
            /// 判断两条日志是否表示同一个日志。
            /// </summary>
            public static bool operator ==(Context left, Context right) => left.Equals(right);

            /// <summary>
            /// 判断两条日志是否表示不同日志。
            /// </summary>
            public static bool operator !=(Context left, Context right) => !(left == right);
        }
    }
}
