using System;
using System.Windows.Media;

namespace DD.ColorPicker
{
    public class BrushColorChangedEventArgs : EventArgs
    {
        public SolidColorBrush Brush { get; set; }
        public Color Color { get; set; }
        public double PenSize { get; set; }
        public bool IsBoxBrush { get; set; }
        public bool IsMarkerBrush { get; set; }
    }
}
