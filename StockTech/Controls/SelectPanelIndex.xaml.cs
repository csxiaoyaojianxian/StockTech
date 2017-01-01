//可视化股票技术分析软件

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using StockTech.Data;

namespace StockTech.Controls
{
    /// <summary>
    /// Interaction logic for SelectPanelIndex.xaml
    /// </summary>
    public partial class SelectPanelIndex : UserControl
    {
        public SelectPanelIndex()
        {
            InitializeComponent();
            loadAllStocks();
        }

        public delegate void SymbolSelectedEventHandler(string symbol);
        public event SymbolSelectedEventHandler OnSymbolSelected;

        List<IndexItem> items = null;
        private void loadAllStocks()
        {
            items = IndexFile.Inst.Items;
            this.listView.ItemsSource = items;

            this.txtItemTotal.Text = "共有股票条数:" + items.Count;

            this.listView.SelectionChanged += new SelectionChangedEventHandler(listView_SelectionChanged);

        }

        void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IndexItem item = (IndexItem)this.listView.SelectedItem;
            if (item !=null)
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
