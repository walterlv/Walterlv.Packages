using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

using Walterlv.Logging.Core;
using Walterlv.Logging.IO;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 提供 Markdown 格式的日志记录。
    /// </summary>
    public sealed class MarkdownLogger : TextFileLogger
    {
        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// </summary>
        /// <param name="logFile">日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public MarkdownLogger(FileInfo logFile, string lineEnd = "\n")
            : base(logFile, lineEnd)
        {
        }

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。其中信息和警告在同一个文件，警告高亮；错误在另一个文件。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="errorLogFile">错误日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public MarkdownLogger(FileInfo infoLogFile, FileInfo errorLogFile, string lineEnd = "\n")
            : base(infoLogFile, errorLogFile, lineEnd)
        {
        }

        /// <inheritdoc />
        protected override string BuildLogText(in LogContext context, bool containsExtraInfo, string lineEnd)
        {
            var time = context.Time.ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var member = context.CallerMemberName;
            var text = context.CurrentLevel switch
            {
                LogLevel.Detail => context.Text,
                LogLevel.Message => context.Text,
                LogLevel.Warning => $"**{context.Text}**",
                LogLevel.Error => $"**{context.Text}**",
                LogLevel.Fatal => $"**{context.Text}** *致命错误，异常中止*",
                _ => context.Text,
            };
            string? extraInfo = null;
            if (containsExtraInfo && context.ExtraInfo != null)
            {
                extraInfo = context.ExtraInfo.StartsWith("```", StringComparison.Ordinal)
                    ? $"```csharp{lineEnd}{context.ExtraInfo}{lineEnd}```"
                    : context.ExtraInfo;
            }
            return extraInfo is null
                ? $@"[{time}][{member}] {text}"
                : $@"[{time}][{member}] {text}{lineEnd}{extraInfo}";
        }

        /// <summary>
        /// 不再支持。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("不再使用 append 参数决定日志是否保留，请使用 new MarkdownLogger().WithWholeFileOverride() 替代。")]
        public MarkdownLogger(FileInfo logFile, bool append, string lineEnd)
            : base(logFile, append, lineEnd)
        {
        }

        /// <summary>
        /// 不再支持。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("不再使用 append 参数决定日志是否保留，请使用 new MarkdownLogger().WithWholeFileOverride() 替代。")]
        public MarkdownLogger(FileInfo infoLogFile, FileInfo errorLogFile,
            bool shouldAppendInfo, bool shouldAppendError, string lineEnd)
            : base(infoLogFile, errorLogFile, shouldAppendInfo, shouldAppendError, lineEnd)
        {
        }
    }
}
