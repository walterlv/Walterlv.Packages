using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Walterlv.Logging.Markdown
{
    /// <summary>
    /// 提供 Markdown 格式的日志记录。
    /// </summary>
    public sealed partial class MarkdownLogger : ILogger
    {
        private readonly AsyncQueue<string> _infoQueue;
        private readonly AsyncQueue<string>? _errorQueue;
        private readonly string _lineEnd;

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// </summary>
        /// <param name="logFile">日志文件。如果你希望有 Markdown 的语法高亮，建议指定后缀为 .md。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public MarkdownLogger(FileInfo logFile, bool append = false, string lineEnd = "\n")
        {
            if (logFile is null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            _lineEnd = VerifyLineEnd(lineEnd);
            _infoQueue = new AsyncQueue<string>();
            StartWriteLogFile(logFile, _infoQueue, append);
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
        {
            if (infoLogFile is null)
            {
                throw new ArgumentNullException(nameof(infoLogFile));
            }

            if (errorLogFile is null)
            {
                throw new ArgumentNullException(nameof(errorLogFile));
            }

            _lineEnd = VerifyLineEnd(lineEnd);
            _infoQueue = new AsyncQueue<string>();
            _errorQueue = new AsyncQueue<string>();
            StartWriteLogFile(infoLogFile, _infoQueue, append);
            StartWriteLogFile(errorLogFile, _errorQueue, append);
        }

        /// <summary>
        /// 获取当前时间的格式化字符串。
        /// </summary>
        private string Now => DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        /// <summary>
        /// 获取或设置日志的记录等级。
        /// 你可以在日志记录的过程当中随时修改日志等级，修改后会立刻生效。
        /// 默认是所有调用日志记录的方法都全部记录。
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Detail;

        /// <inheritdoc />
        public void Trace(string text, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.Detail)
            {
                return;
            }

            _infoQueue.Enqueue($"[{Now}][{callerMemberName}] {text}");
        }

        /// <inheritdoc />
        public void Message(string text, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.Message)
            {
                return;
            }

            _infoQueue.Enqueue($"[{Now}][{callerMemberName}] {text}");
        }

        /// <inheritdoc />
        public void Warning(string message, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.Warning)
            {
                return;
            }

            _infoQueue.Enqueue($"[{Now}][{callerMemberName}] **{message}**");
        }

        /// <inheritdoc />
        public void Error(string message, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.ErrorAndFatal)
            {
                return;
            }

            if (_errorQueue is null)
            {
                // 如果使用单个日志文件，则将日志作为警告级别输出。
                _infoQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}**");
            }
            else
            {
                // 如果使用多个日志文件，则在信息文件和错误文件中都以警告级别输出。
                _infoQueue.Enqueue($"[{Now}][{callerMemberName}] **{message}**");
                _errorQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}**");
            }
        }

        /// <inheritdoc />
        public void Error(Exception exception, string? message = null, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.ErrorAndFatal)
            {
                return;
            }

            if (_errorQueue is null)
            {
                // 如果使用单个日志文件，则将异常信息与常规日志放到一起。
                _infoQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}**
```csharp
{exception.ToString()}
```");
            }
            else
            {
                // 如果使用多个日志文件，则在信息文件中简单输出发生了异常，在错误文件中输出异常的详细信息。
                _infoQueue.Enqueue($"[{Now}][{callerMemberName}] **{message}** *请参见错误日志*");
                _errorQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}**
```csharp
{exception.ToString()}
```
");
            }
        }

        /// <inheritdoc />
        public void Fatal(Exception exception, string message, [CallerMemberName] string? callerMemberName = null)
        {
            if (Level < LogLevel.Fatal)
            {
                return;
            }

            if (_errorQueue is null)
            {
                // 如果使用单个日志文件，则将崩溃信息与常规日志放到一起。
                _infoQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}** *致命错误，异常中止*
```csharp
{exception.ToString()}
```");
            }
            else
            {
                // 如果使用多个日志文件，则在信息文件中简单输出崩溃，在错误文件中输出崩溃的详细信息。
                _infoQueue.Enqueue($"[{Now}][{callerMemberName}] **{message}** *致命性错误，异常中止，请参见错误日志*");
                _errorQueue.Enqueue($@"[{Now}][{callerMemberName}] **{message}** *致命错误，异常中止*
```csharp
{exception.ToString()}
```
");
            }
        }

        /// <summary>
        /// 开始异步写入日志文件。
        /// </summary>
        /// <param name="file">日志文件。</param>
        /// <param name="logQueue">要接收日志的异步队列。</param>
        /// <param name="append">是追加文件还是覆盖文件。</param>
        private async void StartWriteLogFile(FileInfo file, AsyncQueue<string> logQueue, bool append)
        {
            var directory = file.Directory;
            if (directory != null && !Directory.Exists(directory.FullName))
            {
                directory.Create();
            }
            var writer = new Lazy<StreamWriter>(() => new StreamWriter(file.FullName, append, Encoding.UTF8)
            {
                AutoFlush = true,
                NewLine = _lineEnd,
            }, LazyThreadSafetyMode.ExecutionAndPublication);
            while (true)
            {
                var text = await logQueue.DequeueAsync().ConfigureAwait(false);
                writer.Value.WriteLine(text);
            }
        }

        /// <summary>
        /// 检查字符串是否是行尾符号，如果不是则抛出异常。
        /// </summary>
        /// <param name="lineEnd">行尾符号。</param>
        /// <returns>行尾符号。</returns>
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
