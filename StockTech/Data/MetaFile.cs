//可视化股票技术分析软件

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockTech.Data
{
    /// <summary>
    /// 股票名称，代码，拼音等...
    /// </summary>
    public class MetaItem
    {
        public string SearchText
        {
            get
            {
                return searchText;
            }
        }


        string symbol;
        string name;
        string shortName = "";
        string searchText = "";

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

        public string ShortName
        {
            get
            {
                return shortName;
            }
        }
       

        public MetaItem(string symbol, string name, string shortName)
        {

            this.symbol = symbol;
            this.name = name;
            this.shortName = shortName;
            this.searchText = symbol + " " + name + " " + shortName;
        }

    }

    /* 股票名称 代码 拼音等...
     * */
    public class MetaFile
    {
        List<MetaItem> items = new List<MetaItem>();

        public List<MetaItem> Items
        {
            get
            {
                return this.items;
            }
        }

       
        protected static MetaFile inst = null;
        public static MetaFile Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new MetaFile();
                }
                return inst;
            }
        }

        protected MetaFile()
        {
           
            string path = PathConfig.metaFilePath();
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
                    items.Add(new MetaItem(fields[0], fields[1], fields[2]));
                }
                else if (fields.Length == 1)
                {
                    items.Add(new MetaItem(fields[0], "", ""));
                }
                else if (fields.Length == 2)
                {
                    items.Add(new MetaItem(fields[0], fields[1], ""));
                }


            }

            reader.Close();
        }

       

        //default get the chinese name
        public string getName(string symbol)
        {
            //
            if (symbol == null)
            {
                return null;
            }


            foreach (MetaItem item in items)
            {
                if (item.Symbol == symbol)
                {
                    return item.Name;
                }

            }

            return null;

        }

        public bool hasSymbol(string symbol)
        {
            if (symbol == null || symbol.Length < 8)
            {
                return false;
            }
            //
            string sym = symbol.Substring(2);

            foreach (MetaItem item in items)
            {
                if (item.Symbol == sym)
                {
                    return true;
                }

            }

            return false;

        }


        public List<MetaItem> getSymbolByPattern(string pattern)
        {
            var items = this.items.Where((i) => i.SearchText.Contains(pattern)).ToList();
            return items;
        }


        internal string getSymbolByName(string name)
        {
            var items = this.items.Where(p => p.Name == name);
            if (items.Count() > 0)
            {
                var item = items.First();
                string symbol = item.Symbol;
                return symbol;
            }
            return null;
        }

    }
}
