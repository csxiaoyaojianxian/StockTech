//可视化股票技术分析软件
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StockTech.Data;
using StockTech.Py;
using StockTech.Util;

namespace StockTech
{
    //绘制技术分析图表相关...
    class DrawingCanvas : FrameworkElement
    {
        public DrawingCanvas()
        {         
            addEMADays(5);//
            addEMADays(10);//
            addEMADays(30);//

            load();//初始化界面，载入数据
            PyEngine.Inst.initCanvas(this);//绘制曲线
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

        string symbol = null;

        public void loadLastSymbol()
        {
            if (symbol == null)
            {
                load();
            }
            else
            {
                load(symbol);
            }         
        }

        //DayFile priceFile = null;
        public void load(string symbol = "sh000001")
        {
            if (this.symbol == symbol)
            {
                return;
            }

            //clear all.
            this.objects.Clear();
            
            this.symbol = symbol;
            //this.priceFile = DayFile.get(symbol);
            this.priceList = DayDb.Inst.getPriceList(symbol);

            if (this.priceList==null)
            {
                return ;
            }
            //记录条数
            this.totalItemCount = this.priceList.ItemCount;// (int)this.priceFile.ItemCount;
      
            chartOffset = getMaxOffset();
            maxChartOffset = chartOffset;

            PyEngine.Inst.addArray("open", this.priceList.Opens,this.priceList.ItemCount);
            PyEngine.Inst.addArray("close", this.priceList.Closes, this.priceList.ItemCount);
            PyEngine.Inst.addArray("high", this.priceList.Highs, this.priceList.ItemCount);
            PyEngine.Inst.addArray("low", this.priceList.Lows, this.priceList.ItemCount);
            PyEngine.Inst.addArray("vol", this.priceList.Vols, this.priceList.ItemCount);
            PyEngine.Inst.addArray("amount", this.priceList.Amounts, this.priceList.ItemCount);
           
            PyEngine.Inst.runLast();

            this.InvalidateVisual();

        }


        double totalWidth = 0;
        double totalHeight = 0;
        Rect rect = new Rect();

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


        //判断坐标是否在矩形框内
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

        //显示
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

            //绘制K线图框
            drawGrid(dc, klineLeft, klineTop, klineWidth, klineHeight, 5);
            //绘制K线图
            drawObjects(dc);

            //build in 中框和日期
            drawDateTxt(dc, dateLeft, dateTop, dateWidth, dateHeight, 5);
            drawGrid(dc, volumeLeft, volumeTop, volumeWidth, volumeHeight, 0);
            //bottom
            drawGrid(dc, timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight, 0);
            drawTimeLine(dc, timeLineLeft, timeLineTop, timeLineWidth, timeLineHeight);


            if (OnRegionChanged != null)
            {
                OnRegionChanged();
            }

        }


