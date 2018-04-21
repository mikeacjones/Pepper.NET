using System;

namespace PepperNET
{
    public sealed class SetOnce<T>
    {
        private T value;
        private bool hasValue;
        public override string ToString()
        {
            return hasValue ? Convert.ToString(value) : "";
        }
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                if (hasValue) throw new InvalidOperationException("Value already set");
                this.value = value;
                this.hasValue = true;
            }
        }
        internal SetOnce() { }
        internal SetOnce(T val)
        {
            this.Value = val;
        }
        public T ValueOrDefault { get { return value; } }
        public static implicit operator T(SetOnce<T> value) { return value.Value; }
    }
}
