using System.Collections.Generic;
using System.Windows.Media;
using StockTech.Util;
using System.Windows;
using System.Windows.Controls;
using StockTech.Download;
using StockTech.Data;

namespace StockTech.Py
{
    public class StockEngine
    {
        static StockEngine inst = null;
        public static StockEngine Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new StockEngine();
                }
                return inst;
            }
        }

        public StockEngine()
        {
            menu.OnMenuItemClicked += new StockContextMenu.MenuItemClickedHandler(menu_OnMenuItemClicked);
        }

        void menu_OnMenuItemClicked(string txt)
        {
            PyEngine.Inst.contextMenuClick(txt);
        }

        public void showDownloadBox()
        {

            Window win = new DayDbDownDlg();
            win.ShowDialog();

        }

        Dictionary<string, double[]> arrays = new Dictionary<string, double[]>();
        Dictionary<string, int> arrayCounts = new Dictionary<string, int>();

        public void addArray(string label, double[] values)
        {
            if (arrays.ContainsKey(label))
            {
                arrays[label] = values;
                arrayCounts[label] = values.Length;
                return;
            }
            arrays.Add(label, values);
            arrayCounts.Add(label, values.Length);
            return;

        }



        internal void addArray(string label, double[] values, int itemCount)
        {
            if (arrays.ContainsKey(label))
            {
                arrays[label] = values;
                arrayCounts[label] = itemCount;
                return;
            }
            arrays.Add(label, values);
            arrayCounts.Add(label, itemCount);
            return;
        }

        public double[] ema(string label, double days, string outLabel)
        {
            if (arrays.ContainsKey(label))
            {
                double[] inputs = arrays[label];
                double[] outputs = MathUtil.calcEMA(inputs, days);
                addArray(outLabel, outputs);
                return outputs;
            }
            return null;
        }




        public double[] diff(string label0, string label1, string outLabel)
        {
            if (arrays.ContainsKey(label0) && arrays.ContainsKey(label1))
            {
                double[] inputs0 = arrays[label0];
                double[] inputs1 = arrays[label1];
                double[] outputs = MathUtil.calcDiff(inputs0, inputs1);

                addArray(outLabel, outputs);
                return outputs;
            }
            return null;
        }

        //tell canvas to line
        public void line(string label, double part, double r, double g, double b, double a, double thickness)
        {
            if (!arrays.ContainsKey(label))
            {
                return;

            }

            Color c = new Color()
            {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
                A = (byte)a
            };

            canvas.addDrawingObj(label, arrays[label], (int)part, c, thickness, DrawingObjectType.Line);
            

        }

        public void clearDrawings()
        {
            canvas.clearDrawings();
        }

        
        List<string> contextMenus = new List<string>();
        public void addContextMenu(string name)
        {
            foreach (var i in contextMenus)
            {
                if (i == name)
                {
                    return;
                }

            }


            contextMenus.Add(name);

            menu.addMenu(name);
            
        }

        public double[] getVals(string id){
            if (arrays.ContainsKey(id))
            {
                return arrays[id];
            }
            return null;
        }


        public void formula(){
                
        }

        public void formulaEnd()
        {

        }


        public void addFormula(string f)
        {
        }

        //now have no formula compiler...
        public void rsi()
        {

        }


        //
        public double[] offsetArray(double[] values, int offset)
        {
            //悲剧没指针
            return null;
        }

        StockContextMenu menu = new StockContextMenu();

        MainView mainView = null;

        internal void setMainView(MainView mainView)
        {
            this.mainView = mainView;
            if (this.mainView.ContextMenu == null)
            {
                this.mainView.ContextMenu = menu;
            }

        }


        public void refreshCanvas()
        {
            canvas.InvalidateVisual();
        }

        
        public void zeroBars(string label, double part, double r, double g, double b, double a, double thickness)
        {
            if (!arrays.ContainsKey(label))
            {
                return;

            }

            Color c = new Color()
            {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
                A = (byte)a
            };
            canvas.addDrawingObj(label, arrays[label], (int)part, c, thickness, DrawingObjectType.zVLines);
        }


        public void kdj(string close, string high, string low, double days, string k, string d, string j)
        {
            if (arrays.ContainsKey(close) && arrays.ContainsKey(high) && arrays.ContainsKey(low))
            {
                double[] inputs0 = arrays[close];
                double[] inputs1 = arrays[high];
                double[] inputs2 = arrays[low];

                double[] kArray;
                double[] dArray;
                double[] jArray;

                var itemCount=arrayCounts[close];
                MathUtil.calcKDJ((int)days, inputs0, inputs1, inputs2, out kArray, out dArray, out jArray,itemCount);

                addArray(k, kArray);
                addArray(d, dArray);
                addArray(j, jArray);
            }
        }


        //draw candle line.
        public void candleLine(string id, double part, string open, string close, string high, string low)
        {
            if (!arrays.ContainsKey(open) || !arrays.ContainsKey(close)
                || !arrays.ContainsKey(high) || !arrays.ContainsKey(low))
            {
                return;
            }

            canvas.addKLineObj(id, (int)part, arrays[open], arrays[close], arrays[high], arrays[low]);

        }

        Dictionary<string, List<object>> configs = new Dictionary<string, List<object>>();

        Dictionary<string, string> strDict = new Dictionary<string, string>();
        public void setStr(string label, string value)
        {
            if (strDict.ContainsKey(label))
            {
                strDict[label] = value;
                return;
            }
           
            strDict.Add(label, value);

        }


        internal string getStr(string label)
        {
            if (strDict.ContainsKey(label))
            {
                return strDict[label];
            }

            return null;
        }

        public void set(string label, string value)
        {
            if (configs.ContainsKey(label))
            {
                configs[label].Add(value);
                return;
            }
            var list=new List<object>() ;
            configs.Add(label, list);
            list.Add(value);
        }

        string lastScriptName;
        public void run(string scriptName)
        {
            string path = "./Py/" + scriptName + ".py";
            if (canvas!=null)
            {
                canvas.clearDrawings();
            }
            //execScript(path);
            PyEngine.Inst.loadScript(scriptName);
            lastScriptName = scriptName;
        }


        public void alert(string s)
        {
            MessageBox.Show(s.ToString());
        }

        public void vLines(string id, double part, string label)
        {
            if (!arrays.ContainsKey(label))
            {
                return;
            }

            Color c = new Color();
            //
            canvas.addDrawingObj(id, arrays[label], (int)part, c, 1, DrawingObjectType.vLines);
        }

        DrawingCanvas canvas = null;
        internal void init(DrawingCanvas canvas)
        {
            this.canvas = canvas;
           
        }
        

        internal List<object> getTechList()
        {
            if (configs.ContainsKey("tech"))
            {
                return configs["tech"];
            }
            return null;
        }


        internal void loadDb()
        {
            DayDb.Inst.load();
            canvas.load();
            PyEngine.Inst.loadTech();

            
        }

    }
}
