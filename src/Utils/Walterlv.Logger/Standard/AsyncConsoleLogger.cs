using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.Standard
{
    /// <summary>
    /// 提供向控制台输出日志的方法。
    /// </summary>
    public sealed class AsyncConsoleLogger : AsyncOutputLogger
    {
        private readonly ConsoleLogWriter _writer = new ConsoleLogWriter();

        /// <inheritdoc />
        protected override Task OnInitializedAsync() => Task.FromResult<object?>(null);

        /// <inheritdoc />
        protected override void OnLogReceived(in LogContext context) => _writer.Write(context);
    }
}
