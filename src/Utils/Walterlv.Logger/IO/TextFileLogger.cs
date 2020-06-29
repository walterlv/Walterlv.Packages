using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.IO
{
    /// <summary>
    /// 提供记录到文本文件的日志。
    /// </summary>
    public class TextFileLogger : AsyncOutputLogger, IDisposable
    {
        private readonly string _lineEnd;
        private readonly FileInfo _infoLogFile;
        private readonly FileInfo _errorLogFile;
        private readonly bool _shouldAppendInfo;
        private readonly bool _shouldAppendError;
        private Mutex? _infoMutex;
        private Mutex? _errorMutex;

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// </summary>
        /// <param name="logFile">日志文件。</param>
        /// <param name="append">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo logFile, bool append = false, string lineEnd = "\n")
        {
            if (logFile is null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            _infoLogFile = logFile;
            _errorLogFile = logFile;
            _lineEnd = VerifyLineEnd(lineEnd);
            _shouldAppendInfo = append;
            _shouldAppendError = append;
        }

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。信息文件包含用于诊断的所有信息，而错误文件包含代码中无法预知的错误记录。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。</param>
        /// <param name="errorLogFile">错误日志文件。</param>
        /// <param name="shouldAppendInfo">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="shouldAppendError">如果你希望每次创建同文件的新实例时追加到原来日志的末尾，则设为 true；如果希望覆盖之前的日志，则设为 false。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo infoLogFile, FileInfo errorLogFile,
            bool shouldAppendInfo = false, bool shouldAppendError = true, string lineEnd = "\n")
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
            var areSameFile = string.Equals(infoLogFile.FullName, errorLogFile.FullName, StringComparison.OrdinalIgnoreCase);
            _infoLogFile = infoLogFile;
            _errorLogFile = areSameFile ? infoLogFile : errorLogFile;
            _shouldAppendInfo = shouldAppendInfo;
            _shouldAppendError = shouldAppendError;
        }

        /// <inheritdoc />
        protected override Task OnInitializedAsync()
        {
            if (_isDisposed)
            {
                return Task.FromResult<object?>(null);
            }

            _infoMutex = new Mutex(false, _infoLogFile.FullName);
            _errorMutex = _errorLogFile == _infoLogFile
                ? _infoMutex
                : new Mutex(false, _errorLogFile.FullName);

            return Task.FromResult<object?>(null);
        }

        /// <inheritdoc />
        protected sealed override void OnLogReceived(in LogContext context)
        {
            if (_isDisposed)
            {
                return;
            }

            var infoMutex = _infoMutex!;
            var errorMutex = _errorMutex!;
            var areSameFile = infoMutex == errorMutex;
            if (!areSameFile && context.CurrentLevel <= LogLevel.Error)
            {
                // 写入日志的主要部分。
                Write(infoMutex, _infoLogFile, BuildLogText(in context, containsExtraInfo: false, _lineEnd));

                // 写入日志的扩展部分。
                Write(errorMutex, _errorLogFile, BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
            }
            else
            {
                Write(infoMutex, _infoLogFile, BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
            }
        }

        /// <summary>
        /// 派生类重写此方法时，将单条日志格式化成一端可被记录到文件的字符串。
        /// </summary>
        /// <param name="context">单条日志信息。</param>
        /// <param name="containsExtraInfo">此次格式化时，是否应该包含额外的日志信息。</param>
        /// <param name="lineEnd">记录到文件时应该使用的行尾符号。</param>
        /// <returns>格式化后的日志文本。</returns>
        protected virtual string BuildLogText(in LogContext context, bool containsExtraInfo, string lineEnd)
        {
            var time = context.Time.ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var member = context.CallerMemberName;
            var text = context.Text;
            string? extraInfo = null;
            if (containsExtraInfo && context.ExtraInfo != null)
            {
                extraInfo = context.ExtraInfo;
            }
            return extraInfo is null
                ? $@"[{time}][{member}] {text}"
                : $@"[{time}][{member}] {text}{lineEnd}{extraInfo}";
        }

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

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _infoMutex?.Dispose();
                    _errorMutex?.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// 将日志最后的缓冲写完后关闭日志记录，然后回收所有资源。
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// 将日志最后的缓冲写完后关闭日志记录，然后回收所有资源。
        /// </summary>
        public void Close() => Dispose(true);

        private static void Write(Mutex mutex, FileInfo file, params string[] texts)
        {
            try
            {
                mutex.WaitOne();
            }
            catch (AbandonedMutexException ex)
            {
                // 发现有进程意外退出后，遗留了锁。
                // 此时已经拿到了锁。
                // 参见：https://blog.walterlv.com/post/mutex-in-dotnet.html
                texts = new string[] { $"Unexpected lock on this log file is detected. Abandoned index is {ex.MutexIndex}." }.Concat(texts).ToArray();
            }

            try
            {
                File.AppendAllLines(file.FullName, texts);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
