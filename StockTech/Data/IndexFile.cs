//可视化股票技术分析软件

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StockTech.Data
{

    public class IndexItem
    {
        string symbol;
        string name;

        public string Symbol
        {
            get
            {
                return symbol;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }


        public IndexItem(string symbol, string name)
        {
            this.symbol = symbol;
            this.name = name;
        }

    }

    public class IndexFile
    {
        // string exchange;//sh sz
        List<IndexItem> items = new List<IndexItem>();

        public List<IndexItem> Items
        {
            get
            {
                return this.items;
            }
        }


        protected static IndexFile inst = null;
        public static IndexFile Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new IndexFile();
                }
                return inst;
            }
        }

        protected IndexFile()
        {

            string path = PathConfig.IndexFilePath;
            //stock.index.txt
            StreamReader reader = File.OpenText(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] fields = line.Split('\t');

                if (fields.Length < 1)
                {
                    continue;
                }
                if (fields[0] == "")
                {
                    continue;
                }

                if (fields.Length >= 3)
                {
                    items.Add(new IndexItem(fields[0], fields[1]));
                }

                else if (fields.Length == 2)
                {
                    items.Add(new IndexItem(fields[0], fields[1]));
                }


            }

            reader.Close();
        }

    }

}

