using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using StockTech.Data;

namespace StockTech.Controls
{
    /// <summary>
    /// Interaction logic for SelectPanelUser.xaml
    /// </summary>
    public partial class SelectPanelUser : UserControl
    {
       
         public delegate void SymbolSelectedEventHandler(string symbol);
        public event SymbolSelectedEventHandler OnSymbolSelected;

        public SelectPanelUser()
        {
            InitializeComponent();

            loadPortfolio();
            this.listView.SelectionChanged += new SelectionChangedEventHandler(listView_SelectionChanged);
           
        }

      

        PortfolioFile f = null;
        private void loadPortfolio()
        {
            f = PortfolioFile.Inst;
            f.OnItemAdded += new PortfolioFile.ItemHander(f_OnItemAdded);
            f.OnItemRemoved += new PortfolioFile.ItemHander(f_OnItemRemoved);
            this.listView.ItemsSource = f.PortfolioList;

            this.txtItemTotal.Text = "共有股票条数:" + f.PortfolioList.Count;

        }

        void f_OnItemRemoved(PortfolioItem item)
        {
            this.listView.ItemsSource = null;
            this.listView.ItemsSource = f.PortfolioList;
            this.txtItemTotal.Text = "共有股票条数:" + f.PortfolioList.Count;
        }

        void f_OnItemAdded(PortfolioItem item)
        {
            this.listView.ItemsSource = null;
            this.listView.ItemsSource = f.PortfolioList;
            this.txtItemTotal.Text = "共有股票条数:" + f.PortfolioList.Count;

        }

         void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var item = (PortfolioItem)this.listView.SelectedItem;
            if (item!=null)
            {
                string symbol = item.Symbol;
                if (OnSymbolSelected != null)
                {
                    OnSymbolSelected(symbol);
                }
            }

        }

        private void listView_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                GridViewColumn clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                if (clickedColumn != null)
                {
                    //Get binding property of clicked column
                    string bindingProperty = (clickedColumn.DisplayMemberBinding as Binding).Path.Path;
                    SortDescriptionCollection sdc = this.listView.Items.SortDescriptions;
                    ListSortDirection sortDirection = ListSortDirection.Ascending;
                    if (sdc.Count > 0)
                    {
                        SortDescription sd = sdc[0];
                        sortDirection = (ListSortDirection)((((int)sd.Direction) + 1) % 2);
                        sdc.Clear();
                    }
                    sdc.Add(new SortDescription(bindingProperty, sortDirection));
                }
            }
        }


    }
}
