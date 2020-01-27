using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Walterlv.Logging.IO;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 提供 Markdown 格式的日志记录。
    /// </summary>
    public sealed class MarkdownLogger : TextFileLogger
    {
        private const string ExceptionStackTraceFormatter = "```csharp\n{0}\n```";

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// </summary>
        /// <param name="logFile">日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public MarkdownLogger(FileInfo logFile, bool append = false, string lineEnd = "\n")
            : base(logFile, append, lineEnd, ExceptionStackTraceFormatter)
        {
        }

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。其中信息和警告在同一个文件，警告高亮；错误在另一个文件。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="errorLogFile">错误日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public MarkdownLogger(FileInfo infoLogFile, FileInfo errorLogFile, bool append = false, string lineEnd = "\n")
            : base(infoLogFile, errorLogFile, append, lineEnd, ExceptionStackTraceFormatter)
        {
        }
    }
}
