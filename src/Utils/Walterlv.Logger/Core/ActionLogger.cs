using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walterlv.Logging.Core
{
    /// <summary>
    /// 提供函数式的同步日志记录方法。
    /// </summary>
    public sealed class ActionLogger : OutputLogger
    {
        private readonly Action<LogContext> _onLogReceived;
        private readonly Action? _onInitialized;

        /// <summary>
        /// 创建函数式的同步日志记录方法。
        /// </summary>
        /// <param name="onLogReceived">当有新的日志需要记录时，在此函数中记录日志。</param>
        /// <param name="onInitialized">在第一条日志准备开始记录之前，如果需要初始化，则在此传入初始化函数。</param>
        public ActionLogger(Action<LogContext> onLogReceived, Action? onInitialized = null)
        {
            _onLogReceived = onLogReceived ?? throw new ArgumentNullException(nameof(onLogReceived));
            _onInitialized = onInitialized;
        }

        /// <inheritdoc />
        protected override void OnInitialized() => _onInitialized?.Invoke();

        /// <inheritdoc />
        protected override void OnLogReceived(in LogContext context) => _onLogReceived(context);
    }
}
