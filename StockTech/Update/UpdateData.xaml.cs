//可视化股票技术分析软件
//孙剑峰
//2016.04.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Net;
using StockTech.Controls;
using StockTech.Data;
using StockTech.Util;
using StockTech.Py;

using HtmlAgilityPack;

namespace StockTech.Update
{
    /// <summary>
    /// UpdateData.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateData : Window
    {
        public UpdateData()
        {
            int default_price = 23;
            long default_vol = 100000000, default_amount = 10000000000;         
            InitializeComponent();
            datepicker1.SelectedDate = DateTime.Now.AddDays(-2);
            datepicker2.SelectedDate = DateTime.Now.AddDays(-1);
            t1.Text = t2.Text = t3.Text = t4.Text = default_price.ToString();
            t5.Text = default_vol.ToString();
            t6.Text = default_amount.ToString();
        }
        public UpdateData(double x, double y)
        {
            InitializeComponent();
            /*
            //启用‘Manual’属性后，可以手动设置窗体的显示位置
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = x;
            this.Left = y;
            */
            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        
        double m_open = 0, m_close = 0, m_high = 0, m_low = 0;
        long m_vol = 0, m_amount = 0;
        int check_finish = 0;

        //test_offset
        int test_offset = 40000;
        //模式选择，0为联网更新，1为模拟数据
        int method = 0;

        Thread workThread = null;
        string datePath = "./Data/Db/date.txt";
        string symbolPath = "./Data/Db/symbols.txt";
        string dbPath = "./Data/Db/daydb";
        string dbdatapath = "./Data/Db/daydb.dt";

        List<string> symbols = new List<string>();
        public List<int> datesImported = new List<int>();

        string datetime1 = "";
        string datetime2 = "";
        int y1, m1, d1;
        int y2, m2, d2;
        DateTime dt;
        DateTime t_now;

        int k1 = 0;
        int k2 = 0;
        int all;
        int cur = 1;
        double x=0;

        //模拟值趋势预设
        int trend = 0;
        int trend_change = 0;
        int trend_index = 0;
        int range1 = 40; //浮动天数范围设置，横向天数,四分之一周期
        int range2 = 500; //变化幅度，纵向值
        //int range3 = 5; //变化趋势幅度，值越大幅度越小

        public struct DayPrice
        {
            public int open;
            public int close;
            public int high;
            public int low;
            public int vol;
            public float amount;
        }
        DayPrice items = new DayPrice();

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                method = 1;
            }
            else
            {
                method = 0;
            }
            workThread = new Thread(new ThreadStart(delegate { startImport(); }));
            workThread.Name = "work_thread";
            workThread.Start();
        }

        private void startImport()
        {
            loadSymbols();
            loadDates();
            if(init()==1)
            {
                if (check() == 1)
                {
                    update();
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        
        private void loadSymbols()
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                textCurrentProcess.Text = "导入符号表! ";
            });
            var f = File.Open(symbolPath, FileMode.OpenOrCreate);
            StreamReader r = new StreamReader(f);
            //从文件读取符号到List类型的symbols中
            while (!r.EndOfStream)
            {
                var line = r.ReadLine();
                symbols.Add(line);
            }
            r.Close();
            f.Close();
        }

