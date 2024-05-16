using System.Collections.Generic;
using System.Text;

namespace System
{
    public struct Pair<A, B>
    {
        private A first;
        private B second;
        public A First => first;
        public B Second => second;

        public Pair(A first, B second)
        {
            this.first = first;
            this.second = second;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            if (First != null)
                stringBuilder.Append(First.ToString());

            stringBuilder.Append(", ");
            if (Second != null)
                stringBuilder.Append(Second.ToString());

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        public static implicit operator KeyValuePair<A, B>(Pair<A, B> pair) => new KeyValuePair<A, B>(pair.First, pair.Second);
        public static implicit operator Pair<A, B>(KeyValuePair<A, B> pair) => new Pair<A, B>(pair.Key, pair.Value);
    }
}
