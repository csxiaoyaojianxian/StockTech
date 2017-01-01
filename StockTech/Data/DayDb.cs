//可视化股票技术分析软件
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using StockTech.Util;

namespace StockTech.Data
{
  
    public class DayDb
    {
        static DayDb inst = null;
        public static DayDb Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new DayDb();
                }
                return inst;
            }
        }

        
        List<string> symbols = new List<string>();
        string datePath = "./Data/Db/date.txt";
        string symbolPath = "./Data/Db/symbols.txt";
        string dbPath = "./Data/Db/daydb";

        string dbDatesFilePath = "./Data/Db/daydb.dt";

        //test_offset
        int test_offset = 40000;

        protected DayDb()
        {
            load();
        }
        
        public void load()
        {
            if (!File.Exists(datePath) || !File.Exists(symbolPath) || !File.Exists(dbPath))
            {
                return;
            }
            //载入日期
            var f = File.Open(datePath, FileMode.Open);
            var r = new StreamReader(f);
            while (!r.EndOfStream)
            {
                var s = r.ReadLine();
                int d = 0;
                if (Int32.TryParse(s, out d))
                {
                    //dateAll.Add(d);
                    
                    //test
                    dateAll.Add(d + test_offset);
                }
            }
            f.Close();
            //载入股票代码
            f = File.Open(symbolPath, FileMode.Open);
            r = new StreamReader(f);
            while (!r.EndOfStream)
            {
                var s = r.ReadLine();
                if (s != "")
                {
                    symbols.Add(s);
                }
            }
            f.Close();
            //
            readDatesInDb();
            readDbArrays();
        }


        int[,] openArray = null;
        int[,] closeArray = null;
        int[,] highArray = null;
        int[,] lowArray = null;
        int[,] volArray = null;
        float[,] amountArray = null;
        List<int> dateAll = new List<int>();
        List<int> datesInDb = new List<int>();

        /// <summary>
        /// 从文件读日期
        /// </summary>
        private void readDatesInDb()
        {
            // Db\daydb.dt
            FileStream f = File.Open(dbDatesFilePath, FileMode.Open);
            BinaryReader r = new BinaryReader(f);

            int len = (int)f.Length;
            int pos = 0;
            while (pos < len)
            {
                var i = r.ReadInt32();
                //datesInDb.Add(i);

                //test
                datesInDb.Add(i + test_offset);

                pos += sizeof(int);
            }
            r.Close();
            f.Close();
            //delegate委托
            datesInDb.Sort(
               delegate(int x, int y)
               {
                   return
                       x > y ? 1 :
                       x < y ? -1 :
                       0;
               });

        }

        /// <summary>
        /// 读取数据到数组
        /// </summary>
        private void readDbArrays()
        {

            if (datesInDb.Count < 1)
            {
                return;
            }

           FileStream  f = File.Open(dbPath, FileMode.Open);
           BinaryReader r = new BinaryReader(f);
            //
            //日期的起始日、结束日、总天数、股票数
            int dateStartIndex = dateAll.IndexOf(datesInDb[0]);
            int dateEndIndex = dateAll.IndexOf(datesInDb[datesInDb.Count - 1]);
            
            int dtCnt = dateEndIndex - dateStartIndex + 1;
            int symCnt = symbols.Count;
            //check file bytes.

            openArray = new int[dtCnt, symCnt];
            closeArray = new int[dtCnt, symCnt];
            highArray = new int[dtCnt, symCnt];
            lowArray = new int[dtCnt, symCnt];
            volArray = new int[dtCnt, symCnt];
            amountArray = new float[dtCnt, symCnt];

            int priceChunkSize = sizeof(int) * 5 + sizeof(float);
            int rowBytes = priceChunkSize * symbols.Count;//a row is in same date.
            int startOffset = rowBytes * dateStartIndex;
            int endOffset = rowBytes *( dateEndIndex+1);

            f.Seek(startOffset, SeekOrigin.Begin);
            int offset = startOffset;
            //读取每天的所有股票的数据
            for (int i = 0; i < dtCnt; i++)
            {
                for (var j = 0; j < symCnt; j++)
                {
                    openArray[i, j] = r.ReadInt32();
                    closeArray[i, j] = r.ReadInt32();
                    highArray[i, j] = r.ReadInt32();
                    lowArray[i, j] = r.ReadInt32();
                    volArray[i, j] = r.ReadInt32();
                    amountArray[i, j] = r.ReadSingle();
                    
                }
                
            }

            r.Close();
            f.Close();

        }

        /// <summary>
        /// 读数据到程序
        /// </summary>

        //根据symbol获取每日的Price
        public PriceList getPriceList(string symbol)
        {
            //确定索引
            int symbolIndex = symbols.IndexOf(symbol);
            if (symbolIndex < 0)
            {
                return null;
            }
            if (openArray == null || closeArray == null
                || highArray == null || lowArray == null
                || volArray == null || amountArray == null)
            {
                return null;
            }
            
            int dtCnt = datesInDb.Count;

            //itemCount为实际在K线图显示的数据的索引
            int itemCount = 0;
            //初始化
            int[] dates = new int[dtCnt];
            double[] opens = new double[dtCnt];
            double[] closes = new double[dtCnt];
            double[] highs = new double[dtCnt];
            double[] lows = new double[dtCnt];
            double[] vols = new double[dtCnt];
            double[] amounts = new double[dtCnt];

            for (var i = 0; i < dtCnt; i++)
            {
               
                var date = datesInDb[i];
                double open = openArray[i, symbolIndex];
                double close = closeArray[i, symbolIndex];
                double high = highArray[i, symbolIndex];
                double low = lowArray[i, symbolIndex];
                double vol = volArray[i, symbolIndex];
                double amount = amountArray[i, symbolIndex]; 

                //
                if (open > 0 && close > 0 && high > 0 && low > 0&&vol>0&&amount>0)
                {
                    dates[itemCount] = date;//read into kline
                    opens[itemCount] = open;
                    closes[itemCount] = close;
                    highs[itemCount] = high;
                    lows[itemCount] = low;
                    vols[itemCount] = vol;
                    amounts[itemCount] = amount;
                    itemCount++;

                }
            }

            PriceList l=new PriceList(dates,opens,closes,highs,lows,vols,amounts,itemCount);
            return l;
        }
      
    }

    public class PriceList
    {
        public int[] Dates { get { return this.dates; } }
        public double[] Opens { get { return this.opens; } }
        public double[] Closes { get { return this.closes; } }
        public double[] Highs { get { return this.highs; } }
        public double[] Lows { get { return this.lows; } }
        public double[] Vols { get { return this.vols; } }
        public double[] Amounts { get { return this.amounts; } }
        public int ItemCount { get { return this.itemCount; } }
        private int[] dates;
        private double[] opens;
        private double[] closes;
        private double[] highs;
        private double[] lows;
        private double[] vols;
        private double[] amounts;
        private int itemCount = 0;

        public PriceList(int[] dates, double[] opens, double[] closes, double[] highs, double[] lows, double[] vols,double[] amounts, int itemCount)
        {
            // TODO: Complete member initialization
            this.dates = dates;
            this.opens = opens;
            this.closes = closes;
            this.highs = highs;
            this.lows = lows;
            this.vols = vols;
            this.amounts = amounts;
            this.itemCount = itemCount;
        }

        public DayPrice LastPrice
        {
            get
            {
                return getPrice(itemCount - 1);
            }
        }

        internal DayPrice getPrice(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= itemCount)
            {
                return null;
            }

            var p = new DayPrice()
            {
                Date = dates[itemIndex],
                Open = opens[itemIndex],
                Close = closes[itemIndex],
                High = highs[itemIndex],
                Low = lows[itemIndex],
                Volume = vols[itemIndex],
                Amount = amounts[itemIndex],
            };
            return p;
        }

        internal double getHighestPrice(int start, int end)
        {
            if (end > itemCount - 1)
            {
                end = itemCount - 1;
            }
            return MathUtil.getHighest(highs, start, end,itemCount);
        }

        internal double getLowestPrice(int start, int end)
        {
            if (end > itemCount - 1)
            {
                end = itemCount - 1;
            }
            double val= MathUtil.getLowest(lows, start, end,itemCount);
           
            return val;
        }

        internal double getHighestVolume(int start, int end)
        {
            if (end > itemCount - 1)
            {
                end = itemCount - 1;
            }
            return MathUtil.getHighest(vols, start, end,itemCount);
            
        }
    }


    /// <summary>
    /// 股票每日价格
    /// </summary>
    public class DayPrice
    {
        public string Symbol { get; set; }
        public int Date { get; set; }

        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Amount { get; set; }
        public double Volume { get; set; }

        public double AmountBillion
        {
            get
            {
                return Amount / 100000000;
            }
        }


        public string Name
        {
            get
            {
                return MetaFile.Inst.getName(Symbol);
            }
        }

        public int DayOfWeek
        {
            get
            {
                DateTime dt = CommonUtil.getDate(Date);
                return CommonUtil.getDayOfWeek(dt.DayOfWeek);
            }
        }

        public double Change
        {
            get
            {
                double percent = (Close - Open) / Open;
                return percent;

            }
        }

        public double CloseVOpen
        {
            get
            {
                double percent = (Close - Open) / Open * 100;
                return percent;

            }
        }

        public double HighVOpen
        {
            get
            {
                double percent = (High - Open) / Open * 100;
                return percent;

            }
        }

        public double LowVOpen
        {
            get
            {
                double percent = (Low - Open) / Open * 100;
                return percent;

            }
        }
        
    }

}
