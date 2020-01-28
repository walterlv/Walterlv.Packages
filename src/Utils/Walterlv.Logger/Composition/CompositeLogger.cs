using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Walterlv.Logging.Core;

namespace Walterlv.Logging.Composition
{
    public class CompositeLogger : AsyncOutputLogger, ICollection<AsyncOutputLogger>, IEnumerable<ILogger>
    {
        private readonly object _locker = new object();
        private readonly List<AsyncOutputLogger> _loggers = new List<AsyncOutputLogger>();
        private LogLevel _level = LogLevel.Message;

        public CompositeLogger()
        {
        }

        public CompositeLogger(params AsyncOutputLogger[] loggers)
        {
            if (loggers is null)
            {
                throw new ArgumentNullException(nameof(loggers));
            }

            _loggers = loggers.ToList();
        }

        public override LogLevel Level
        {
            get => _level;
            set
            {
                _level = value;
                foreach (var logger in _loggers)
                {
                    logger.Level = value;
                }
            }
        }

        protected override Task OnInitializedAsync() => Task.FromResult<object?>(null);

        protected override void OnLogReceived(in Context context)
        {
            foreach (var logger in _loggers)
            {
                logger.LogCore(in context);
            }
        }

        public int Count => _loggers.Count;

        public bool Contains(AsyncOutputLogger logger) => _loggers.Contains(logger);

        public void Add(AsyncOutputLogger logger)
        {
            lock (_locker)
            {
                _loggers.Add(logger);
            }
        }

        public bool Remove(AsyncOutputLogger logger)
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

        bool ICollection<AsyncOutputLogger>.IsReadOnly => false;

        public IEnumerator<AsyncOutputLogger> GetEnumerator() => _loggers.GetEnumerator();

        IEnumerator<ILogger> IEnumerable<ILogger>.GetEnumerator() => _loggers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _loggers.GetEnumerator();

        void ICollection<AsyncOutputLogger>.CopyTo(AsyncOutputLogger[] array, int arrayIndex) => _loggers.CopyTo(array, arrayIndex);
    }
}
