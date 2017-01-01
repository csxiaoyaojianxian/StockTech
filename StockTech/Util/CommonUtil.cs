//可视化股票技术分析软件

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StockTech.Util
{
    public class CommonUtil
    {

        public static int getDateInt()
        {
            return DateTime.Now.Year * 100 * 100 + DateTime.Now.Month * 100 + DateTime.Now.Day;
        }

        public static int getDateInt(DateTime dt)
        {
            return dt.Year * 100 * 100 + dt.Month * 100 + dt.Day;
        }


        static public DateTime getNearWorkDate()
        {
            DateTime dt = DateTime.Now;
            while (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            {
                dt = dt.AddDays(-1);
            }

            return dt;

        }


        public static string formatDate(int date)
        {
            int year = date / 10000;
            int month = (date - year * 10000) / 100;
            int day = date - year * 10000 - month * 100;

            string monthText = month.ToString();
            if (month < 10)
            {
                monthText = '0' + monthText;
            }

            string dayText = day.ToString();
            if (day < 10)
            {
                dayText = '0' + dayText;
            }

            return year + "/" + monthText + "/" + dayText;
        }

        public static string formatDayOfWeek(int date)
        {
            int year = date / 10000;
            int month = (date - year * 10000) / 100;
            int day = date - year * 10000 - month * 100;
            DateTime dt = new DateTime(year, month, day);
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "星期一";
                case DayOfWeek.Tuesday:
                    return "星期二";
                case DayOfWeek.Wednesday:
                    return "星期三";
                case DayOfWeek.Thursday:
                    return "星期四";
                case DayOfWeek.Friday:
                    return "星期五";
                case DayOfWeek.Saturday:
                    return "星期六";
                case DayOfWeek.Sunday:
                    return "星期日";
            }
            return null;
        }

        public static string formatPrice(double price)
        {
            int priceInt = (int)price;
            double priceFormated = priceInt / 100.0;
            return priceFormated.ToString();
        }

        public static string formatPricePercent(double percent)
        {
            int percentInt = (int)(percent * 10000);
            double formated = percentInt / 100.0;
            return formated.ToString() + "%";


        }


        public static int getDateInt(string dateStr)
        {
            //2011-06-07
            string[] strs = dateStr.Split('-');
            if (strs.Length >= 3)
            {
                int year = Int32.Parse(strs[0]);
                int month = Int32.Parse(strs[1]);
                int day = Int32.Parse(strs[2]);
                return year * 100 * 100 + month * 100 + day;
            }

            return 0;

        }

        //
        public static int getStructType(string txt)
        {
            if (txt.Equals("送、转股"))
            {
                return 0;
            }
            else if (txt.Equals("增发"))
            {
                return 1;
            }
            else if (txt.Equals("其它上市"))
            {
                return 2;
            }//定期报告
            else if (txt.Equals("定期报告"))
            {
                return 3;
            }//股权分置
            else if (txt.Equals("股权分置"))
            {
                return 4;
            }//股份性质变动
            else if (txt.Equals("股份性质变动"))
            {
                return 5;
            }//发行前股本
            else if (txt.Equals("发行前股本"))
            {
                return 6;
            }//IPO
            else if (txt.Equals("IPO"))
            {
                return 7;
            }//配股
            else if (txt.Equals("配股"))
            {
                return 8;
            }//权证行权
            else if (txt.Equals("权证行权"))
            {
                return 9;
            }//债转股
            else if (txt.Equals("债转股"))
            {
                return 10;
            }// 历史遗留
            else if (txt.Equals(" 历史遗留") || txt.Equals("历史遗留"))
            {
                return 11;
            }//超额配售
            else if (txt.Equals("超额配售"))
            {
                return 12;
            }//吸收合并
            else if (txt.Equals("吸收合并"))
            {
                return 13;
            }//回购
            else if (txt.Equals("回购"))
            {
                return 14;
            }//股改追送
            else if (txt.Equals("股改追送"))
            {
                return 15;
            }//资产重组
            else if (txt.Equals("资产重组"))
            {
                return 16;
            }//股权激励
            else if (txt.Equals("股权激励"))
            {
                return 17;
            }//公司成立
            else if (txt.Equals("公司成立"))
            {
                return 18;
            }//拆细
            else if (txt.Equals("拆细"))
            {
                return 19;
            }//拆细
            else if (txt.Equals("非经营资产剥离"))
            {
                return 20;

            }
            else
            {
                throw new NotImplementedException();
            }

            // return -1;

        }


        public static string getStructType(int type)
        {
            switch (type)
            {
                case 0:
                    return "转送";
                case 1:
                    return "增发";
                case 2:
                    return "其它上市";
                case 3:
                    return "定期报告";
                case 4:
                    return "股权分置";
                case 5:
                    return "股份性质变动";
                case 6:
                    return "发行前股本";
                case 7:
                    return "IPO";
                case 8:
                    return "配股";
                case 9:
                    return "权证行权";
                case 10:
                    return "债转股";
                case 11:
                    return "历史遗留";
                case 12:
                    return "超额配售";
                case 13:
                    return "吸收合并";
                case 14:
                    return "回购";
                case 15:
                    return "股改追送";
                case 16:
                    return "资产重组";
                case 17:
                    return "股权激励";
                case 18:
                    return "公司成立";
                case 19:
                    return "拆细";
                case 20:
                    return "非经营资产剥离";
                default:
                    return "其它";
            }

        }



        public static string formatPriceBy100Million(double value)
        {
            int capitalInt = (int)(value / 1000000);
            double capital100Million = (double)capitalInt / 100.0;
            return capital100Million + "亿";
        }

        public static double get100Million(double value)
        {
            int capitalInt = (int)(value / 1000000);
            double capital100Million = (double)capitalInt / 100.0;
            return capital100Million;
        }

        public static double get1Million(double value)
        {
            int capitalInt = (int)(value / 10000);
            double capital100Million = (double)capitalInt / 100.0;
            return capital100Million;
        }

        public static string formatPriceBy10Thousand(double value)
        {
            int capitalInt = (int)(value / 100);
            double capital100Million = (double)capitalInt / 100.0;
            return capital100Million + "万";
        }

        public static string formatPriceByThousand(double value)
        {
            int capitalInt = (int)(value / 10);
            double capital100Million = (double)capitalInt / 100.0;
            return capital100Million + "千";
        }


        public static string formatDoubleCN(double value)
        {
            if (value > 10000000000.0)
            {
                return formatPriceBy100Million(value);
            }

            if (value > 1000000.0)
            {
                return formatPriceBy10Thousand(value);
            }

            if (value > 100000.0)
            {
                return formatPriceByThousand(value);
            }

            return value.ToString();
        }


        public static int getDateInt(string p, out bool success)
        {
            int val = 0;
            success = int.TryParse(p, out val);
            if (success)
            {
                if (p.Length != "20010101".Length)
                {
                    success = false;
                }
                return val;
            }

            string[] items = p.Split('-');
            if (items.Length >= 3)
            {
                success = int.TryParse(items[0] + items[1] + items[2], out val);
                return val;
            }

            items = p.Split('/');
            if (items.Length >= 3)
            {
                success = int.TryParse(items[0] + items[1] + items[2], out val);
                return val;
            }


            success = false;
            return val;
        }

        public static double getDouble(string p)
        {
            string[] numtxts = p.Trim().Split(',');
            string numb = "";
            foreach (var str in numtxts)
            {
                numb += str;
            }
            double ret = 0;
            bool successed = double.TryParse(numb, out ret);
            if (!successed)
            {
            }
            return ret;

        }


        internal static DateTime getDate(int date)
        {
            int year = date / 10000;
            int month = (date - year * 10000) / 100;
            int day = date - year * 10000 - month * 100;

            DateTime dt = new DateTime(year, month, day);
            return dt;
        }


        internal static int getDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;

            }

            throw new NotImplementedException();

        }
    }
}
