//Copyright (c) 2010-2012, 王旭明 youkes.com
//All rights reserved.
//MIT licence.
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StockTech.Data;

namespace StockTech
{
    //绘制技术分析图表相关...
    class TechCanvas : FrameworkElement
    {
        
        public TechCanvas()
        {
            addEMADays(5);//
            addEMADays(10);//
            addEMADays(30);//
           
        }

        Point mousePos = new Point();
        //
        int bottomType = 1;//

  
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            mouseDown(e);
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            mouseMove(e);

        }


        string symbol=null;
        
        DayFile priceFile = null;
        public void load(string symbol="sh000001")
        {
            if (this.symbol == symbol)
            {
                return;
            }

            this.symbol=symbol;
            this.priceFile = DayFile.get(symbol);
            this.totalItemCount = (int)this.priceFile.ItemCount;

            chartOffset = getMaxOffset();
            maxChartOffset = chartOffset;

            this.InvalidateVisual();
        }

        double width=0;
        double height=0;
        Rect rect=new Rect();

        double klineLeft = 0;
        double klineTop = 0;
        double klineWidth = 0;
        double klineHeight = 0;

        double volumeLeft = 0;
        double volumeTop = 0;
        double volumeWidth = 0;
        double volumeHeight = 0;

        double timeLineLeft = 0;
        double timeLineTop = 0;
        double timeLineWidth = 0;
        double timeLineHeight = 0;

        double dateLeft = 0;
        double dateTop = 0;
        double dateWidth = 0;
        


