using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpQOI
{
    public struct Color
    {
        public TrueByte Red;
        public TrueByte Green;
        public TrueByte Blue;
        public TrueByte Alpha;

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 255;
        }

        public override int GetHashCode() => (3 * Red + 5 * Green + 7 * Blue + 11 * Alpha).Value;

        public override string ToString() => $"Color({Red:X2},{Green:X2},{Blue:X2},{Alpha:X2})";
    }
}
