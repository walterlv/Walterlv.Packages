using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Walterlv.Logging.Core
{
    /// <summary>
    /// 为异步的日志记录提供公共基类。
    /// </summary>
    public abstract partial class AsyncOutputLogger : ILogger
    {
        private bool _isInitialized;
        private readonly AsyncQueue<Context> _queue;

        /// <summary>
        /// 创建 Markdown 格式的日志记录实例。
        /// </summary>
        protected AsyncOutputLogger()
        {
            _queue = new AsyncQueue<Context>();
            StartLogging();
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
        public virtual LogLevel Level { get; set; } = LogLevel.Message;

        /// <inheritdoc />
        public void Trace(string? text, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Detail, null, callerMemberName);

        /// <inheritdoc />
        public void Message(string? text, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Message, null, callerMemberName);

        /// <inheritdoc />
        public void Warning(string? text, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Warning, null, callerMemberName);

        /// <inheritdoc />
        public void Error(string? text, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Error, null, callerMemberName);

        /// <inheritdoc />
        public void Error(Exception exception, string? text = null, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Error, exception.ToString(), callerMemberName);

        /// <inheritdoc />
        public void Fatal(Exception exception, string? text, [CallerMemberName] string? callerMemberName = null)
            => LogCore(text, LogLevel.Error, exception.ToString(), callerMemberName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogCore(string? text, LogLevel currentLevel,
            string? extraInfo, [CallerMemberName] string? callerMemberName = null)
        {
            if (callerMemberName is null)
            {
                throw new ArgumentNullException(nameof(callerMemberName), "不允许显式将 CallerMemberName 指定成 null。");
            }

            if (string.IsNullOrWhiteSpace(callerMemberName))
            {
                throw new ArgumentException("不允许显式将 CallerMemberName 指定成空字符串。", nameof(callerMemberName));
            }

            if (Level < currentLevel)
            {
                return;
            }

            _queue.Enqueue(new Context(DateTimeOffset.Now, callerMemberName, text ?? "", extraInfo, currentLevel));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void LogCore(in Context context)
        {
            if (string.IsNullOrWhiteSpace(context.CallerMemberName))
            {
                throw new ArgumentException("不允许显式将 CallerMemberName 指定成 null 或空字符串。", nameof(Context.CallerMemberName));
            }

            if (Level < context.CurrentLevel)
            {
                return;
            }

            _queue.Enqueue(context);
        }

        /// <summary>
        /// 开始异步输出日志。
        /// </summary>
        private async void StartLogging()
        {
            while (true)
            {
                var context = await _queue.DequeueAsync().ConfigureAwait(false);
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    await OnInitializedAsync().ConfigureAwait(false);
                }
                OnLogReceived(context);
            }
        }

        /// <summary>
        /// 派生类重写此方法时，可以在收到第一条日志的时候执行一些初始化操作。
        /// </summary>
        protected abstract Task OnInitializedAsync();

        /// <summary>
        /// 派生类重写此方法时，将日志输出。
        /// </summary>
        /// <param name="context">包含一条日志的所有上下文信息。</param>
        protected abstract void OnLogReceived(in Context context);
    }
}
