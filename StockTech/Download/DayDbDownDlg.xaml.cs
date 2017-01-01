using System;
using System.Windows;
using StockTech.Util;

namespace StockTech.Download
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DayDbDownDlg : Window
    {
        
        public DayDbDownDlg()
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


        //
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            DayDbDown.Inst.OnDownFinish += new DayDbDown.DownFinishHandler(Inst_OnDownFinish);
            DayDbDown.Inst.OnNoNewData += new DayDbDown.NoNewDataHandler(Inst_OnNoNewData);

            DayDbDown.Inst.startDown();

        }

        void Inst_OnNoNewData()
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.Close();
                MessageBox.Show("暂无新数据下载");
            });

        }

        void Inst_OnDownFinish()
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentDown.Text = "下载完成! ";
                this.Close();
                MessageBox.Show("下载完成");
            });
        }

      
      
        void updateGUI(int date,int val,int max)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentDown.Text = "正在下载: " + CommonUtil.formatDate(date) + " 的日线";
                progressBar.Maximum = val;
                progressBar.Value = max;
            });

        }

      
    }
}

