using System.Text;

namespace System
{
    public struct Trio<A, B, C>
    {
        private A first;
        private B second;
        private C third;
        public A First => first;
        public B Second => second;
        public C Third => third;

        public Trio(A first, B second, C third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
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

            stringBuilder.Append(", ");
            if (Third != null)
                stringBuilder.Append(Third.ToString());

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }
}
