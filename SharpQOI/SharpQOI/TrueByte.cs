using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpQOI
{
    public struct TrueByte
    {
        public byte Value;

        public TrueByte(byte value) => Value = value;

        public TrueByte(int value) => Value = unchecked((byte)value);

        public static implicit operator TrueByte(byte value) => new(value);

        public static TrueByte operator +(TrueByte a) => a;
        public static TrueByte operator -(TrueByte a) => new(-a.Value);
        public static TrueByte operator ~(TrueByte a) => new(~a.Value);
        public static TrueByte operator +(TrueByte a, TrueByte b) => new(a.Value + b.Value);
        public static TrueByte operator -(TrueByte a, TrueByte b) => new(a.Value - b.Value);
        public static TrueByte operator *(TrueByte a, TrueByte b) => new(a.Value * b.Value);
        public static TrueByte operator /(TrueByte a, TrueByte b) => new(a.Value / b.Value);
        public static TrueByte operator %(TrueByte a, TrueByte b) => new(a.Value % b.Value);
        public static TrueByte operator &(TrueByte a, TrueByte b) => new(a.Value & b.Value);
        public static TrueByte operator |(TrueByte a, TrueByte b) => new(a.Value | b.Value);
        public static TrueByte operator ^(TrueByte a, TrueByte b) => new(a.Value ^ b.Value);
        public static TrueByte operator <<(TrueByte a, int b) => new(a.Value << b);
        public static TrueByte operator >>(TrueByte a, int b) => new(a.Value >> b);
        public static bool operator <(TrueByte a, TrueByte b) => a.Value < b.Value;
        public static bool operator >(TrueByte a, TrueByte b) => a.Value > b.Value;
        public static bool operator <=(TrueByte a, TrueByte b) => a.Value <= b.Value;
        public static bool operator >=(TrueByte a, TrueByte b) => a.Value >= b.Value;
    }
}