        bool isInRect(double left, double top, double width, double height, double x, double y)
        {
            if (x > left && x < left + width)
            {
                if (y > top && y < top + height)
                {
                    return true;
                }
            }

            return false;
        }


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }

            render(dc);

           
        }


        private void render(DrawingContext dc)
        {

            if (this.symbol == null)
            {
                load();
            }

            initLayout();

            //draw background
            drawBackground(dc);

            //
            if (!initDrawRegion())
            {
                return;
            }


            
            //绘制顶部K线图
            drawGrid(dc, klineLeft, klineTop, klineWidth, klineHeight, 5);

            drawKLine(dc, klineLeft, klineTop, klineWidth, klineHeight);

            //绘制用户添加的EMA,在K线图上
            drawEMALines(dc, klineLeft, klineTop, klineWidth, klineHeight);

            //
            drawDateTxt(dc, dateLeft, dateTop, dateWidth, dateHeight, 5);

            //middle
            drawGrid(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight, 0);


            switch (bottomType)
            {
                case 0:
                    drawVolume(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight);
                    break;
                case 1:
                    drawMacd(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight);
                    break;
                case 2:
                    drawKDJ(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight);
                    break;
                default:
                    drawVolume(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight);
                    break;
            }


            //bottom
            drawGrid(dc, timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight, 0);
            drawTimeLine(dc, timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight);


            if (OnRegionChanged != null)
            {
                OnRegionChanged();
            }

        }

        private void drawKDJ(DrawingContext dc, double left, double top, double width, double height)
        {
            double highest = priceFile.getHighestJ(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            double lowest = priceFile.getLowestJ(drawItemStartIndex, drawItemStartIndex + drawItemCount);

            //draw K
           
            PathFigure pfK = new PathFigure();
            PathGeometry pgK = new PathGeometry();
            pgK.Figures.Add(pfK);

            PathFigure pfD = new PathFigure();
            PathGeometry pgD = new PathGeometry();
            pgD.Figures.Add(pfD);

            PathFigure pfJ = new PathFigure();
            PathGeometry pgJ = new PathGeometry();
            pgJ.Figures.Add(pfJ);

            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
              
                double x = i * (itemWidth + itemSpace) + itemWidth / 2.0 + 0.5 + left;
                double yK = height - (priceFile.K[itemIndex] - lowest) / (highest - lowest) * height + 0.5 + top;
                double yD = height - (priceFile.D[itemIndex] - lowest) / (highest - lowest) * height + 0.5 + top;
                double yJ = height - (priceFile.J[itemIndex] - lowest) / (highest - lowest) * height + 0.5 + top;

                if (i == 0)
                {
                    pfK.StartPoint = new Point(x, yK);
                    pfD.StartPoint = new Point(x, yD);
                    pfJ.StartPoint = new Point(x, yJ);
                }
                else
                {
                    pfK.Segments.Add(new LineSegment(new Point(x, yK), true));
                    pfD.Segments.Add(new LineSegment(new Point(x, yD), true));
                    pfJ.Segments.Add(new LineSegment(new Point(x, yJ), true));
                }
            }
            var pen = getPen(255, 0, 0, 1);
            dc.DrawGeometry(Brushes.Transparent, pen, pgK);
            pen = getPen(0, 128, 0, 1);
            dc.DrawGeometry(Brushes.Transparent, pen, pgD);
            pen = getPen(0, 0, 255, 1);
            dc.DrawGeometry(Brushes.Transparent, pen, pgJ);

        }


        

        int totalItemCount = 0;

        //绘制日期
        void drawDateTxt(DrawingContext dc,double left, double top, double width, double height,int lineCount) {
            if (priceFile == null || totalItemCount == 0)
            {
                return;
            }
    
            var startIndex = (int)(chartOffset / (itemWidth + itemSpace));
            var cnt = (int)(width / (itemWidth + itemSpace));
            var itemOffset = (int)(width / (itemWidth + itemSpace) / (lineCount + 1));

            for (var i = 0; i < lineCount + 1; i++) {
                if (i * itemOffset + startIndex >= totalItemCount)
                {
                    break;
                }
                var xoffset = (int)(i *width/(lineCount+1)) + left + 0.5;
                var date = priceFile.Dates[i*itemOffset + startIndex];
                var year = (int)(date / 10000);
                var month = (int)((date - year * 10000) / 100);
                var day = (int)((date - year * 10000 - month * 100));
                var str = year + "/" + month + "/" + day;
               FormattedText txt = new FormattedText(str,
                 System.Globalization.CultureInfo.CurrentCulture,
                 FlowDirection.LeftToRight, new Typeface("Verdana"),
                 12, new SolidColorBrush(Color.FromRgb(64, 64, 64)));
                dc.DrawText(txt, new Point(xoffset, top));

            }
            //
        }

        public void addEMADays(int days)
        {
            //already add.
            foreach (var item in emaDays)
            {
                if (item == days)
                {
                    this.InvalidateVisual();
                    return;
                }
            }

            emaDays.Add(days);
            this.InvalidateVisual();
        }

        public void flipEMADays(int days)
        {
            //-1 is remove all days.
            if (days == -1)
            {
                emaDays.Clear();
                this.InvalidateVisual();
                return;
            }

            if (days <= 0)
            {
                
                return;
            }

            //already add remove.
            for (int i = 0; i < emaDays.Count; i++)
            {
                if (emaDays[i] == days)
                {
                    emaDays.RemoveAt(i);
                    this.InvalidateVisual();
                    return;
                }
            }

            emaDays.Add(days);
            this.InvalidateVisual();
        }

        //要绘制各部分的比例
        static double klinePart = 4;
        static double volumePart = 3;
        static double timeLinePart = 1;
       static double toalPart = klinePart + volumePart + timeLinePart;

       static double dateHeight = 16;//this part
        
        //初始化布局，计算布局坐标
        void initLayout()
        {
            this.width = this.ActualWidth;
            this.height = this.ActualHeight;
            this.rect.Width = width;
            this.rect.Height = height;


            this.klineLeft = 0;
            this.klineTop = 0;
            this.klineWidth = this.width;
            this.klineHeight = (this.height - dateHeight) * klinePart / (toalPart);


            dateLeft = 0;
            dateTop = klineHeight;
            dateWidth = this.width;
            

            //
            volumeLeft = 0;
            volumeTop = dateTop + dateHeight;
            volumeWidth = this.width;
            volumeHeight = (this.height - dateHeight) * volumePart / (toalPart);

            timeLineLeft = 0;
            timeLineTop = volumeTop + volumeHeight;
            timeLineWidth = this.width;
            timeLineHeight = (this.height - dateHeight) * timeLinePart / (toalPart);


            maxChartOffset = getMaxOffset();
            if (chartOffset > maxChartOffset)
            {
                chartOffset = maxChartOffset;
            }
            if (chartOffset < 0)
            {
                chartOffset = 0;
            }

        }

        double itemWidth = 6; //K线宽度
        double itemSpace = 2;//K线之间的间隙
        double maxChartOffset = 0;
        double chartOffset = 0;

        double getMaxOffset()
        {
            if (priceFile==null||totalItemCount==0)
            {
                return 0;
            }
            var cnt = this.width / (itemWidth + itemSpace);
            
            var offset = (totalItemCount - cnt) * (itemWidth + itemSpace);
            if (offset < 0)
            {
                offset = 0;
            }
            
            return offset + 6;
        }


        void drawBackground(DrawingContext dc) {
            var brush=getBrush(246,255,255);
            var pen=getPen(224, 224, 224, 1);
            dc.DrawRectangle(brush, blackPen, rect);
        }

        Pen blackPen = new Pen(new SolidColorBrush(Color.FromRgb(128, 128, 128)), 1);
        //绘制线框，用途在于可视化等分图表
        void drawGrid(DrawingContext dc,double left, double top, double width, double height, double lineCount)
        {
            //绘制竖线.
            for (double x = 0; x < width + 1; x += width / (lineCount + 1))
            {
                double xPos = (int)(left + x) + 0.5;
                dc.DrawLine(blackPen, new Point(xPos, top), new Point(xPos, height + top));
            }

            //绘制横线.
            for (double y = 0; y < height + 1; y += height / (lineCount + 1))
            {
                var yPos = (int)(top + y) + 0.5;
                dc.DrawLine(blackPen, new Point(0, yPos), new Point(width, yPos));
            }

        }

        //可视区域的最高最低值。
        double highestRegionPrice = 0;
        double lowestRegionPrice = 0;
        double highestRegionVol = 0;


        List<int> emaDays = new List<int>();

        void drawEMALines(DrawingContext dc, double left, double top, double width, double height)
        {
           
            for(int i=0;i< emaDays.Count;++i)
            {
                drawEMALine(emaDays[i], dc, left, top, width, height);
            }

        }

        //
        void drawEMALine(int days,DrawingContext dc, double left, double top, double width, double height)
        {
            
            double[] emas = priceFile.getEMAs(days);

            //draw line curve.
            PathFigure pf = new PathFigure();
            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);

            bool started = false;
            var itemOffset = this.width / drawItemCount;
            for (var i = 0; i < drawItemCount; i ++)
            {
                var xoffset = (int)(i * itemOffset) + 0.5 + left;
                var yoffset = (int)((highestRegionPrice - emas[drawItemStartIndex+i]) / (highestRegionPrice - lowestRegionPrice) * (height)) + 0.5 + top;
                if (yoffset < 0)
                {
                        continue;

                }


                if (yoffset > height)
                {
                        continue;

                }


                if (!started)
                {
                    pf.StartPoint = new Point(xoffset, yoffset);
                    started = true;
                }
                else
                {
                    pf.Segments.Add(new LineSegment(new Point(xoffset, yoffset), true));
                }

            }

            //var pen = getPen(48, 184, 243, 1);
            var pen = getPen(20, 20, 20, 1);
            dc.DrawGeometry(Brushes.Transparent, pen, pg);

        }

        //绘制顶部K线图
        void drawKLine(DrawingContext dc, double left, double top, double width, double height)
        {
            if (!renderKLine)
            {
                return;
            }

            //绘制可视区域的K线
            for (int i = 0; i < drawItemCount; ++i)
            {
                int itemIndex = drawItemStartIndex + i;
                double xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                double yTop = (int)((highestRegionPrice - priceFile.Highs[itemIndex]) / (highestRegionPrice - lowestRegionPrice) * height) + 0.5 + top;

                double yBottom = (int)((highestRegionPrice - priceFile.Lows[itemIndex]) / (highestRegionPrice - lowestRegionPrice) * height) + 0.5 + top;
                double yOpen = (int)((highestRegionPrice - priceFile.Opens[itemIndex]) / (highestRegionPrice - lowestRegionPrice) * height) + 0.5 + top;
                double yClose = (int)((highestRegionPrice - priceFile.Closes[itemIndex]) / (highestRegionPrice - lowestRegionPrice) * height) + 0.5 + top;
                double bodyBottom = yOpen;
                double bodyTop = yClose;

                Color bodyColor = new Color();
                bodyColor.R = 255;
                bodyColor.G = 0;
                bodyColor.B = 0;
                if (priceFile.Opens[itemIndex] > priceFile.Closes[itemIndex])
                {
                    bodyTop = yOpen;
                    bodyBottom = yClose;
                    bodyColor.R = 0;
                    bodyColor.G = 128;
                    bodyColor.B = 0;
                }

                var pen=getPen(0, 0, 0, 1);
                var brush=getBrush(bodyColor.R, bodyColor.G, bodyColor.B);

                //draw top vertical line
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, yTop), new Point(xoffset + itemWidth / 2, bodyTop));
                
                //draw kline body
                double bodyHeight = bodyBottom - bodyTop;
                dc.DrawRectangle(brush, pen, new Rect(xoffset, bodyTop, itemWidth, bodyHeight));

                //draw bottom line.
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, bodyBottom), new Point(xoffset + itemWidth / 2, yBottom));
            }

        }

        //
        void drawVolume(DrawingContext dc,double left, double top, double width, double height)
        {
           
            for (var i = 0; i < drawItemCount; ++i)
            {
                int itemIndex = drawItemStartIndex + i;
                double xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                double yoffset = (1.0 - priceFile.Vols[itemIndex]/ highestRegionVol) * height + 0.5 + top;
               
                Color color = new Color();
                color.R = 255;
                color.G = 0;
                color.B = 0;

                if (priceFile.Opens[itemIndex] > priceFile.Closes[itemIndex])
                {
                    color.R = 0;
                    color.G = 128;
                    color.B = 0;
                }

                var pen=getPen(color.R,color.G,color.B,1);
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, yoffset), new Point(xoffset + itemWidth / 2, top + height));
                
            }
        }

        int drawItemStartIndex = 0;
        int drawItemCount = 0;

        //计算可视区域的始末
        bool initDrawRegion()
        {
            if (priceFile == null || totalItemCount == 0)
            {
                return false;
            }

            drawItemCount = (int)(width / (itemWidth + itemSpace)) ;
            drawItemStartIndex = (int)(chartOffset / (itemWidth + itemSpace));
            if (drawItemStartIndex >= totalItemCount)
            {
                return false;
            }

            if (drawItemStartIndex + drawItemCount > totalItemCount)
            {
                drawItemCount = totalItemCount - drawItemStartIndex - 1;
            }

            highestRegionPrice = this.priceFile.getHighest(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            lowestRegionPrice = this.priceFile.getLowest(drawItemStartIndex, drawItemStartIndex + drawItemCount);
           highestRegionVol = priceFile.getHighestVolume(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            return true;
        }


        void drawMacd(DrawingContext dc, double left, double top, double width, double height)
        {
           
            double highest = priceFile.getHighestMacdDiff(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            double lowest = priceFile.getLowestMacdDiff(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            if (highest < -lowest)
            {
                highest = -lowest;
            }
            //draw histo
            var pen=getPen(0,0,255,1);
            dc.DrawLine(pen, new Point(left, top + height / 2 + 0.5), new Point(left + width, top + height / 2 + 0.5));

            var macdDiff=priceFile.macdDiff;
            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
                var xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                var yoffset = height - (int)((macdDiff[itemIndex] + highest) / (2 * highest) * height) + 0.5 + top;
                if (itemIndex > 0)
                {
                    if (macdDiff[itemIndex] * macdDiff[itemIndex - 1] < 0)
                    {
                        if (macdDiff[itemIndex - 1] < 0)
                        {
                            
                            
                            dc.DrawLine(getPen(255, 0, 0, 2.5), new Point(xoffset, top + height / 2 - 4), new Point(xoffset, top + height / 2 + 4));
                        
                            
                        }
                        if (macdDiff[itemIndex - 1] > 0)
                        {
                            dc.DrawLine(getPen(0, 128, 0, 2.5), new Point(xoffset, top + height / 2 - 4), new Point(xoffset, top + height / 2 + 4));
                        }

                        continue;
                    }

                }

                pen.Thickness=1;
                dc.DrawLine(pen,new Point(xoffset, top + height / 2 + 0.5),new Point(xoffset, yoffset));

            }



            double diffHigh = priceFile.getHighestEmaDiff(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            highest = priceFile.getHighestDea(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            if (diffHigh > highest)
            {
                highest = diffHigh;
            }
            var diffLow = priceFile.getLowestEmaDiff(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            lowest = priceFile.getLowestDea(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            if (lowest > diffLow)
            {
                lowest = diffLow;
            }

            // fast line
            pen=getPen(255,0,0,1);
            var emaDiff = priceFile.emaDiff;

            PathFigure pfFast = new PathFigure();
            PathGeometry pgFast = new PathGeometry();
            pgFast.Figures.Add(pfFast);


            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
                var xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                var yoffset = (int)((highest - emaDiff[itemIndex]) / (highest - lowest) * height) + 0.5 + top;

                if (i == 0)
                {
                    pfFast.StartPoint = new Point(xoffset, yoffset);
                }
                else
                {
                    pfFast.Segments.Add(new LineSegment(new Point(xoffset, yoffset), true));
                    
                }
            }
            dc.DrawGeometry(Brushes.Transparent, pen, pgFast);

            //slow line
            pen = getPen(0, 128, 0, 1);
            PathFigure pfSlow = new PathFigure();
            PathGeometry pgSlow = new PathGeometry();
            pgSlow.Figures.Add(pfSlow);
            
            var dea = priceFile.dea;

            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
                var xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                var yoffset = (int)((highest - dea[itemIndex]) / (highest - lowest) * height) + 0.5 + top;

                if (i == 0)
                {
                    pfSlow.StartPoint = new Point(xoffset, yoffset);

                }
                else
                {
                    pfSlow.Segments.Add(new LineSegment(new Point(xoffset, yoffset), true));

                }
            }
            dc.DrawGeometry(Brushes.Transparent, pen, pgSlow);

        }



            //draw stock curve.
        void drawTimeLine(DrawingContext dc, double left, double top, double width, double height)
        {
            if (priceFile == null)
            {
                return;
            }
            
            var itemOffset = this.width / totalItemCount;

            //draw time line
            var yearLast = 0;
            for (var i = 0; i < totalItemCount; i++)
            {
                var xoffset = (int)(i * itemOffset) + left + 0.5;
                var year = (int)(priceFile.Dates[i] / 10000);
                if (year > yearLast)
                {
                    FormattedText txt = new FormattedText(year.ToString(),
                     System.Globalization.CultureInfo.CurrentCulture,
                     FlowDirection.LeftToRight, new Typeface("Verdana"),
                     12, new SolidColorBrush(Color.FromRgb(64, 64, 64)));
                    dc.DrawText(txt, new Point(xoffset, top+height-16));
                }
                yearLast = year;

            }

            //draw curve line
            var highest = priceFile.getHighest(0, totalItemCount);
            var lowest = priceFile.getLowest(0, totalItemCount);

            var pixelcount = 3;
            var inc = (int)(totalItemCount * pixelcount / this.width);
            if (inc == 0)
            {
                inc = 1;
            }

            //var color = "#30b8f3";
            var pen = getPen(48, 184, 243, 1);


            //draw line curve.
            PathFigure pf = new PathFigure();
            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);


            for (var i = 0; i < totalItemCount; i += inc)
            {
                var xoffset = (int)(i * itemOffset) + 0.5 + left;
                var yClose = (int)((highest - priceFile.Closes[i]) / (highest - lowest) * (height - 12)) + 0.5 + top;

                if (i == 0)
                {
                    pf.StartPoint = new Point(xoffset, yClose);
                }
                else
                {
                    pf.Segments.Add(new LineSegment(new Point(xoffset, yClose), true));
                }

            }

            dc.DrawGeometry(Brushes.Transparent, pen,pg);
         
            //draw item time region button
            //this item offset is
            double x = (int)(drawItemStartIndex * itemOffset) + left + 0.5;
            var xwidth = (int)(drawItemCount * itemOffset);

            var brush=getBrush(224,224,255,128);
            pen = getPen(224, 224, 255,1,128);
            dc.DrawRectangle(brush, pen, new Rect(x,top,xwidth,height));

         
        }

       
        public Pen getPen(byte r, byte g, byte b, double thickness,byte a=255)
        {
            Pen pen = new Pen();
            Color penColor = Color.FromArgb(a, r, g, b);

            pen.Brush = new SolidColorBrush(penColor);
            pen.Thickness = thickness;
            //pen.Freeze();
            return pen;
        }

         public Brush getBrush(byte r, byte g, byte b,byte a=255)
        {
            var brush = new SolidColorBrush(Color.FromArgb(a,r, g, b));
            //brush.Freeze();
            return brush;
        }



         internal void setType(int p)
         {
             if (p == bottomType)
             {
                 return;
             }
             bottomType = p;
             this.InvalidateVisual();
         }

         public delegate void PriceChangedEventHandler(DayPrice p);
         public event PriceChangedEventHandler OnPriceChanged;

         public delegate void RegionChangedHandler();
         public event RegionChangedHandler OnRegionChanged;

         internal void mouseMove(MouseEventArgs e)
         {
             if (priceFile == null)
             {
                 return;
             }

             var pos = e.GetPosition(this);
             var x = pos.X;
             var y = pos.Y;

             if (e.LeftButton == MouseButtonState.Pressed)
             {
                 if (!isInRect(timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight, x, y))
                 {
                     chartOffset += mousePos.X - x;
                 }
                 else
                 {
                     //do our time line select task. infact we just change our chartOffset
                     //calc start index in time space.
                     var startIndex = (int)((x - timeLineLeft) / timeLineWidth * totalItemCount);
                     chartOffset = startIndex * (itemWidth + itemSpace) - klineWidth / 2.0;
                 }
                 if (OnRegionChanged != null)
                 {
                     OnRegionChanged();
                 }

                 this.InvalidateVisual();
             }

             if (chartOffset < 0)
             {
                 chartOffset = 0;
             }
             maxChartOffset = getMaxOffset();
             if (chartOffset > maxChartOffset)
             {
                 chartOffset = maxChartOffset;
             }
             mousePos = pos;


             if (isInRect(timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight, mousePos.X, mousePos.Y))
             {
                 return;
             }

             if (e.LeftButton != MouseButtonState.Pressed)
             {

                 var itemIndex = (int)((chartOffset + mousePos.X) / (itemWidth + itemSpace));
                 if (itemIndex < totalItemCount)
                 {
                     var price = priceFile.getPrice(itemIndex);
                     //tell listener that price item changed.
                     if (OnPriceChanged!=null)
                     {
                         OnPriceChanged(price);
                     }
                 }
             }

            

         }

         public void mouseDown(MouseButtonEventArgs e)
         {
             var pos = e.GetPosition(this);

             var x = pos.X;
             var y = pos.Y;
             if (!isInRect(timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight, x, y))
             {
                 return;
             }

             //in timeline area
             var startIndex = (int)((x - timeLineLeft) / timeLineWidth * totalItemCount);
             chartOffset = startIndex * (itemWidth + itemSpace) - klineWidth / 2.0;


             maxChartOffset = getMaxOffset();
             if (chartOffset > maxChartOffset)
             {
                 chartOffset = maxChartOffset;
             }
             if (chartOffset < 0)
             {
                 chartOffset = 0;
             }
             this.InvalidateVisual();
         }


         public double KLinePartHeight
         {
             get
             {
                 return this.klineHeight;
             }
         }

         public double HighestPrice
         {
             get
             {
                 return this.highestRegionPrice;
             }
         }

         public double LowestPrice
         {
             get
             {
                 return this.lowestRegionPrice;
             }
         }


         public double TotalWidth
         {
             get
             {
                 double totalWidth = (totalItemCount) * (this.itemWidth + this.itemSpace);
                 return totalWidth;
             }
         }

         internal double getPriceY(MouseEventArgs e)
         {
             double width = this.ActualWidth;
             double height = this.ActualHeight;

             double y = e.GetPosition(this).Y;

             double price = this.highestRegionPrice - y / this.klineHeight * (highestRegionPrice - lowestRegionPrice);
             return price;

            
         }

         internal double getPriceYPercent(MouseEventArgs e)
         {
             double width = this.ActualWidth;
             double height = this.ActualHeight;

             double y = e.GetPosition(this).Y;

             double price = this.highestRegionPrice - y / this.klineHeight * (this.highestRegionPrice - this.lowestRegionPrice);

             double avg = (this.highestRegionPrice + this.lowestRegionPrice) / 2.0;
             double percent = (price - avg) / avg;

             return percent;
         }

         public DayPrice LastPrice { get { return this.priceFile.LastPrice; } }

         bool renderKLine = true;
         internal void flipKLine()
         {
             renderKLine = !renderKLine;
             this.InvalidateVisual();
         }
    }
}
