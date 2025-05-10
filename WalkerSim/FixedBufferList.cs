using System.Collections;
using System.Collections.Generic;

namespace WalkerSim
{
    internal sealed class FixedBufferList<T> : IEnumerable<T>
    {
        private readonly T[] _data;
        private int _count;

        public FixedBufferList(int maxSize)
        {
            _data = new T[maxSize];
        }

        public bool Full => _count >= _data.Length;
        public bool Empty => _count == 0;
        public int Count => _count;

        public void Add(T item)
        {
            _data[_count++] = item;
        }

        public void Clear() => _count = 0;

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly FixedBufferList<T> _list;
            private int _index;

            internal Enumerator(FixedBufferList<T> list)
            {
                _list = list;
                _index = -1;
            }

            public T Current => _list._data[_index];
            object IEnumerator.Current => Current;

            public bool MoveNext() => ++_index < _list._count;
            public void Reset() => _index = -1;
            public void Dispose() { }
        }
    }
}
