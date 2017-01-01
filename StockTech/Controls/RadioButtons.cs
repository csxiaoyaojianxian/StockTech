using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace StockTech.Controls
{
    class RadioButtons : Grid
    {
         List<RadioButton> buttons=new List<RadioButton>();

        string name=null;
        public RadioButtons(string name){
            this.name=name;
        }

        //
        public void addButton(object content)
        {
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new System.Windows.GridLength(68),
            });
            var b = new RadioButton()
            {
                Content = content,
                GroupName = name,
            };
            b.Checked += new System.Windows.RoutedEventHandler(b_Checked);
            Grid.SetColumn(b, buttons.Count);
            buttons.Add(b);
            if (buttons.Count == 1)
            {
                b.IsChecked = true;
            }
            this.Children.Add(b);
        }

        void b_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (OnChecked!=null)
            {
                OnChecked((string)((RadioButton)sender).Content);
            }

        }

        public delegate void CheckedHandler(string name);
        public event CheckedHandler OnChecked;


        internal string getCheckedText()
        {
            foreach (var i in buttons)
            {
                if (i.IsChecked==true)
                {
                    return (string)i.Content;
                }

            }
            return null;
        }
    }
}
