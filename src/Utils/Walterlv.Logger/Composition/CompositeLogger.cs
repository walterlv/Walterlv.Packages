using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.Composition
{
    public class CompositeLogger : AsyncOutputLogger, ICollection<ILogger>
    {
        private readonly object _locker = new object();
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public int Count => _loggers.Count;

        public bool Contains(ILogger logger) => _loggers.Contains(logger);

        public void Add(ILogger logger)
        {
            lock (_locker)
            {
                _loggers.Add(logger);
            }
        }

        public bool Remove(ILogger logger)
        {
            lock (_locker)
            {
                return _loggers.Remove(logger);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _loggers.Clear();
            }
        }

        bool ICollection<ILogger>.IsReadOnly => false;

        public IEnumerator<ILogger> GetEnumerator() => _loggers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _loggers.GetEnumerator();

        void ICollection<ILogger>.CopyTo(ILogger[] array, int arrayIndex) => _loggers.CopyTo(array, arrayIndex);

        protected override Task OnInitializedAsync() => Task.FromResult<object?>(null);

        protected override void OnLogReceived(in Context context)
        {
            foreach (var logger in _loggers)
            {
                if (logger is AsyncOutputLogger asyncLogger)
                {
                    asyncLogger.LogCore(in context);
                }
                else
                {
                    LogForGenericLogger(in context);
                }
            }
        }

        private void LogForGenericLogger(in Context context)
        {

        }
    }
}
