using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StockTech.Data
{
    public class PortfolioItem
    {
        //用户记录
        public string Symbol { get; set; }
        public string Name
        {
            get
            {
                string name = MetaFile.Inst.getName(Symbol);
                return name;
            }
        }



        public int BuyPrice { get; set; }
        public int Amount { get; set; }


    }

    public class PortfolioFile
    {
        string fileName = null;

        static PortfolioFile inst = null;
        public static PortfolioFile Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new PortfolioFile(PathConfig.PortfolioPath);
                }

                return inst;
            }
        }

        protected PortfolioFile(string fileName)
        {
            this.fileName = fileName;
            loadFromFile(fileName);
        }

        private void loadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            FileStream fstream = File.OpenRead(fileName);
            BinaryReader reader = new BinaryReader(fstream);

            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                string symbol = reader.ReadString();
                int stockCount = reader.ReadInt32();
                int buyPrice = reader.ReadInt32();

                PortfolioItem item = new PortfolioItem()
                {
                    Symbol = symbol,
                    Amount = stockCount,
                    BuyPrice = buyPrice,
                };

                list.Add(item);

            }

            reader.Close();
            fstream.Close();

        }

        public PortfolioItem add(string symbol, out bool successed)
        {

            return add(symbol, 0, 0, out successed);
        }


        //price is *100
        internal PortfolioItem add(string symbol, int price, int stockCount, out bool successed)
        {
            bool hasSymbol = list.Exists(p => p.Symbol == symbol);
            if (hasSymbol)
            {
                successed = false;
                return list.Find(p => p.Symbol == symbol);
            }
            successed = true;

            PortfolioItem item = new PortfolioItem()
            {
                Symbol = symbol,
                Amount = stockCount,
                BuyPrice = price,
            };

            list.Add(item);

            saveToFile();

            if (OnItemAdded != null)
            {
                OnItemAdded(item);
            }

            return item;
        }

        public delegate void ItemHander(PortfolioItem item);
        public event ItemHander OnItemAdded = null;
        public event ItemHander OnItemRemoved = null;

        private void saveToFile()
        {
            FileStream fstream = File.Create(fileName);
            BinaryWriter writer = new BinaryWriter(fstream);

            writer.Write(list.Count);
            foreach (var i in list)
            {
                writer.Write(i.Symbol);
                writer.Write(i.Amount);
                writer.Write(i.BuyPrice);
            }

            writer.Flush();
            writer.Close();
            fstream.Close();

        }

        List<PortfolioItem> list = new List<PortfolioItem>();
        public List<PortfolioItem> PortfolioList
        {
            get
            {
                return list;
            }
        }

        public bool IsStockInPortfolio(string symbol)
        {
            bool inPortfolio = list.Exists(p => p.Symbol == symbol);
            return inPortfolio;
        }

        public void remove(string symbol, out bool successed)
        {
            successed = list.Exists(p => p.Symbol == symbol);
            if (successed)
            {
                var i = list.Find(p => p.Symbol == symbol);
                list.Remove(i);

                saveToFile();


                if (OnItemRemoved != null)
                {
                    OnItemRemoved(i);
                }

            }
        }
    }
}


