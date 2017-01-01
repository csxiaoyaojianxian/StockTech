
//可视化股票技术分析软件

using System;
using System.Windows;
using System.Windows.Media;
using StockTech.Controls;
using StockTech.Data;
using StockTech.Util;
using StockTech.Py;
using StockTech.Update;
using System.IO;

namespace StockTech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainView view = new MainView();
        RadioButtons radioButtons = new RadioButtons("tech");

        public MainWindow()
        {

            InitializeComponent();

            this.radioContainer.Children.Add(radioButtons);
            
            this.Background = new SolidColorBrush(Color.FromRgb(246, 255, 255));
            this.gridContainer.Children.Add(view);
            this.stockAutoComplete.OnSymbolSelected += new StockAutoComplete.SymbolSelectedEventHandler(OnSymbolSelected);
            this.selectPanel.OnSymbolSelected += new SelectPannel.SymbolSelectedEventHandler(OnSymbolSelected);
            this.view.OnPriceChanged += new MainView.PriceChangedEventHandler(view_OnPriceChanged);

            OnSymbolSelected(symbol);

            view.setType(1);

            addRadionButtons();
            radioButtons.OnChecked += new RadioButtons.CheckedHandler(radioButtons_OnChecked);
        }

        void radioButtons_OnChecked(string name)
        {
            PyEngine.Inst.onTechClick(name);
        }

        private void addRadionButtons()
        {
            var list = PyEngine.Inst.getTechList();
            if (list == null)
            {
                return;
            }

            foreach (var i in list)
            {
                radioButtons.addButton(i);
            }

        }

        void view_OnPriceChanged(DayPrice price)
        {
            this.priceBox.setPrice(symbol,price);
        }

        string symbol = "sh000001";
        void OnSymbolSelected(string symbol)
        {
            //this.txtName.Text = MetaFile.Inst.getName(symbol);
            this.symbol = symbol;
            //symbol changed.

            this.view.load(symbol);


            this.view.reload(this.radioButtons.getCheckedText());
            this.priceBox.setPrice(symbol, this.view.LastPrice);
            if (PortfolioFile.Inst.IsStockInPortfolio(this.symbol))
            {
                this.buttonAdd.Content = "-";
            }
            else
            {
                this.buttonAdd.Content = "+";

            }

        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            bool sucessed = false;
            if ((string)this.buttonAdd.Content == "+")
            {
                PortfolioFile.Inst.add(this.symbol, out sucessed);

                if (sucessed)
                {
                    this.buttonAdd.Content = "-";

                }
            }
            else
            {
                PortfolioFile.Inst.remove(this.symbol, out sucessed);
                if (sucessed)
                {
                    this.buttonAdd.Content = "+";

                }
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateData ud = new UpdateData();

            
            string datePath = "./Data/Db/date.txt";
            string symbolPath = "./Data/Db/symbols.txt";
            string dbPath = "./Data/Db/daydb";
            string dbdatapath = "./Data/Db/daydb.dt";

            if (!File.Exists(symbolPath))
            {
                MessageBox.Show("初始化数据不完整");
                ud.buttonStart.IsEnabled = false;
                return;
            }

            if (!Directory.Exists("./Data"))
            {
                Directory.CreateDirectory("./Data");
            }

            if (!File.Exists(datePath))
            {
                File.Create(datePath);
            }
            if (!File.Exists(dbPath))
            {
                File.Create(dbPath);
            }
            if (!File.Exists(dbdatapath))
            {
                File.Create(dbdatapath);
            }
            ud.textCurrentProcess.Text = "初始化完成";

            ud.ShowDialog();
        }
    }
}
