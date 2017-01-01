using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StockTech.Data;

namespace StockTech.Controls
{
    /// <summary>
    /// Interaction logic for SelecPanelIndust.xaml
    /// </summary>
    public partial class SelectPanelIndust : UserControl
    {
        public delegate void SymbolSelectedEventHandler(string symbol);
        public event SymbolSelectedEventHandler OnSymbolSelected;

        public SelectPanelIndust()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SelectPanelIndust_Loaded);

        }

        void SelectPanelIndust_Loaded(object sender, RoutedEventArgs e)
        {
           
            this.treeView.ItemsSource = IndustFile.Inst.Items;
            this.treeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(treeView_SelectedItemChanged);

        }

        void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            var item = (StockNode)this.treeView.SelectedItem;
            string symbol = MetaFile.Inst.getSymbolByName(item.Name);
            if (symbol == null)
            {
                return;
            }

            if (OnSymbolSelected != null)
            {
                OnSymbolSelected(symbol);
            }
        }

    }
}

