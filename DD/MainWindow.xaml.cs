using DD.ColorPicker;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DrawingAttributes inkDA;
        List<StrokeCollection> _added;

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr onj);

        public MainWindow()
        {
            InitializeComponent();

            // Set up the DrawingAttributes for the pen.
            inkDA = new DrawingAttributes();
            inkDA.Height = 5;
            inkDA.Width = 5;


            ik.DefaultDrawingAttributes = inkDA;
            ik.Strokes.StrokesChanged += Strokes_StrokesChanged;
            _added = new List<StrokeCollection>();
            ColorPicker.OnColorChanged += OnColorChangedEvent;
        }

        private void OnColorChangedEvent(object sender, BrushColorChangedEventArgs e)
        {
            inkDA.Color = e.Color;
            inkDA.Height = e.PenSize;
            inkDA.Width = e.PenSize;
            inkDA.IsHighlighter = e.IsMarkerBrush;
            if (e.IsBoxBrush)
                inkDA.StylusTip = StylusTip.Rectangle;
            else
                inkDA.StylusTip = StylusTip.Ellipse;
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Added.Count != 0)
                _added.Add(e.Added);
        }

        private void DWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                this.Close();
        }

        private void DWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                CurrentScreenToClipboard();
                this.Close();
            }
            if (e.Key == System.Windows.Input.Key.Z)
            {
                try
                {
                    var s = _added.TakeLast(1).ToList()[0];
                    ik.Strokes.Remove(s);
                    _added.Remove(s);
                }
                catch 
                {

                }
            }
        }

        private void ik_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                if (ColorPicker.Visibility == Visibility.Hidden)
                {
                    ColorPicker.Visibility = Visibility.Visible;
                }
                else
                    ColorPicker.Visibility = Visibility.Hidden;
                var pos = e.GetPosition(this);
                var Margin = ColorPicker.Margin;
                Margin.Left = pos.X - ColorPicker.Width/2d;
                Margin.Top = pos.Y - ColorPicker.Height/2d;
                if (Margin.Left < 0)
                    Margin.Left = 0;
                if (Margin.Top < 0)
                    Margin.Top = 0;
                ColorPicker.Margin = Margin;
            }
        }

        void CurrentScreenToClipboard()
        {
            Bitmap bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                var source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                Clipboard.SetImage(source);
            }
            catch (Exception)
            {

            }

            finally
            {
                DeleteObject(handle);
            }
        }

        private void Close_button_Click(object sender, RoutedEventArgs e)
        {
            Close_button.Visibility = Visibility.Hidden;
            CurrentScreenToClipboard();
            this.Close();
        }
    }
}
