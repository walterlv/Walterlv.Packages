using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Walterlv.Logging.Core
{
    partial class AsyncOutputLogger
    {
        private class AsyncQueue<T>
        {
            private readonly SemaphoreSlim _semaphoreSlim;
            private readonly ConcurrentQueue<T> _queue;

            public AsyncQueue()
            {
                _semaphoreSlim = new SemaphoreSlim(0);
                _queue = new ConcurrentQueue<T>();
            }

            public int Count => _queue.Count;

            public void Enqueue(T item)
            {
                _queue.Enqueue(item);
                _semaphoreSlim.Release();
            }

            public void EnqueueRange(IEnumerable<T> source)
            {
                var n = 0;
                foreach (var item in source)
                {
                    _queue.Enqueue(item);
                    n++;
                }
                _semaphoreSlim.Release(n);
            }

            public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
            {
                while (true)
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (_queue.TryDequeue(out var item))
                    {
                        return item;
                    }
                }
            }
        }
    }
}
