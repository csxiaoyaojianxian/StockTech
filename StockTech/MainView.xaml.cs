//可视化股票技术分析软件

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StockTech.Controls;
using StockTech.Data;
using StockTech.Util;
using StockTech.Py;

namespace StockTech
{
    /// <summary>
    /// Interaction logic for TechView.xaml
    /// </summary>
    /// 
    public partial class MainView : UserControl
    {
        DrawingCanvas canvas = new DrawingCanvas();
        CrossLine crossLine = new CrossLine();
        const int MaxPriceLines = 5;

        //跟随鼠标移到的左文字块，用于显示支撑位，阻力位价格
        TextBlock leftTextBlock = new TextBlock()
        {
            Text = "0.0",
            Background = new SolidColorBrush(Colors.Azure),
            FontSize = 12,
            Margin = new Thickness(2, 0, 0, 0),
            Foreground = new SolidColorBrush(Colors.Black),
            //Width=64,
        };

        //跟随鼠标移到的右文字块，用于显示价格百分比
        TextBlock rightTextBlock = new TextBlock()
        {
            Text = "0.0",
            //Width = 64,
            Background = new SolidColorBrush(Colors.Azure),
            FontSize = 12,
            Margin = new Thickness(2, 0, 0, 0),
            Foreground = new SolidColorBrush(Colors.Black),
            
        };

        //价格区间
        List<TextBlock> leftTextblocks = new List<TextBlock>();
        List<TextBlock> rightTextblocks = new List<TextBlock>();


        public delegate void PriceChangedEventHandler(DayPrice p);
        public event PriceChangedEventHandler OnPriceChanged;


        public MainView()
        {
            InitializeComponent();

            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            this.grid.Children.Add(canvas);
            this.grid.Children.Add(crossLine);

            loadFixedTexts();

            this.leftLayout.Children.Add(leftTextBlock);
            this.rightLayout.Children.Add(rightTextBlock);

           
            this.MouseMove += new MouseEventHandler(TechView_MouseMove);
            this.MouseDown += new MouseButtonEventHandler(TechView_MouseDown);
            this.MouseLeave += new MouseEventHandler(TechView_MouseLeave);
            this.MouseEnter += new MouseEventHandler(TechView_MouseEnter);
            this.SizeChanged += new SizeChangedEventHandler(TechView_SizeChanged);
            this.canvas.OnPriceChanged += new DrawingCanvas.PriceChangedEventHandler(canvas_OnPriceChanged);
            this.canvas.OnRegionChanged += new DrawingCanvas.RegionChangedHandler(canvas_OnRegionChanged);

            StockEngine.Inst.setMainView(this);
        }

      

        void contextMenu_OnKLineClick()
        {
            canvas.flipKLine();
           
        }

        void contextMenu_OnEMAClick(int days)
        {
            canvas.flipEMADays(days);
           
        }

      
        void canvas_OnRegionChanged()
        {
            setHighestLowestPrice(canvas.HighestPrice, canvas.LowestPrice);
            //throw new System.NotImplementedException();
        }

        void TechView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateTextblockLayout();
        }

        Color lineColor = Colors.Silver;
        Color textColor = Colors.Black;

        private void loadFixedTexts()
        {
            for (int i = 1; i <= MaxPriceLines + 2; i++)
            {
                TextBlock leftTxtblock = new TextBlock()
                {
                    Margin = new Thickness(1),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(textColor),
                    Text = "0",
                    SnapsToDevicePixels = true,
                };

                leftTextblocks.Add(leftTxtblock);
                this.leftLayout.Children.Add(leftTxtblock);

                TextBlock rightTxtblock = new TextBlock()
                {
                    Margin = new Thickness(1),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(textColor),
                    Text = "0",

                };

                rightTextblocks.Add(rightTxtblock);
                this.rightLayout.Children.Add(rightTxtblock);
            }
            
        }