        private void loadDates()
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                textCurrentProcess.Text = "导入已下载的日期表 ";
            });

            string p = datePath;

            var f = File.Open(p, FileMode.Open);
            StreamReader r = new StreamReader(f);
            while (!r.EndOfStream)
            {
                string line = r.ReadLine();
                int dt = 0;
                if (Int32.TryParse(line, out dt))
                {
                    datesImported.Add(dt);
                }

            }
            r.Close();
            f.Close();

        }

        public int init()
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                textCurrentProcess.Text = "系统校验";
                datetime1 = datepicker1.SelectedDate.ToString();
                datetime2 = datepicker2.SelectedDate.ToString() ;
                if (method == 1)
                {
                    m_open = Convert.ToDouble(t1.Text);
                    m_close = Convert.ToDouble(t2.Text);
                    m_high = Convert.ToDouble(t3.Text);
                    m_low = Convert.ToDouble(t4.Text);
                    m_vol = Convert.ToInt64(t5.Text);
                    m_amount = Convert.ToInt64(t6.Text);
                }
            });
            t_now = DateTime.Now;
            //检查网络状态
            if(method == 0)
            {
                System.Net.NetworkInformation.Ping ping;
                System.Net.NetworkInformation.PingReply res;
                ping = new System.Net.NetworkInformation.Ping();
                try
                {
                    res = ping.Send("hq.sinajs.cn");
                    if (res.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {               
                        MessageBox.Show("未连接到Internet");
                        return 0;
                    }
                }
                catch(Exception er)
                {
                    MessageBox.Show("网络断开");
                    return 0;
                }
            }
            return 1;
        }

        private int check()
        {
            if (datetime1.Equals(""))
            {
                if(datetime1.Equals("") && check_finish == 0)
                {
                    MessageBox.Show("检测完成");
                    check_finish = 1;
                    return 0;
                }
                MessageBox.Show("请输入开始时间");
                return 0;
            }
            if (datetime2.Equals(""))
            {
                MessageBox.Show("请输入结束时间");
                return 0;
            }
            try
            {
                string[] ss;
                ss = datetime1.Split('-');
                k1 = Convert.ToInt32(ss[0] + ss[1] + ss[2]);
                y1 = Convert.ToInt32(ss[0]);
                m1 = Convert.ToInt32(ss[1]);
                d1 = Convert.ToInt32(ss[2]);
                ss = datetime2.Split('-');
                k2 = Convert.ToInt32(ss[0] + ss[1] + ss[2]);
                y2 = Convert.ToInt32(ss[0]);
                m2 = Convert.ToInt32(ss[1]);
                d2 = Convert.ToInt32(ss[2]);
            }
            catch
            {
                MessageBox.Show("时间格式错误");
                return 0;
            }
            if (k2 < k1)
            {
                MessageBox.Show("开始时间不能晚于结束时间");
                return 0;
            }
            if (new DateTime(y2, m2, d2) >= DateTime.Now)
            {
                MessageBox.Show("结束时间不能晚于当前时间");
                return 0;
            }  
            if(FormatDatetime(new DateTime(y1, m1, d1)) < datesImported[datesImported.Count() - 1] + test_offset)
            {
                MessageBox.Show("已存在比开始时间更新的数据");
                return 0;
            }
            return 1;
        }
        private void update()
        {
            int processIndex = 0;
            int symCount = symbols.Count();
            int sec = 0;

            DateTime dtStart = new DateTime(y1, m1, d1);
            DateTime dtEnd = new DateTime(y2, m2, d2);
            TimeSpan ts = dtEnd.Subtract(dtStart);

            all = ts.Days * symCount;

            try
            {
                for (dt = new DateTime(y1, m1, d1); dt <= new DateTime(y2, m2, d2); dt = dt.AddDays(1))       
                {
                    this.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        textCurrentProcess.Text = "正在更新！";
                        progressBar.Maximum = ts.Days + 1;
                        progressBar.Value = processIndex;

                        buttonStart.IsEnabled = false;
                        datepicker1.IsEnabled = false;
                        datepicker2.IsEnabled = false;
                        checkBox.IsEnabled = false;
                        l1.IsEnabled = l2.IsEnabled = l3.IsEnabled = l4.IsEnabled = l5.IsEnabled = l6.IsEnabled = false;
                        t1.IsEnabled = t2.IsEnabled = t3.IsEnabled = t4.IsEnabled = t5.IsEnabled = t6.IsEnabled = false;
                    });
                    processIndex++;

                    ChangeTrend();

                    for (int i = 0; i < symCount; i++)
                    {
                        this.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            progressBar2.Maximum = symCount;
                            progressBar2.Value = i;

                            if (i < symCount)
                                tb_1.Text = "股票代码: " + symbols[i].ToString() + "      日期: " + dt.ToShortDateString();
                            sec = DateTime.Now.Subtract(t_now).Seconds + 60 * DateTime.Now.Subtract(t_now).Minutes + 3600 * DateTime.Now.Subtract(t_now).Hours + 3600 * 24 * DateTime.Now.Subtract(t_now).Days;
                            tb_2.Text = "第" + cur + "条 ， 共" + all + "条 , 已经完成 "+x.ToString("F2") + " % , 使用"+ sec + "."+ DateTime.Now.Subtract(t_now).Milliseconds + "秒";
                        });
                        cur = i + dt.Subtract(new DateTime(y1, m1, d1)).Days * symCount;
                        x = Convert.ToDouble(cur) / all *100;
                        
                        if(datesImported.IndexOf(FormatDatetime(dt))>=0)
                        {
                            continue;
                        }
                        if(method == 0)
                        {
                            getdate(FormatDatetime(dt), Convert.ToInt32(symbols[i].ToString().Substring(2, 6)), dt.Year, ConvertMonthToQuarter(dt.Month));
                        }
                        else
                        {
                            getdate_simulate(m_open, m_close, m_high, m_low, m_vol, m_amount);
                        }                   
                        wdata();
                    }
                }

                for (dt = new DateTime(y1, m1, d1); dt <= new DateTime(y2, m2, d2); dt = dt.AddDays(1))
                {
                    if (datesImported.IndexOf(FormatDatetime(dt)) >= 0)
                    {
                        continue;
                    }
                    wdate(FormatDatetime(dt)-test_offset);
                }
            }
            catch (WebException webEx)
            {

                Console.WriteLine(webEx.Message.ToString());
                MessageBox.Show(webEx.Message.ToString());

            }

            System.Windows.MessageBox.Show("更新完成! 耗时"+ sec +"秒\n请重启软件~~~");
        }


        /*
         * 传入数据类型 20160416、601006、2015、2
         * 
        */
        public void getdate(int date_now, int sh, int year, int quarter)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDoc = htmlWeb.Load("http://money.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/" + sh + ".phtml?year=" + year + "&jidu=" + quarter);
            HtmlNode rootNode = htmlDoc.DocumentNode;

            //定位到表
            rootNode = rootNode.SelectSingleNode("//html[1]/body[1]/div[1]/div[9]/div[2]/div[1]/div[3]/table[2]");

            int[] date_all = new int[93];
            int i_temp = 0;
            for (; ; i_temp++)
            {
                string s_temp = "";
                int j_temp = i_temp + 2;
                try
                {
                    s_temp = rootNode.SelectSingleNode("//tr[" + j_temp + "]/td[1]/div[1]/a[1]").InnerText;
                    s_temp = s_temp.Substring(5, 10);
                }
                catch
                {
                    //MessageBox.Show(i_temp.ToString());
                    break;
                }
                //string d_temp = DateTime.Parse(s_temp).ToString("yyyy-MM-dd");
                string[] ss = s_temp.Split('-');
                string t_temp = ss[0] + ss[1] + ss[2];

                date_all[i_temp] = Convert.ToInt32(t_temp);
            }

            int index = -1;
            for (int i = 0; i < i_temp; i++)
            {
                if (date_all[i] == date_now)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                items.open = 0;
                items.close = 0;
                items.high = 0;
                items.low = 0;
                items.vol = 0;
                items.amount = 0;
            }
            else
            {
                index += 2;
                items.open = Convert.ToInt32(Convert.ToSingle(rootNode.SelectSingleNode("//tr[" + index + "]/td[2]/div[1]").InnerText) * 10000);
                items.close = Convert.ToInt32(Convert.ToSingle(rootNode.SelectSingleNode("//tr[" + index + "]/td[4]/div[1]").InnerText) * 10000);
                items.high = Convert.ToInt32(Convert.ToSingle(rootNode.SelectSingleNode("//tr[" + index + "]/td[3]/div[1]").InnerText) * 10000);
                items.low = Convert.ToInt32(Convert.ToSingle(rootNode.SelectSingleNode("//tr[" + index + "]/td[5]/div[1]").InnerText) * 10000);
                items.vol = Convert.ToInt32(rootNode.SelectSingleNode("//tr[" + index + "]/td[6]/div[1]").InnerText);
                items.amount = Convert.ToInt64(rootNode.SelectSingleNode("//tr[" + index + "]/td[7]/div[1]").InnerText);
            }
        }

        /* 模拟数据开关
         * 
         * 
        */
        private void checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                l1.Visibility = Visibility.Visible;
                t1.Visibility = Visibility.Visible;
                l2.Visibility = Visibility.Visible;
                t2.Visibility = Visibility.Visible;
                l3.Visibility = Visibility.Visible;
                t3.Visibility = Visibility.Visible;
                l4.Visibility = Visibility.Visible;
                t4.Visibility = Visibility.Visible;
                l5.Visibility = Visibility.Visible;
                t5.Visibility = Visibility.Visible;
                l6.Visibility = Visibility.Visible;
                t6.Visibility = Visibility.Visible;
                method = 1;
            }
            else
            {
                l1.Visibility = Visibility.Hidden;
                t1.Visibility = Visibility.Hidden;
                l2.Visibility = Visibility.Hidden;
                t2.Visibility = Visibility.Hidden;
                l3.Visibility = Visibility.Hidden;
                t3.Visibility = Visibility.Hidden;
                l4.Visibility = Visibility.Hidden;
                t4.Visibility = Visibility.Hidden;
                l5.Visibility = Visibility.Hidden;
                t5.Visibility = Visibility.Hidden;
                l6.Visibility = Visibility.Hidden;
                t6.Visibility = Visibility.Hidden;
                method = 0;
            }
        }

        //private void checkBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (checkBox.IsChecked == true)
        //    {
        //        method = 1;
        //    }
        //    else
        //    {
        //        method = 0;
        //    }
        //}

        /* 模拟数据
         * 
         * 
        */
        public void getdate_simulate(double base_open, double base_close, double base_high, double base_low, long base_vol, long base_amount)
        {
            Random random = new Random();
            double num1 = random.NextDouble();
            double num2 = random.NextDouble();
            double num3 = random.NextDouble();
            double num4 = random.NextDouble();
            double num5 = random.NextDouble();
            double num6 = random.NextDouble();
            int num = random.Next()%100;
            int ch1 = random.Next();
            int ch2 = random.Next();
            int ch3 = random.Next();
            int ch4 = random.Next();

            //不使用随机数
            //items.open = Convert.ToInt32(base_open * 10000);
            //items.close = Convert.ToInt32(base_close * 10000);
            //items.high = Convert.ToInt32(base_high * 10000);
            //items.low = Convert.ToInt32(base_low * 10000);
            //items.vol = Convert.ToInt32(base_vol);
            //items.amount = Convert.ToInt64(base_amount);
            
            //启用随机数
            items.open = Convert.ToInt32((num1 + base_open) * 10000);
            items.close = Convert.ToInt32((num2 + base_close) * 10000);
            items.high = Convert.ToInt32((num3 + base_high) * 10000);
            items.low = Convert.ToInt32((num4 + base_low) * 10000);
            items.vol = Convert.ToInt32(num5 * 100000000 + base_vol);
            items.amount = Convert.ToInt64(num6 * 1000000000 + base_amount);

            if (ConvertCh(ch1) == 0)
            {
                items.open += num;
            }
            else
            {
                items.open -= num;
            }
            if (ConvertCh(ch2) == 0)
            {
                items.close += num;
            }
            else
            {
                items.close -= num;
            }
            if (ConvertCh(ch3) == 0)
            {
                items.high += num;
            }
            else
            {
                items.high -= num;
            }
            if (ConvertCh(ch4) == 0)
            {
                items.low += num;
            }
            else
            {
                items.low -= num;
            }
            if (items.open <= 0)
            {
                items.open = Math.Abs(items.open);
            }
            if (items.close <= 0)
            {
                items.close = Math.Abs(items.close);
            }
            if (items.high <= 0)
            {
                items.high = Math.Abs(items.high);
            }
            if (items.low <= 0)
            {
                items.low = Math.Abs(items.low);
            }
            if (items.high <= items.open || items.high <= items.close || items.high <= items.low)
            {
                items.high = Math.Max(items.open, items.close);
                items.high = Math.Max(items.high, items.low) + num;
            }
            if (items.low >= items.open || items.low >= items.close)
            {
                items.low = Math.Min(items.open, items.close);
            }
            items.open += trend;
            items.close += trend;
            items.high += trend;
            items.low += trend;
            items.vol += trend;
            items.amount += trend;
        }

        public void wdate(int date)
        {
            FileStream f = new FileStream(datePath, FileMode.Append);
            StreamWriter s = new StreamWriter(f);
            s.WriteLine(date);
            s.Close();
            f.Close();

            FileStream fs = new FileStream(dbdatapath, FileMode.Append);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(date);
            bw.Close();
            fs.Close();
        }
        public void wdata()
        {
            FileStream fs = new FileStream(dbPath, FileMode.Append);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(items.open);
            bw.Write(items.close);
            bw.Write(items.high);
            bw.Write(items.low);
            bw.Write(items.vol);
            bw.Write(items.amount);
            bw.Close();
            fs.Close();
        }

        //根据月份确定季度
        public int ConvertMonthToQuarter(int month)
        {
            return month / 3 + (month % 3 > 0 ? 1 : 0);
        }
        //由随机数的奇偶性确定正负号
        public int ConvertCh(int ch)
        {
            if(ch % 2 == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public int FormatDatetime(DateTime datetime)
        {
            string temp_y, temp_m, temp_d;
            temp_y = datetime.Year.ToString();
            temp_m = datetime.Month.ToString();
            temp_d = datetime.Day.ToString();
            if (int.Parse(temp_m) < 10)
                temp_m = "0" + temp_m;
            if (int.Parse(temp_d) < 10)
                temp_d = "0" + temp_d;
            string s = temp_y + temp_m + temp_d;
            return Convert.ToInt32(s);
        }
        //模拟趋势变化
        public void ChangeTrend()
        {
            trend_index++;
            Random random = new Random();
            if (trend_index % range1 == 0)
            {
                trend_change = 1 - trend_change;
            }
            if (trend_change == 0)
            {
                trend += range2 + random.Next() % 500;
            }
            else
            {
                trend -= range2 + random.Next() % 500;
            }
        }
    }
}
