//可视化股票技术分析软件

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StockTech.Controls
{
    /// <summary>
    /// cross line for 
    /// </summary>
    class CrossLine  : UserControl
    {
        //
        public CrossLine()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                drawCrossLine(dc, x, y);
            }
        }
        double x = 0;
        double y = 0;
        //要绘制各部分的比例
        static double klinePart = 4;
        static double volumePart = 3;
        static double timeLinePart = 1;
        static double toalPart = klinePart + volumePart + timeLinePart;

        static double dateHeight = 16;//this part

        protected override void OnMouseMove(MouseEventArgs e)
        {
           
            var pos = e.GetPosition(this);
            x = pos.X;
            y = pos.Y;
           

            this.InvalidateVisual();
            
        }

        void drawCrossLine(DrawingContext dc, double x, double y)
        {
           
            var xoffset = (int)(x) + 0.5;
            var yoffset = (int)(y) + 0.5;

            Pen pen = new Pen();
            Color penColor = Color.FromRgb(0,0,0);

            pen.Brush = new SolidColorBrush(penColor);
            pen.Thickness = 1;

            var verticalHeight = (klinePart + volumePart) / (toalPart) * (this.ActualHeight - dateHeight) + dateHeight;
            if (yoffset < verticalHeight)
            {

                //vertical
                dc.DrawLine(pen, new Point(xoffset, 0), new Point(xoffset, verticalHeight));
                //horizontal
                dc.DrawLine(pen, new Point(0, yoffset), new Point(this.ActualWidth, yoffset));

            }

        }


       

    }
}