        private void updateTextblockLayout()
        {
            foreach (var i in this.leftTextblocks)
            {
                if (i.Text.Length >= 8)
                {
                    i.FontSize = 10;
                }
                else
                {
                    i.FontSize = 12;
                }
            }

            if (leftTextblocks.Count < MaxPriceLines)
            {
                return;
            }

            double height = this.canvas.KLinePartHeight;

            for (int i = 1; i <= MaxPriceLines + 2; i++)
            {
                int y = (int)(height / (double)(MaxPriceLines + 1) * (i - 1));

                if (i == 1)
                {
                    Canvas.SetTop(leftTextblocks[MaxPriceLines + 2 - i], y);
                    Canvas.SetTop(rightTextblocks[MaxPriceLines + 2 - i], y);
                    continue;
                }

                if (i == MaxPriceLines + 2)
                {
                    Canvas.SetTop(leftTextblocks[MaxPriceLines + 2 - i], y - 18);
                    Canvas.SetTop(rightTextblocks[MaxPriceLines + 2 - i], y - 18);
                    continue;
                }

                Canvas.SetTop(leftTextblocks[MaxPriceLines + 2 - i], y - 12);
                Canvas.SetTop(rightTextblocks[MaxPriceLines + 2 - i], y - 12);

            }
        }



        internal void setHighestLowestPrice(double highest, double lowest)
        {
            if (leftTextblocks.Count < MaxPriceLines)
            {
                return;
            }

            double avg = (highest + lowest) / 2.0;
            for (int i = 1; i <= MaxPriceLines + 2; i++)
            {
                double price = lowest + (highest - lowest) / (MaxPriceLines + 1) * (i - 1);
                leftTextblocks[i - 1].Text = CommonUtil.formatPrice(price);

                double percent = (price - avg) / avg;
                rightTextblocks[i - 1].Text = CommonUtil.formatPricePercent(percent);

            }

            updateTextblockLayout();

        }

        void TechView_MouseEnter(object sender, MouseEventArgs e)
        {
            crossLine.Visibility = System.Windows.Visibility.Visible;
            leftTextBlock.Visibility = System.Windows.Visibility.Visible;
            rightTextBlock.Visibility = System.Windows.Visibility.Visible;

        }

        void canvas_OnPriceChanged(Data.DayPrice p)
        {
            if (OnPriceChanged != null)
            {

                OnPriceChanged(p);
            }
        }

        void TechView_MouseLeave(object sender, MouseEventArgs e)
        {
            crossLine.Visibility = System.Windows.Visibility.Hidden;
            leftTextBlock.Visibility = System.Windows.Visibility.Hidden;
            rightTextBlock.Visibility = System.Windows.Visibility.Hidden;
        }

        void TechView_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            canvas.mouseDown(e);
        }

        void TechView_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                leftTextBlock.Visibility = System.Windows.Visibility.Hidden;
                rightTextBlock.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                leftTextBlock.Visibility = System.Windows.Visibility.Visible;
                rightTextBlock.Visibility = System.Windows.Visibility.Visible;
            }

            
            //
            var pos = e.GetPosition(this);

            if (pos.Y > canvas.KLinePartHeight)
            {
                leftTextBlock.Visibility = System.Windows.Visibility.Hidden;
                rightTextBlock.Visibility = System.Windows.Visibility.Hidden;
            }

            Canvas.SetTop(leftTextBlock, pos.Y);
            Canvas.SetTop(rightTextBlock, pos.Y);
            canvas.mouseMove(e);

            double price = this.canvas.getPriceY(e);
            string priceText = CommonUtil.formatPrice(price);

            double percent = this.canvas.getPriceYPercent(e);
            string percentText = CommonUtil.formatPricePercent(percent);
           
            leftTextBlock.Text = priceText;
            rightTextBlock.Text = percentText;
        }

        internal void setType(int p)
        {
            canvas.setType(p);
        }

        internal void load(string symbol)
        {
            canvas.load(symbol);
        }

        public DayPrice LastPrice
        {
            get
            {
                return this.canvas.LastPrice;
            }
        }


        internal void reload(string p)
        {
            canvas.reload(p);
        }
    }
}

