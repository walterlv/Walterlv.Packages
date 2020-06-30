using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
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
        private Mutex? _infoMutex;
        private Mutex? _errorMutex;
        private object _disposeLocker = new object();
        private bool _isDisposed;

        /// <summary>
        /// 针对文件的拦截器。第一个参数是文件，第二个参数是此文件所对应的日志等级，只有两种值：<see cref="LogLevel.Message"/> 和 <see cref="LogLevel.Error"/>。
        /// </summary>
        private Action<FileInfo, LogLevel>? _fileInterceptor;

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// </summary>
        /// <param name="logFile">日志文件。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo logFile, string lineEnd = "\n")
        {
            if (logFile is null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            _infoLogFile = logFile;
            _errorLogFile = logFile;
            _lineEnd = VerifyLineEnd(lineEnd);
        }

        /// <summary>
        /// 创建文本文件记录日志的 <see cref="TextFileLogger"/> 的新实例。
        /// 在记录的时候，信息/警告和错误是分开成两个文件的。信息文件包含用于诊断的所有信息，而错误文件包含代码中无法预知的错误记录。
        /// </summary>
        /// <param name="infoLogFile">信息和警告的日志文件。</param>
        /// <param name="errorLogFile">错误日志文件。</param>
        /// <param name="lineEnd">行尾符号。默认是 \n，如果你愿意，也可以改为 \r\n 或者 \r。</param>
        public TextFileLogger(FileInfo infoLogFile, FileInfo errorLogFile, string lineEnd = "\n")
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
        }

        /// <summary>
        /// 请求在写入首条日志前针对日志文件执行一些代码。代码可能在任一线程中执行，但确保不会并发。
        /// </summary>
        /// <param name="fileInterceptor">
        /// 针对某文件的拦截器。
        /// 第一个参数是文件，第二个参数是此文件所对应的日志等级，只有两种值，对应此日志文件中能记录信息的最严重范围：
        ///  - <see cref="LogLevel.Warning"/> 表示此日志最严重只记到警告。
        ///  - <see cref="LogLevel.Fatal"/> 表示此日志最严重记到崩溃。
        /// </param>
        internal void AddInitializeInterceptor(Action<FileInfo, LogLevel> fileInterceptor)
        {
            if (_infoMutex != null)
            {
                throw new InvalidOperationException("已经有日志开始输出了，不可再继续配置日志行为。");
            }

            _fileInterceptor += fileInterceptor ?? throw new ArgumentNullException(nameof(fileInterceptor));
        }

        /// <inheritdoc />
        protected override Task OnInitializedAsync()
        {
            if (_isDisposed)
            {
                return Task.FromResult<object?>(null);
            }

            // 初始化文件写入安全区。
            var areSameFile = _errorLogFile == _infoLogFile;
            _infoMutex = CreateMutex(_infoLogFile);
            _errorMutex = areSameFile ? _infoMutex : CreateMutex(_errorLogFile);

            // 初始化文件。
            CriticalInvoke(_infoMutex, _fileInterceptor, interceptor => interceptor?.Invoke(_infoLogFile, LogLevel.Warning));
            if (!areSameFile)
            {
                CriticalInvoke(_errorMutex, _fileInterceptor, interceptor => interceptor?.Invoke(_errorLogFile, LogLevel.Fatal));
            }

            return Task.FromResult<object?>(null);

            static Mutex CreateMutex(FileInfo file) => new Mutex(
                false,
                Path.GetFullPath(file.FullName).ToLower(CultureInfo.InvariantCulture).Replace(Path.DirectorySeparatorChar, '_'));
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
            var areSameFile = _errorLogFile == _infoLogFile;
            if (!areSameFile && context.CurrentLevel <= LogLevel.Error)
            {
                // 写入日志的主要部分。
                CriticalWrite(infoMutex, _infoLogFile, BuildLogText(in context, containsExtraInfo: false, _lineEnd));

                // 写入日志的扩展部分。
                CriticalWrite(errorMutex, _errorLogFile, BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
            }
            else
            {
                CriticalWrite(infoMutex, _infoLogFile, BuildLogText(in context, context.ExtraInfo != null, _lineEnd));
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

        /// <summary>
        /// 派生类重写此方法以回收非托管资源。注意如果重写了此方法，必须在重写方法中调用基类方法。
        /// </summary>
        /// <param name="disposing">如果主动释放资源，请传入 true；如果被动释放资源（析构函数），请传入 false。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_disposeLocker)
            {
                if (disposing)
                {
                    try
                    {
                        WaitFlushingAsync().Wait();
                        _infoMutex?.Dispose();
                        _errorMutex?.Dispose();
                    }
                    finally
                    {
                        _isDisposed = true;
                    }
                }
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

        private static void CriticalWrite(Mutex mutex, FileInfo file, params string[] texts)
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

        private static void CriticalInvoke<T>(Mutex mutex, T? action, Action<T> invoker) where T : MulticastDelegate
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
                Debug.WriteLine($"Unexpected lock on this log file is detected. Abandoned index is {ex.MutexIndex}.");
            }

            try
            {
                if (action != null)
                {
                    var exceptions = new List<Exception>();
                    foreach (var a in action.GetInvocationList().Cast<T>())
                    {
                        try
                        {
                            invoker(a);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                    if (exceptions.Count == 1)
                    {
                        ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
                    }
                    else if (exceptions.Count > 1)
                    {
                        throw new AggregateException(exceptions);
                    }
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 不再支持。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("不再使用 append 参数决定日志是否保留，请使用 new TextFileLogger().WithWholeFileOverride() 替代。")]
        public TextFileLogger(FileInfo logFile, bool append, string lineEnd = "\n")
            : this(logFile, lineEnd) => this.WithWholeFileOverride(append);

        /// <summary>
        /// 不再支持。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("不再使用 append 参数决定日志是否保留，请使用 new TextFileLogger().WithWholeFileOverride() 替代。")]
        public TextFileLogger(FileInfo infoLogFile, FileInfo errorLogFile,
            bool shouldAppendInfo, bool shouldAppendError, string lineEnd = "\n")
            : this(infoLogFile, errorLogFile, lineEnd) => this.WithWholeFileOverride(shouldAppendInfo, shouldAppendError);
    }
}