        int totalItemCount = 0;
        PriceList priceList = null;
        //绘制日期
        void drawDateTxt(DrawingContext dc, double left, double top, double width, double height, int lineCount)
        {
            if (priceList == null || totalItemCount == 0)
            {
                return;
            }

            var startIndex = (int)(chartOffset / (itemWidth + itemSpace));
            var cnt = (int)(width / (itemWidth + itemSpace));
            var itemOffset = (int)(width / (itemWidth + itemSpace) / (lineCount + 1));

            for (var i = 0; i < lineCount + 1; i++)
            {
                if (i * itemOffset + startIndex >= totalItemCount)
                {
                    break;
                }
                var xoffset = (int)(i * width / (lineCount + 1)) + left + 0.5;
                var date = priceList.Dates[i * itemOffset + startIndex];
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
        //中间时间轴高度，为实际高度，非比例
        static double dateHeight = 16;//this part

        //初始化布局，计算布局坐标
        void initLayout()
        {
            this.totalWidth = this.ActualWidth;
            this.totalHeight = this.ActualHeight;
            //Draw a Rectangle
            this.rect.Width = totalWidth;
            this.rect.Height = totalHeight;

            //定位part1:上部K线图
            this.klineLeft = 0;
            this.klineTop = 0;
            this.klineWidth = this.totalWidth;
            //计算K线高度
            this.klineHeight = (this.totalHeight - dateHeight) * klinePart / (toalPart);

            //定位Part2:中部日期轴
            dateLeft = 0;
            dateTop = klineHeight;
            dateWidth = this.totalWidth;

            //定位Part3:中部柱状图
            volumeLeft = 0;
            volumeTop = dateTop + dateHeight;
            volumeWidth = this.totalWidth;
            volumeHeight = (this.totalHeight - dateHeight) * volumePart / (toalPart);

            //定位Part4:下部时间轴
            timeLineLeft = 0;
            timeLineTop = volumeTop + volumeHeight;
            timeLineWidth = this.totalWidth;
            timeLineHeight = (this.totalHeight - dateHeight) * timeLinePart / (toalPart);

            //确定偏移量
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
            if (priceList == null || totalItemCount == 0)
            {
                return 0;
            }
            //可显示的K线的数量
            var cnt = this.totalWidth / (itemWidth + itemSpace);

            //整个中间部分的宽度（不可见部分）即显示的第一个记录在总记录中的位置（换算成长度）
            var offset = (totalItemCount - cnt) * (itemWidth + itemSpace);
            if (offset < 0)//总记录数不满一屏
            {
                offset = 0;
            }

            return offset + 6;//末尾不贴到边界
        }

        //中间矩形内容框的背景色
        void drawBackground(DrawingContext dc)
        {
            var brush = getBrush(246, 255, 255);//填充色
            var pen = getPen(224, 224, 224, 1);//线条色
            dc.DrawRectangle(brush, pen, rect);
        }

        Pen blackPen = new Pen(new SolidColorBrush(Color.FromRgb(128, 128, 128)), 1);
        //绘制线框，用途在于可视化等分图表，即上部表格，lineCount从0开始
        void drawGrid(DrawingContext dc, double left, double top, double width, double height, double lineCount)
        {
            //绘制竖线.
            for (double x = 0; x < width + 1; x += width / (lineCount + 1))
            {
                //xPos指
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

        int drawItemStartIndex = 0;
        int drawItemCount = 0;

        //计算可视区域的始末
        bool initDrawRegion()
        {
            if (priceList == null || totalItemCount == 0)
            {
                return false;
            }

            //显示的记录个数和开始序号
            drawItemCount = (int)(totalWidth / (itemWidth + itemSpace));
            drawItemStartIndex = (int)(chartOffset / (itemWidth + itemSpace));
            if (drawItemStartIndex >= totalItemCount)
            {
                return false;
            }
            //微调显示的个数，确保能显示完全
            if (drawItemStartIndex + drawItemCount > totalItemCount)
            {
                drawItemCount = totalItemCount - drawItemStartIndex - 1;
            }
            //确定纵坐标范围
            highestRegionPrice = this.priceList.getHighestPrice(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            lowestRegionPrice = this.priceList.getLowestPrice(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            highestRegionVol = priceList.getHighestVolume(drawItemStartIndex, drawItemStartIndex + drawItemCount);
            return true;

        }

        //draw stock curve.
        void drawTimeLine(DrawingContext dc, double left, double top, double width, double height)
        {
            if (priceList == null)
            {
                return;
            }
            //每条记录的宽度
            var itemOffset = this.totalWidth / totalItemCount;

            //draw time line
            var yearLast = 0;
            for (var i = 0; i < totalItemCount; i++)
            {
                var xoffset = (int)(i * itemOffset) + left + 0.5;
                var year = (int)(priceList.Dates[i] / 10000);
                if (year > yearLast)
                {
                    //格式文本，指定位置
                    FormattedText txt = new FormattedText(year.ToString(),
                     System.Globalization.CultureInfo.CurrentCulture,
                     FlowDirection.LeftToRight, new Typeface("Verdana"),
                     12, new SolidColorBrush(Color.FromRgb(64, 64, 64)));
                    //绘制文本
                    dc.DrawText(txt, new Point(xoffset, top + height - 16));
                }
                yearLast = year;

            }

            //draw curve line
            var highest = priceList.getHighestPrice(0, totalItemCount);
            var lowest = priceList.getLowestPrice(0, totalItemCount);

            var pixelcount = 3;
            var inc = (int)(totalItemCount * pixelcount / this.totalWidth);
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
                var yClose = (int)((highest - priceList.Closes[i]) / (highest - lowest) * (height - 12)) + 0.5 + top;

                if (i == 0)
                {
                    pf.StartPoint = new Point(xoffset, yClose);
                }
                else
                {
                    pf.Segments.Add(new LineSegment(new Point(xoffset, yClose), true));
                }

            }

            dc.DrawGeometry(Brushes.Transparent, pen, pg);

            //draw item time region button
            //this item offset is
            double x = (int)(drawItemStartIndex * itemOffset) + left + 0.5;
            var xwidth = (int)(drawItemCount * itemOffset);

            var brush = getBrush(224, 224, 255, 128);
            pen = getPen(224, 224, 255, 1, 128);
            dc.DrawRectangle(brush, pen, new Rect(x, top, xwidth, height));


        }

        //定义笔
        public Pen getPen(byte r, byte g, byte b, double thickness, byte a = 255)
        {
            Pen pen = new Pen();
            Color penColor = Color.FromArgb(a, r, g, b);

            pen.Brush = new SolidColorBrush(penColor);
            pen.Thickness = thickness;
            //pen.Freeze();
            return pen;
        }

        public Brush getBrush(byte r, byte g, byte b, byte a = 255)
        {
            var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            //brush.Freeze();
            return brush;
        }


        //定义刷
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
            if (priceList == null)
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
                    var price = priceList.getPrice(itemIndex);
                    //tell listener that price item changed.
                    if (OnPriceChanged != null)
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

        public DayPrice LastPrice
        {
            get
            {
                if (this.priceList==null)
                {
                    return null;
                }
                return this.priceList.LastPrice;
            }
        }

        bool renderKLine = true;
        internal void flipKLine()
        {
            renderKLine = !renderKLine;
            this.InvalidateVisual();
        }

        internal void addDrawingObj(string id, double[] points, int part, Color c, double thickness,DrawingObjectType type )
        {
            DrawingObject obj =new DrawingObject()
            {
                Vals = points,
                Part = part,
                Color = c,
                Thickness = thickness,
                Type=type,
            };

            if (objects.ContainsKey(id))
            {
                objects[id] = obj;
                return;
            }

           
            objects.Add(id,obj);

        }

        //
        Dictionary<string, DrawingObject> objects = new Dictionary<string, DrawingObject>();
        private void drawObjects(DrawingContext dc)
        {
            //均线
            drawObjectGroup(dc, 0, DrawingObjectType.Line);
            //MADC曲线
            drawObjectGroup(dc, 1, DrawingObjectType.Line);
            //
            drawObjectGroup(dc, 0, DrawingObjectType.zVLines);
            //MACD垂线
            drawObjectGroup(dc, 1, DrawingObjectType.zVLines);
            //蜡烛图
            drawObjectGroup(dc, 0, DrawingObjectType.CandleLine);
            //
            drawObjectGroup(dc, 1, DrawingObjectType.CandleLine);
            //
            drawObjectGroup(dc, 0, DrawingObjectType.vLines);
            //成交量垂线
            drawObjectGroup(dc, 1, DrawingObjectType.vLines);

        }

        private void drawObjectGroup(DrawingContext dc, int part, DrawingObjectType type)
        {
            double highest = 0;
            double lowest = double.MaxValue;

            foreach (var i in objects)
            {
                var obj = i.Value;
                if (obj.Part != part || obj.Type != type)
                {
                    continue;
                }
                double h = 0;
                double l = 0;
                //candle line.
                if (obj.Type == DrawingObjectType.CandleLine)
                {
                    h = MathUtil.getHighest(obj.Vals2, drawItemStartIndex, drawItemStartIndex + drawItemCount,priceList.ItemCount);
                    if (h > highest)
                    {
                        highest = h;
                    }
                    l = MathUtil.getLowest(obj.Vals3, drawItemStartIndex, drawItemStartIndex + drawItemCount, priceList.ItemCount);
                    if (l < lowest)
                    {
                        lowest = l;
                    }
                    continue;
                }

                h = MathUtil.getHighest(obj.Vals, drawItemStartIndex, drawItemStartIndex + drawItemCount, priceList.ItemCount);
                if (h > highest)
                {
                    highest = h;
                }
                l = MathUtil.getLowest(obj.Vals, drawItemStartIndex, drawItemStartIndex + drawItemCount, priceList.ItemCount);
                if (l < lowest)
                {
                    lowest = l;
                }

                //special for zero bars.
                if (obj.Type == DrawingObjectType.zVLines)
                {
                    if (highest < -lowest)
                    {
                        highest = -lowest;
                    }
                }

            }

            foreach (var i in objects)
            {
                var obj = i.Value;
                if (obj.Part == part && obj.Type == type)
                {
                    drawObj(dc, highest, lowest, obj);
                }
            }
        }

        private void drawObj(DrawingContext dc,double highest,double lowest,DrawingObject obj)
        {
            initPart(obj.Part);
            switch (obj.Type)
            {
                case DrawingObjectType.Line:
                    drawLine(dc, highest, lowest, obj);
                    break;
                case DrawingObjectType.zVLines:
                    drawZeroVerticalLines(dc, highest, lowest, obj);
                    break;
                case DrawingObjectType.CandleLine:
                    drawCandleLine(dc, highest, lowest, obj);
                    break;
                case DrawingObjectType.vLines:
                    drawVLines(dc, highest, lowest, obj);
                    break;
            }

        }

        private void drawVLines(DrawingContext dc, double highest, double lowest, DrawingObject obj)
        {
            if (obj.Type != DrawingObjectType.vLines)
            {
                return;
            }
            var vals = obj.Vals;
            for (var i = 0; i < drawItemCount; ++i)
            {
                int itemIndex = drawItemStartIndex + i;
                double xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                double yoffset = (1.0 - vals[itemIndex] / highest) * height + 0.5 + top;

                Color color = obj.Color;

                if (obj.drawItemHandler != null)
                {
                    //very slow.
                    /*
                    JSEngine.Inst.call(obj.drawItemHandler,priceFile.Opens[itemIndex],
                        priceFile.Closes[itemIndex],
                        priceFile.Highs[itemIndex],
                        priceFile.Lows[itemIndex],
                        priceFile.Vols[itemIndex],
                        priceFile.Amounts[itemIndex]);
                     * */
                }

                var pen = getPen(color.R, color.G, color.B, 1);
                if (priceList.Opens[itemIndex] > priceList.Closes[itemIndex])
                {
                    pen = getPen(255, 0, 0, 1);
                }
                else
                {
                    pen = getPen(0, 128, 0, 1);
                }

                
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, yoffset), new Point(xoffset + itemWidth / 2, top + height));

            }

        }

        private void drawCandleLine(DrawingContext dc, double highest, double lowest, DrawingObject obj)
        {
            var opens = obj.Vals;
            var closes = obj.Vals1;
            var highs = obj.Vals2;
            var lows = obj.Vals3;
            if (opens == null || closes == null || highs == null || lows == null 
                || opens.Length==0
                || opens.Length != closes.Length || closes.Length != highs.Length || highs.Length != lows.Length)
            {
                return;
            }

            //绘制可视区域的K线
            for (int i = 0; i < drawItemCount; ++i)
            {
                int itemIndex = drawItemStartIndex + i;
                double xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                double yTop = (int)((highest - highs[itemIndex]) / (highest - lowest) * height) + 0.5 + top;

                double yBottom = (int)((highest - lows[itemIndex]) / (highest - lowest) * height) + 0.5 + top;
                double yOpen = (int)((highest - opens[itemIndex]) / (highest - lowest) * height) + 0.5 + top;
                double yClose = (int)((highest - closes[itemIndex]) / (highest - lowest) * height) + 0.5 + top;
                double bodyBottom = yOpen;
                double bodyTop = yClose;

                Color bodyColor = new Color();
                bodyColor.R = 255;
                bodyColor.G = 0;
                bodyColor.B = 0;
                if (opens[itemIndex] > closes[itemIndex])
                {
                    bodyTop = yOpen;
                    bodyBottom = yClose;
                    bodyColor.R = 0;
                    bodyColor.G = 128;
                    bodyColor.B = 0;
                }

                var pen = getPen(0, 0, 0, 1);
                var brush = getBrush(bodyColor.R, bodyColor.G, bodyColor.B);

                //draw top vertical line
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, yTop), new Point(xoffset + itemWidth / 2, bodyTop));

                //draw kline body
                double bodyHeight = bodyBottom - bodyTop;
                dc.DrawRectangle(brush, pen, new Rect(xoffset, bodyTop, itemWidth, bodyHeight));

                //draw bottom line.
                dc.DrawLine(pen, new Point(xoffset + itemWidth / 2, bodyBottom), new Point(xoffset + itemWidth / 2, yBottom));
            }


        }

        double left = 0;
        double top = 0;
        double height = 0;


        private void initPart(int part)
        {

            left = klineLeft;

            if (part == 0)
            {
                top = klineTop;
                height = klineHeight;

            }

            if (part == 1)
            {
                top = volumeTop;
                height = volumeHeight;

            }

        }


        private void drawZeroVerticalLines(DrawingContext dc, double highest, double lowest, DrawingObject obj)
        {
            if (obj.Type != DrawingObjectType.zVLines)
            {
                return;
            }
            initPart(obj.Part);

            var pen = getPen(0, 0, 255, 1);

            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
                var xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                var yoffset = height - (int)((obj.Vals[itemIndex] + highest) / (2 * highest) * height) + 0.5 + top;
                if (itemIndex > 0)
                {
                    if (obj.Vals[itemIndex] * obj.Vals[itemIndex - 1] < 0)
                    {
                        if (obj.Vals[itemIndex - 1] < 0)
                        {
                            dc.DrawLine(getPen(255, 0, 0, 2.5), new Point(xoffset, top + height / 2 - 4), new Point(xoffset, top + height / 2 + 4));
                        }
                        if (obj.Vals[itemIndex - 1] > 0)
                        {
                            dc.DrawLine(getPen(0, 128, 0, 2.5), new Point(xoffset, top + height / 2 - 4), new Point(xoffset, top + height / 2 + 4));
                        }

                        continue;
                    }

                }

                pen.Thickness = 1;
                dc.DrawLine(pen, new Point(xoffset, top + height / 2 + 0.5), new Point(xoffset, yoffset));

            }
        }

