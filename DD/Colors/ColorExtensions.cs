using System;
using System.Windows.Media;

namespace DD.Colors
{
    public static class ColorExtensions
    {

        public static HSV RGBToHSV(this Color rgb)
        {
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            v = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;

            if (s == 0)
                h = 0.0;

            else
            {
                if (rgb.R == v)
                    h = (rgb.G - rgb.B) / delta;
                else if (rgb.G == v)
                    h = 2 + (rgb.B - rgb.R) / delta;
                else if (rgb.B == v)
                    h = 4 + (rgb.R - rgb.G) / delta;

                h *= 60;

                if (h < 0.0)
                    h = h + 360;
            }

            return new HSV(h, s, v / 255);
        }

        public static Color FromAHSV(byte alpha, HSV hsv)
        {
            if (hsv.H < 0f || hsv.H > 360f)
                throw new ArgumentOutOfRangeException(nameof(hsv.H), hsv.H, "Hue must be in the range [0,360]");
            if (hsv.S < 0f || hsv.S > 1f)
                throw new ArgumentOutOfRangeException(nameof(hsv.S), hsv.S, "Saturation must be in the range [0,1]");
            if (hsv.V < 0f || hsv.V > 1f)
                throw new ArgumentOutOfRangeException(nameof(hsv.V), hsv.V, "Value must be in the range [0,1]");

            byte Component(int n)
            {
                var k = (n + hsv.H / 60f) % 6;
                var c = hsv.V - hsv.V * hsv.S * Math.Max(Math.Min(Math.Min(k, 4 - k), 1), 0);
                var b = (int)Math.Round(c * 255);
                return (byte)(b < 0 ? 0 : b > 255 ? 255 : b);
            }

            return Color.FromArgb(alpha, Component(5), Component(3), Component(1));
        }
    }
}
