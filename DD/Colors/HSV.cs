namespace DD.Colors
{
    public struct HSV
    {
        public HSV(double h, double s, double v)
        {

            H = h;
            S = s;
            V = v;
        }

        public double H { get; set; }

        public double S { get; set; }

        public double V { get; set; }

        public bool Equals(HSV hsv)
        {
            return (H == hsv.H) && (S == hsv.S) && (V == hsv.V);
        }
    }
}
