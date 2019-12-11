using System;

namespace Walterlv.Collections.Concurrent
{
    public static class ReadonlyObservableBagExtensions
    {
        public static ReadonlyObservableBag<T> Select<T>(this ObservableConcurrentBag<T> collection, Func<T, bool> predicate)
        {
            return new ReadonlyObservableBag<T>(collection, predicate);
        }
    }
}
