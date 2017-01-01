//可视化股票技术分析软件

using System.Windows.Controls;
using StockTech.Data;
using StockTech.Util;

namespace StockTech.Controls
{
    /// <summary>
    /// 显示技术图表鼠标最近的K线信息
    /// </summary>
    public partial class PriceInfoBox : UserControl
    {
        public PriceInfoBox()
        {
            InitializeComponent();
        }
       
        internal void setPrice(string symbol, DayPrice price)
        {
            if (price == null)
            {
                return;
            }

            string txt =  symbol;
            txt += MetaFile.Inst.getName(symbol) + "  ";
            txt += CommonUtil.formatDate(price.Date) + "  ";
            txt += CommonUtil.formatDayOfWeek(price.Date) + "  ";
            txt += "开" + (price.Open / 100.0) + "  ";
            txt += "收" + (price.Close / 100.0) + "  ";
            txt += "高" + (price.High / 100.0) + "  ";
            txt += "低:" + (price.Low / 100.0) + "  ";
            txt += "涨" + CommonUtil.formatPricePercent(((price.Close - price.Open) / price.Open)) + "  ";

            txtDayInfo.Text = txt;
        }

    }
}
