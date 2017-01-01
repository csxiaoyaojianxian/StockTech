//可视化股票技术分析软件

using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StockTech.Data;

namespace StockTech.Controls
{

    class StockAutoComplete : AutoComplete
    {

        public StockAutoComplete()
        {
            this.SnapsToDevicePixels = true;
            this.Height = 22;
            this.MaxDropDownHeight = 400;
            this.Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));
            this.Loaded += new System.Windows.RoutedEventHandler(StockAutoComplete_Loaded);

        }

        void StockAutoComplete_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PatternChanged += new AutoCompleteHandler(StockAutoComplete_PatternChanged);
            this.SelectionChanged += new SelectionChangedEventHandler(StockAutoComplete_SelectionChanged);
            this.DisplayMemberPath = "SearchText";
            this.SelectedValuePath = "Symbol";
            this.Delay = 200;
            this.DropDownClosed += new EventHandler(StockAutoComplete_DropDownClosed);
            this.MouseLeave += new MouseEventHandler(StockAutoComplete_MouseLeave);
        }

        void StockAutoComplete_MouseLeave(object sender, MouseEventArgs e)
        {
            this.IsDropDownOpen = false;
        }

        void StockAutoComplete_DropDownClosed(object sender, EventArgs e)
        {
            if (this.SelectedItem != null)
            {
                var item = (MetaItem)this.SelectedItem;
                string symbol = item.Symbol;
                if (this.OnSymbolSelected != null)
                {
                    this.OnSymbolSelected(symbol);
                }
                this.Text = "";
            }
        }

        void StockAutoComplete_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public delegate void SymbolSelectedEventHandler(string symbol);
        public event SymbolSelectedEventHandler OnSymbolSelected;

        void StockAutoComplete_PatternChanged(object sender, AutoComplete.AutoCompleteArgs args)
        {
            // string txt = this.Text;
            string pattern = args.Pattern;
            if (string.IsNullOrEmpty(args.Pattern))
            {
                args.CancelBinding = true;
            }
            else
            {
                args.DataSource = getSymbols(args.Pattern);
            }

        }

        private IEnumerable getSymbols(string pattern)
        {
            return MetaFile.Inst.getSymbolByPattern(pattern).Take(32);
        }

    }
}

