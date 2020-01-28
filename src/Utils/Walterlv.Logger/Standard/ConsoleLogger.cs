using System;
using System.Globalization;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.Standard
{
    /// <summary>
    /// 提供向控制台输出日志的方法。
    /// </summary>
    public sealed class ConsoleLogger : AsyncOutputLogger
    {
        private DateTimeOffset _lastTime;

        protected override Task OnInitializedAsync() => Task.FromResult<object?>(null);

        protected override void OnLogReceived(in Context context)
        {
            // 输出新的一天。
            var currentTime = DateTimeOffset.Now;
            var isNewDay = _lastTime.Date != currentTime.Date;
            _lastTime = currentTime;
            if (isNewDay)
            {
                Console.WriteLine($"[{currentTime.Date.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)}]".PadRight(Console.BufferWidth - 2, '─'));
            }

            // 输出当前时间。
            Console.ForegroundColor = ConsoleColor.DarkGray;
            var time = context.Time.ToLocalTime().ToString("[HH:mm:ss.fff]", CultureInfo.InvariantCulture);
            Console.Write(time);
            Console.Write(' ');

            // 输出日志信息。
            Console.ForegroundColor = context.CurrentLevel switch
            {
                LogLevel.Detail => ConsoleColor.DarkGray,
                LogLevel.Message => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };
            Console.WriteLine(context.Text);

            // 输出额外信息。
            if (context.ExtraInfo != null)
            {
                foreach (var line in context.ExtraInfo.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    Console.WriteLine($"               {line}");
                }
            }

            // 还原控制台。
            Console.ResetColor();
        }
    }
}
