using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extra.Collections
{
    class ArrayMap<T>
    {
        private readonly T[,] array;
        private int count;

        public ArrayMap(long width, long height)
        {
            this.array = new T[width, height];
        }

        public T this[long x, long y]
        {
            get { return array[x, y]; }

            set
            {
                count += (IsSet(value) ? 1 : 0) - (IsSet(array[x, y]) ? 1 : 0);
                array[x, y] = value;
            }
        }

        public int Count
        {
            get { return count; }
        }

        public int GetLength(int dimension)
        {
            return this.array.GetLength(dimension);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.array.Cast<T>().Where(IsSet).GetEnumerator();
        }
        
        private static bool IsSet(T value)
        {
            return !EqualityComparer<T>.Default.Equals(value, default(T));
        }

    }
}

