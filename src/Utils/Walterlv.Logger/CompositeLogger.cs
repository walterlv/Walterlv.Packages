using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Walterlv.Logging
{
    /// <summary>
    /// 组合日志。提供各种不同输出的日志合集共同输出。
    /// </summary>
    public class CompositeLogger : ILogger, IEnumerable<ILogger>
    {
        private readonly object _locker = new object();
        private readonly Dictionary<ILogger, ILogger> _loggers;

        /// <summary>
        /// 创建组合日志的新实例。
        /// </summary>
        public CompositeLogger(params ILogger[] initialLoggers)
        {
            if (initialLoggers is null)
            {
                throw new ArgumentNullException(nameof(initialLoggers));
            }

            _loggers = new Dictionary<ILogger, ILogger>(initialLoggers.ToDictionary(x => x, x => x));
        }

        /// <summary>
        /// 向组合日志中添加日志实例。如果添加的日志已存在，会忽略而不会重复添加也不会出现异常。
        /// </summary>
        /// <param name="logger">要添加的日志实例。</param>
        public void Add(ILogger logger)
        {
            lock (_locker)
            {
                _loggers[logger] = logger ?? throw new ArgumentNullException(nameof(logger));
            }
        }

        /// <summary>
        /// 从组合日志中移除日志实例。
        /// </summary>
        /// <param name="logger">要移除的日志实例。</param>
        /// <returns>如果要移除的日志不存在，则返回 false；否则返回 true。</returns>
        public bool Remove(ILogger logger)
        {
            lock (_locker)
            {
                return _loggers.Remove(logger ?? throw new ArgumentNullException(nameof(logger)));
            }
        }

        /// <inheritdoc />
        IEnumerator<ILogger> IEnumerable<ILogger>.GetEnumerator()
        {
            lock (_locker)
            {
                return _loggers.Select(x => x.Value).ToList().GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ILogger>)this).GetEnumerator();

        /// <inheritdoc />
        public void Error(string message, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Error(message, callerMemberName));

        /// <inheritdoc />
        public void Error(Exception exception, string? message = null, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Error(exception, message, callerMemberName));

        /// <inheritdoc />
        public void Fatal(Exception exception, string message, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Fatal(exception, message, callerMemberName));

        /// <inheritdoc />
        public void Message(string text, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Message(text, callerMemberName));

        /// <inheritdoc />
        public void Trace(string text, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Trace(text, callerMemberName));

        /// <inheritdoc />
        public void Warning(string message, [CallerMemberName] string? callerMemberName = null)
            => Log(x => x.Warning(message, callerMemberName));

        /// <summary>
        /// 转发日志到所有的子日志系统。
        /// </summary>
        /// <param name="logAction"></param>
        private void Log(Action<ILogger> logAction)
        {
            lock (_locker)
            {
                foreach (var logger in _loggers)
                {
                    logAction(logger.Key);
                }
            }
        }
    }
}