        private void drawLine(DrawingContext dc, double highest, double lowest, DrawingObject obj)
        {
            var pen = getPen(obj.Color.R, obj.Color.G, obj.Color.B, obj.Thickness);

            PathFigure pfFast = new PathFigure();
            PathGeometry pgFast = new PathGeometry();
            pgFast.Figures.Add(pfFast);

            for (var i = 0; i < drawItemCount; ++i)
            {
                var itemIndex = drawItemStartIndex + i;
                var xoffset = (int)(i * (itemWidth + itemSpace)) + 0.5 + left;
                var yoffset = (int)((highest - obj.Vals[itemIndex]) / (highest - lowest) * height) + 0.5 + top;

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
        }

        //
        internal void addKLineObj(string id,int part, double[] open, double[] close, double[] high, double[] low)
        {
            DrawingObject obj = new DrawingObject()
            {
                Type=DrawingObjectType.CandleLine,
                Vals=open,
                Vals1=close,
                Vals2=high,
                Vals3=low
            };

            if (objects.ContainsKey(id))
            {
                objects[id] = obj;
                return;
            }

            objects.Add(id, obj);

        }

        internal void clearDrawings()
        {
            objects.Clear();
        }

        internal void setDrawItemEventHandler(string id, string handlerName)
        {
            if(objects.ContainsKey(id)){
                    objects[id].drawItemHandler=handlerName;
            }
           
        }

        internal void reload(string techScriptName)
        {
            PyEngine.Inst.reload(techScriptName);
        }

        internal void contextMenuClick(string txt)
        {
            PyEngine.Inst.contextMenuClick(txt);
        }
    }

    public enum DrawingObjectType
    {
        Line,
        zVLines,
        CandleLine,
        vLines,
    }

    class DrawingObject
    {
        public Color Color;
        public int Part;
        public double Thickness;

        public DrawingObjectType Type;

        public double[] Vals;
        public double[] Vals1;
        public double[] Vals2;
        public double[] Vals3;

        public string drawItemHandler = null;

    }
}