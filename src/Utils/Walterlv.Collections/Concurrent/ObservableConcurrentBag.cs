using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Walterlv.Collections.Concurrent
{
    public class ObservableConcurrentBag<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ConcurrentBag<T> _collection = new ConcurrentBag<T>();
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Add(T item)
        {
            _collection.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { item }));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public int AddRange(IEnumerable<T> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                _collection.Add(item);
                count++;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            return count;
        }

        public int Count => throw new NotImplementedException();
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => throw new NotSupportedException("此集合本身是线程安全的，且对外访问为只读，因此不支持进行同步。");
        void ICollection.CopyTo(Array array, int index) => ((ICollection)_collection).CopyTo(array, index);
        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
