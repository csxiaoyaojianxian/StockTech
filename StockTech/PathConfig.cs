//可视化股票技术分析软件

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockTech
{
    /// <summary>
    /// 数据路径配置 set in python script?
    /// </summary>
    class PathConfig
    {
        internal static string DayFilePath(string symbol)
        {
            string path = "./Data/Day/" + symbol + ".day";
            return path;
        }


        static public string getSymbolFromDayFilePath(string path)
        {
            string symbol = path.Substring(path.Length - 12, 8);
            return symbol;
        }

        

        internal static string DayFileRootPath
        {
            get
            {
                string path = "Data/Day/";
                return path;
            }
        }


        internal static string metaFilePath()
        {
            string path = "Data/stock.meta.txt";
            return path;
        }

        public static string IndustFilePath
        {
            get
            {
                string path = "Data/stock.indust.txt";
                return path;
            }
        }

        public static string PortfolioPath
        {
            get
            {
                string path = "Data/stock.portfolio";
                return path;
            }
        }

    
        public static string IndexFilePath
        {

            get
            {
                string path = "Data/stock.index.txt";
                return path;
            }
        }

      

    }
}

