using Walterlv.Logging.Core;

namespace Walterlv.Logging.Standard
{
    /// <summary>
    /// 提供向控制台输出日志的方法。
    /// </summary>
    public sealed class ConsoleLogger : OutputLogger
    {
        private readonly ConsoleLogWriter _writer = new ConsoleLogWriter();

        /// <inheritdoc />
        protected override void OnInitialized()
        {
        }

        /// <inheritdoc />
        protected override void OnLogReceived(in LogContext context) => _writer.Write(context);
    }
}
