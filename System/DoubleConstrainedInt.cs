using System;
using SALT.Extensions;

namespace System
{
    /// <summary>
    /// An object used to create an <see cref="int"/> value which can never be less than the <see cref="MinValue"/> or more than the <see cref="MaxValue"/>.
    /// </summary>
    public struct DoubleConstrainedInt
    {
        private int value;
        private int min;
        private int max;

        private static DoubleConstrainedInt none = new DoubleConstrainedInt(0);

        /// <summary>
        /// Equal to 0
        /// </summary>
        public static DoubleConstrainedInt zero => none;

        /// <summary>
        /// The <see cref="Value"/> that is constrained.
        /// </summary>
        internal int ConstrainedValue
        {
            get => value;
        }

        /// <summary>
        /// Clamps an <see cref="int"/> between <see cref="MinValue"/> and <see cref="MaxValue"/>
        /// </summary>
        /// <param name="from">The <see cref="int"/> to clamp</param>
        /// <returns>The clamped <see cref="int"/></returns>
        public int GetValue(int from) => from.Clamp(min, max);

        /// <summary>
        /// Clamps an <see cref="int"/> between <see cref="MinValue"/> and <see cref="MaxValue"/>
        /// </summary>
        /// <param name="from">The <see cref="int"/> to clamp</param>
        /// <returns>The clamped <see cref="int"/></returns>
        public object GetValue(object from) => ((int)from).Clamp(min, max);

        /// <summary>
        /// Used to hold an <see cref="int"/> value between <see cref="MinValue"/> and <see cref="MaxValue"/>
        /// </summary>
        public int Value
        {
            get => value.Clamp(min, max);
            set => this.value = value;
        }

        /// <summary>
        /// The lowest number that the <see cref="Value"/> property can be
        /// </summary>
        public int MinValue
        {
            get => min;
            set => this.min = value;
        }

        /// <summary>
        /// The highest number that the <see cref="Value"/> property can be
        /// </summary>
        public int MaxValue
        {
            get => max;
            set => this.max = value;
        }

        /// <summary>
        /// Creates a <see cref="DoubleConstrainedInt"/>
        /// </summary>
        /// <param name="value">The <see cref="int"/> to be constrained</param>
        /// <param name="min">The lowest number that the <see cref="Value"/> property can be</param>
        /// <param name="max">The highest number that the <see cref="Value"/> property can be</param>
        public DoubleConstrainedInt(int value, int min = int.MinValue, int max = int.MaxValue)
        {
            this.value = value;
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Indicates whether both <see cref="DoubleConstrainedInt"/>s are equal.
        /// </summary>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="dci2"/>.</param>
        /// <param name="dci2">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="dci"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="dci"/> and <paramref name="dci2"/> represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(DoubleConstrainedInt dci, DoubleConstrainedInt dci2) => dci.Value == dci2.Value && dci.MaxValue == dci2.MaxValue && dci.MinValue == dci2.MinValue;

        /// <summary>
        /// Indicates whether both <see cref="DoubleConstrainedInt"/>s are not equal.
        /// </summary>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="dci2"/>.</param>
        /// <param name="dci2">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="dci"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="dci"/> and <paramref name="dci2"/> do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(DoubleConstrainedInt dci, DoubleConstrainedInt dci2) => !(dci == dci2);

        /// <summary>
        /// Converts a <see cref="DoubleConstrainedInt"/> to an <see cref="int"/>
        /// </summary>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to convert</param>
        public static implicit operator int(DoubleConstrainedInt dci) => dci.Value;

        /// <summary>
        /// Converts an <see cref="int"/> to a <see cref="DoubleConstrainedInt"/>
        /// </summary>
        /// <param name="i">The <see cref="int"/> to convert</param>
        public static implicit operator DoubleConstrainedInt(int i) => new DoubleConstrainedInt(i);

        /// <summary>
        /// Indicates whether a specified <see cref="object"/> and a specified <see cref="DoubleConstrainedInt"/> are not equal.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with <paramref name="dci"/>.</param>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="obj"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> and <paramref name="dci"/> are not the same type and do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(DoubleConstrainedInt dci, object obj) => !dci.Equals(obj);

        /// <summary>
        /// Indicates whether a specified <see cref="object"/> and a specified <see cref="DoubleConstrainedInt"/> are equal.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with <paramref name="dci"/>.</param>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="obj"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> and <paramref name="dci"/> are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(DoubleConstrainedInt dci, object obj) => dci.Equals(obj);

        /// <summary>
        /// Indicates whether a specified <see cref="object"/> and a specified <see cref="DoubleConstrainedInt"/> are not equal.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with <paramref name="dci"/>.</param>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="obj"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> and <paramref name="dci"/> are not the same type and do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(object obj, DoubleConstrainedInt dci) => !dci.Equals(obj);

        /// <summary>
        /// Indicates whether a specified <see cref="object"/> and a specified <see cref="DoubleConstrainedInt"/> are equal.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with <paramref name="dci"/>.</param>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with <paramref name="obj"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> and <paramref name="dci"/> are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(object obj, DoubleConstrainedInt dci) => dci.Equals(obj);

        /// <summary>
        /// Indicates whether this instance and a specified <see cref="DoubleConstrainedInt"/> are equal.
        /// </summary>
        /// <param name="dci">The <see cref="DoubleConstrainedInt"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="dci"/> and this instance represent the same value; otherwise, <see langword="false"/>.</returns>
        public bool Equals(DoubleConstrainedInt dci) => this == dci;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash for this instance.</returns>
        public override int GetHashCode() => this.Value.GetHashCode() + this.MinValue.GetHashCode() + this.MaxValue.GetHashCode();

        /// <summary>
        /// Returns the fully qualified type name of the instance.
        /// </summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() => string.Format("{0} ({1},{2})", this.Value, this.MinValue, this.MaxValue);
    }
}
