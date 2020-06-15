using System;
using System.Threading.Tasks;

namespace Walterlv.Logging.Core
{
    /// <summary>
    /// 提供函数式的异步日志记录方法。
    /// </summary>
    public sealed class TaskFuncLogger : AsyncOutputLogger
    {
        private readonly Action<LogContext> _onLogReceived;
        private readonly Func<Task>? _onInitializedAsync;

        /// <summary>
        /// 创建函数式的同步日志记录方法。
        /// </summary>
        /// <param name="onLogReceived">当有新的日志需要记录时，在此函数中记录日志。</param>
        /// <param name="onInitializedAsync">在第一条日志准备开始记录之前，如果需要初始化，则在此传入初始化函数。</param>
        public TaskFuncLogger(Action<LogContext> onLogReceived, Func<Task>? onInitializedAsync = null)
        {
            _onLogReceived = onLogReceived ?? throw new ArgumentNullException(nameof(onLogReceived));
            _onInitializedAsync = onInitializedAsync;
        }

        /// <inheritdoc />
        protected override Task OnInitializedAsync()
        {
            if (_onInitializedAsync is null)
            {
                return Task.FromResult<object?>(null);
            }

            var task = _onInitializedAsync?.Invoke();
            if (task is null)
            {
                throw new InvalidOperationException("不应该在初始化函数中返回 null。如果不需要返回任何值，请返回 Task.CompletedTask。");
            }

            return task;
        }

        /// <inheritdoc />
        protected override void OnLogReceived(in LogContext context) => _onLogReceived(context);
    }
}
