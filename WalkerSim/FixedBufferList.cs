using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WalkerSim
{
    internal class FixedBufferList<T> : IEnumerable<T>
    {
        T[] _data;
        int _count = 0;
        int _capacity = 0;

        public FixedBufferList(int maxSize)
        {
            _data = new T[maxSize];
            _capacity = maxSize;
        }

        public bool Full
        {
            get => _count >= _capacity;
        }

        public bool Empty
        {
            get => _count == 0;
        }

        public int Count
        {
            get => _count;
        }

        public void Add(T item)
        {
            _data[_count++] = item;
        }

        public void Clear()
        {
            _count = 0;
        }

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.Take(_count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
