//可视化股票技术分析软件
using System;
using System.Collections.Generic;
using System.IO;


namespace StockTech.Util
{
    /// <summary>
    /// 股票
    /// </summary>
    public class StockUtil
    {

        internal static int convertSymbolToInt(string symbol)
        {
            int ret = Int32.Parse(symbol.Substring(2));
            return ret;

        }

        protected static List<string> symbolsList = null;
        public static List<string> getAllSymbols()
        {
            if (symbolsList != null)
            {
                return symbolsList;
            }

            string[] fileNames = Directory.GetFiles(PathConfig.DayFileRootPath);
            symbolsList = new List<string>();
            foreach (var name in fileNames)
            {
                string symbol = PathConfig.getSymbolFromDayFilePath(name);
                symbolsList.Add(symbol);
            }

            return symbolsList;

        }

       
        public static bool isIndexStock(string symbol)
        {
            string ex = symbol.Substring(0, 2);
            if (ex == "sh")
            {
                return symbol[2] != '6';
            }
            else
            {
                string begin = symbol.Substring(0, 5);
                if (begin == "sz399")
                {
                    return true;
                }

                if (begin == "sz395")
                {
                    return true;
                }

            }
            return false;
        }


        public static bool isBondStock(string symbol)
        {
            string begin = symbol.Substring(0, 5);

            if (begin == "sz395")
            {
                return true;
            }

            return false;
        }


        //该符号代表的是一家公司.
        public static bool isCompanyStock(string symbol)
        {
            return !isIndexStock(symbol);
        }
  

    }
}
