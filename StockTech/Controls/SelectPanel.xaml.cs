//可视化股票技术分析软件
using System.Windows;
using System.Windows.Controls;

namespace StockTech.Controls
{
    /// <summary>
    /// Interaction logic for SelectPannel.xaml
    /// </summary>
    public partial class SelectPannel : UserControl
    {
        public SelectPannel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SelectPannel_Loaded);
        }

        void SelectPannel_Loaded(object sender, RoutedEventArgs e)
        {
            showPanelUser();
            this.panelIndex.OnSymbolSelected += new SelectPanelIndex.SymbolSelectedEventHandler(onSymbolSelected);
            this.panelAll.OnSymbolSelected += new SelectPanelAll.SymbolSelectedEventHandler(onSymbolSelected);
            this.panelIndust.OnSymbolSelected += new SelectPanelIndust.SymbolSelectedEventHandler(onSymbolSelected);
            this.panelUser.OnSymbolSelected += new SelectPanelUser.SymbolSelectedEventHandler(onSymbolSelected);
        }

        public delegate void SymbolSelectedEventHandler(string symbol);
        public event SymbolSelectedEventHandler OnSymbolSelected;


        void onSymbolSelected(string symbol)
        {
            if (OnSymbolSelected != null)
            {
                OnSymbolSelected(symbol);
            }
        }

        private void selectUser_Click(object sender, RoutedEventArgs e)
        {
            showPanelUser();
        }

        //
        private void selectPanelAll_Click(object sender, RoutedEventArgs e)
        {
            showPanelAll();
        }

        private void showPanelUser()
        {
            hideAll();
            panelUser.Visibility = Visibility.Visible;
        }


        private void showPanelAll()
        {
            hideAll();
            panelAll.Visibility = Visibility.Visible;
        }

        private void hideAll()
        {
            panelAll.Visibility = Visibility.Hidden;
            panelIndex.Visibility = Visibility.Hidden;
            panelIndust.Visibility = Visibility.Hidden;
            panelUser.Visibility = Visibility.Hidden;
        }

        private void selectIndust_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            panelIndust.Visibility = Visibility.Visible;
        }

        private void selectIndex_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            panelIndex.Visibility = Visibility.Visible;
        }

    }
}
