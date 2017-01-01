//Copyright (c) 2010-2012, 王旭明 youkes.com
//All rights reserved.
//MIT licence.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockTech.Util;

namespace StockTech.Data
{
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

        public double LastClose { get; set; }


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


        public double OpenVLastClose
        {
            get
            {
                double percent = (Open - LastClose) / LastClose * 100;
                return percent;

            }
        }


        public double CloseVLastClose
        {
            get
            {
                double percent = (Close - LastClose) / LastClose * 100;
                return percent;

            }
        }

        public double HighVLastClose
        {
            get
            {
                double percent = (High - LastClose) / LastClose * 100;
                return percent;

            }
        }

        public double LowVLastClose
        {
            get
            {
                double percent = (Low - LastClose) / LastClose * 100;
                return percent;
            }
        }

    }

    /// <summary>
    /// 日线数据文件，读取日线数据到数组
    /// </summary>
    public class DayFile
    {
        private string symbol;

        protected DayFile(string symbol)
        {
            this.symbol = symbol;
            loadFromFile();
        }

        long itemCount = 0;
        double lastclose = 0;
        private void loadFromFile()
        {
            string path = PathConfig.DayFilePath(symbol);
            if (!File.Exists(path))
            {
                return;//
            }

            FileStream fstream = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fstream);
           
            itemCount = fstream.Length / 32;
            dates = new int[itemCount];
            opens = new double[itemCount];
            closes = new double[itemCount];
            highs = new double[itemCount];
            lows = new double[itemCount];
            vols = new double[itemCount];
            amounts = new double[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                int date = reader.ReadInt32();
                int open = reader.ReadInt32();
                int high = reader.ReadInt32();
                int low = reader.ReadInt32();
                int close = reader.ReadInt32();
                float amount = reader.ReadSingle();
                int volume = reader.ReadInt32();
                int reservation = reader.ReadInt32();

                if (i == 0)
                {
                    lowest = low;
                    highest = high;
                    lastclose = open;
                }
                else
                {
                    if (low < lowest)
                    {
                        lowest = low;
                    }
                    if (high > highest)
                    {
                        highest = high;
                    }

                }

                dates[i] = date;
                opens[i] = open;
                closes[i] = close;
                highs[i] = high;
                lows[i] = low;
                vols[i] = volume;
                amounts[i] = amount;

                lastclose = close;
            }

            fstream.Close();
            reader.Close();

            initMacdData();
            initKDJ(9);
        }

        double highest = 0;
        double lowest = 0;
        public double Highest
        {
            get
            {
                return highest;
            }
        }

        public double Lowest
        {
            get
            {
                return lowest;
            }
        }

        static Dictionary<string, DayFile> dict = new Dictionary<string, DayFile>();
        internal static DayFile get(string symbol)
        {
            DayFile f = null;
            if (dict.TryGetValue(symbol, out f))
            {
                return f;
            }
            f = new DayFile(symbol);
            dict.Add(symbol, f);
            return f;

        }

        public double[] emaFast = null;
        public double[] emaSlow = null;
        public double[] emaDiff = null;
        public double[] dea = null;
        public double[] macdDiff = null;
        private int[] dates=null;
        private double[] opens = null;
        private double[] closes = null;
        private double[] highs = null;
        private double[] lows = null;
        private double[] vols = null;
        private double[] amounts = null;

        public int[] Dates { get { return this.dates; } }
        public double[] Opens { get { return this.opens; } }
        public double[] Closes { get { return this.closes; } }
        public double[] Highs { get { return this.highs; } }
        public double[] Lows { get { return this.lows; } }
        public double[] Vols { get { return this.vols; } }
        public double[] Amounts { get { return this.amounts; } }

        //macd parameters.
        int macd0 = 12;
        int macd1 = 26;
        int macd2 = 9;
        void initMacdData()
        {
           
            emaFast = MathUtil.calcEMA(closes, macd0);
            emaSlow = MathUtil.calcEMA(closes, macd1);
            emaDiff = MathUtil.calcDiff(emaFast, emaSlow);
            dea = MathUtil.calcEMA(emaDiff, macd2);
            macdDiff = MathUtil.calcDiff(emaDiff, dea);


            emas.Add(macd0, emaFast);
            emas.Add(macd1, emaSlow);

        }

        Dictionary<int, double[]> emas = new Dictionary<int, double[]>();

        internal double[] getEMAs(int days)
        {
            if (emas.ContainsKey(days))
            {
                return emas[days];
            }

            double[] ema = MathUtil.calcEMA(closes, days);
            emas.Add(days, ema);
            return ema;
        }


        internal double getHighestVolume(int start, int end)
        {
            return MathUtil.getHighest(vols, start, end);
        }


        internal double getLowestVolume(int start, int end)
        {
            return MathUtil.getLowest(vols, start, end);
        }


        public double getHighest(int start, int end)
        {
            return MathUtil.getHighest(highs, start, end);
            
        }


        public double getLowest(int start, int end)
        {
            return MathUtil.getLowest(lows, start, end);
        }


        internal double getHighestMacdDiff(int start, int end)
        {
            return MathUtil.getHighest(macdDiff, start, end);
        }


        internal double getLowestMacdDiff(int start, int end)
        {
            return MathUtil.getLowest(macdDiff, start, end);
        }

        internal double getHighestEmaDiff(int start, int end)
        {
            return MathUtil.getHighest(emaDiff, start, end);
        }

        internal double getHighestDea(int start, int end)
        {
            return MathUtil.getHighest(dea, start, end);
        }

        internal double getLowestEmaDiff(int start, int end)
        {
            return MathUtil.getLowest(emaDiff, start, end);
        }

        internal double getLowestDea(int start, int end)
        {
            return MathUtil.getLowest(dea, start, end);
        }

        public long ItemCount
        {
            get
            {
                return this.itemCount;
            }
        }


        internal DayPrice getPrice(int i)
        {
            if (i < 0)
            {
                i = 0;
            }
            if (this.dates==null || this.dates.Length == 0)
            {
                return null;
            }
            DayPrice p = new DayPrice()
            {
                Symbol=this.symbol,
                Date=this.dates[i],
                Open=this.opens[i],
                Close=this.closes[i],
                High=this.highs[i],
                Low=this.lows[i],
                Volume=this.vols[i],
                Amount=this.amounts[i]
            };
            return p;
        }


        public DayPrice LastPrice
        {
            get
            {
                return getPrice((int)itemCount - 1);
            }
        }


         double[] kArray=null;
         double[] dArray = null;
         double[] jArray = null;

         public double[] K { get { return this.kArray; } }
         public double[] D { get { return this.dArray; } }
         public double[] J { get { return this.jArray; } }

         
        //SMA(RSV, 3 ,1);
         public void initKDJ(int days)
         {
             //double[] rsvArray = MathUtil.calcRSV(days, closes, highs, lows);
             MathUtil.calcKDJ(days, closes, highs, lows, out kArray, out dArray, out jArray);

         }



         internal double getHighestJ(int start, int end)
         {
             return MathUtil.getHighest(jArray, start, end);
         }

         internal double getLowestJ(int start, int end)
         {
             return MathUtil.getLowest(jArray, start, end);
         }
    }
}

