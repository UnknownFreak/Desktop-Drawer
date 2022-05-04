using DD.Colors;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DD.ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorDialSelector.xaml
    /// </summary>
    public partial class ColorDialSelector : UserControl, INotifyPropertyChanged
    {

        private void TriggerEvent()
        {
            OnColorChanged?.Invoke(this, new BrushColorChangedEventArgs()
            {
                Brush = SelectedColor as SolidColorBrush,
                Color = (SelectedColor as SolidColorBrush).Color,
                PenSize = PenStrokeSize.Value,
                IsBoxBrush = false,
                IsMarkerBrush = false
            });
        }
        private double sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        bool isInsideTriangle(Point cursor, Point p1, Point p2, Point p3)
        {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(cursor, p1, p2);
            d2 = sign(cursor, p2, p3);
            d3 = sign(cursor, p3, p1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        bool rotatingDial;
        bool hsvSelector;
        public enum Quadrants : int { nw=2, ne=1, sw=4, se=3}
        public ColorDialSelector()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private double GetAngle(Point translation, Size circleSize)
        {
            var _X = translation.X - circleSize.Width / 2d;
            var _Y = translation.Y - (circleSize.Height / 2d);

            var theta = Math.Atan2(_Y, _X);
            return ((90 + (theta * 180 / Math.PI)) + 360) % 360;
        }
        double m_Angle = default;
        public double Angle { get { return m_Angle; } set {
                m_Angle = value;
                OnPropertyChanged();
                OnPropertyChanged("Angle120");
                OnPropertyChanged("Angle240");
                hsv.H = Angle;
                AngleColor = ColorExtensions.FromAHSV(255, new HSV(Angle, 1, 1));
                SelectedColor = new SolidColorBrush(ColorExtensions.FromAHSV(255, hsv));
                Triangle.Points.Clear();
                Triangle.Points.Add(getPoint(A));
                Triangle.Points.Add(getPoint(B));
                Triangle.Points.Add(getPoint(C));
                OnPropertyChanged("SelectedColor");
                OnPropertyChanged("AngleColor");
                TriggerEvent();

            }
        }
        public double Angle120 { get { return m_Angle + 120; } }
        public double Angle240 { get { return m_Angle + 240; } }

        public double CenterX { get { return ColorCircleGrid.ActualWidth/2; } }
        public double CenterY { get { return ColorCircleGrid.ActualHeight/2; } }

        public float Saturation { get; set; } = 1;
        public float Value { get; set; } = 0;

        public Brush SelectedColor { get; set; } = Brushes.Black;
        public Color AngleColor { get; set; } = System.Windows.Media.Colors.Red;
        public event EventHandler<BrushColorChangedEventArgs> OnColorChanged;

        HSV hsv;

        public Point getPoint(Shape s)
        {
            return s.TranslatePoint(new Point(0,0), TriangleGrid);
        }

        
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Ellipse_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && rotatingDial)
                Angle = GetAngle(e.GetPosition(Dial), RenderSize);
            if (rotatingDial && e.LeftButton == MouseButtonState.Released)
                rotatingDial = false;

        }

        private void Dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rotatingDial = true;
            Angle = GetAngle(e.GetPosition(Dial), RenderSize);
        }

        private void Dial_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rotatingDial = false;
        }

        private Point? ColorPickerMarkerPosition;

        private void Triangle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!hsvSelector) return;

            var mousePoint = Mouse.GetPosition(InnerGrid);

            if (!isInsideTriangle(mousePoint, getPoint(A), getPoint(B), getPoint(C)))
                return;

            var offsetX = ColorPickerMarkerPosition.Value.X - mousePoint.X;
            var offsetY = ColorPickerMarkerPosition.Value.Y - mousePoint.Y;

            ColorPickerMarker.RenderTransform = new TranslateTransform(-offsetX- 7, -offsetY - 7 );
            ColorPickerMarkerVisual.RenderTransform = new TranslateTransform(-offsetX, -offsetY);
            MapHSV(getPoint(ColorPickerMarker));
        }

        private void Triangle_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left)
            {
                hsvSelector = false;
            }
        }

        private void Triangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                if (ColorPickerMarkerPosition == null)
                    ColorPickerMarkerPosition = ColorPickerMarker.TransformToAncestor(TriangleGrid).Transform(new Point(0, 0));
                var mousePosition = Mouse.GetPosition(InnerGrid);
                var deltaX = mousePosition.X - ColorPickerMarkerPosition.Value.X;
                var deltaY = mousePosition.Y - ColorPickerMarkerPosition.Value.Y;
                ColorPickerMarker.RenderTransform = new TranslateTransform(deltaX - 7, deltaY - 7);
                ColorPickerMarkerVisual.RenderTransform = new TranslateTransform(deltaX, deltaY);
                ColorPickerMarkerVisual.Visibility = Visibility.Visible;
                hsvSelector = true;
            }

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Angle = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Angle = 0;
        }

        private void MapHSV(Point P)
        {
            try
            {
                RenderTargetBitmap Render = new RenderTargetBitmap((int)InnerGrid.ActualWidth, (int)InnerGrid.ActualHeight, 96, 96, PixelFormats.Default);
                Render.Render(InnerGrid);
                CroppedBitmap Cropped = new CroppedBitmap(Render, new Int32Rect((int)P.X+7, (int)P.Y+7, 1, 1));

                byte[] Pixels = new byte[4];
                Cropped.CopyPixels(Pixels, 4, 0);
                var hsv2 =  Color.FromArgb(Pixels[3], Pixels[2], Pixels[1], Pixels[0]).RGBToHSV();
                hsv.S = hsv2.S;
                hsv.V = hsv2.V;
                SelectedColor = new SolidColorBrush(ColorExtensions.FromAHSV(255, hsv));
                OnPropertyChanged("SelectedColor");
                TriggerEvent();

            }
            catch { }
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void PenStrokeSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TriggerEvent();
        }
    }
}
