using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Jint;
using StockTech.Util;
using Jint.Native;
using System.Security.Permissions;
using StockTech.Py;

namespace StockTech.JS
{
    //warp outter js engine. use JINT javascript interpreter
    //now just do this.
    class JSEngine
    {
        static JSEngine inst=null;
        public static JSEngine Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new JSEngine();
                }
                return inst;
            }
        }

        JintEngine jint;

        void init()
        {
            PythonEngine.Inst.init();

            jint = new JintEngine();

            string absUserDir = Directory.GetCurrentDirectory() + "/Js";
            jint.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, absUserDir));

            //set some simple funcitons.
            Func<object, int> alert = delegate(object s)
            {
                MessageBox.Show(s.ToString());
                return 0;
            };

            Func<string, double, string, double[]> ema = delegate(string label, double days, string outLabel)
            {
                if (arrays.ContainsKey(label))
                {
                    double[] inputs = arrays[label];
                    double[] outputs = MathUtil.calcEMA(inputs, days);
                    addArray(outLabel, outputs);
                    return outputs;
                }
                return null;
            };




            Func<string, string, string, double[]> diff = delegate(string label0, string label1, string outLabel)
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
            };

            //tell canvas to line
            Func<string, double, double, double, double, double, double, int> line = delegate(string label, double part, double r, double g, double b, double a, double thickness)
            {
                if (!arrays.ContainsKey(label))
                {
                    return 1;

                }

                Color c = new Color()
                {
                    R = (byte)r,
                    G = (byte)g,
                    B = (byte)b,
                    A = (byte)a
                };

                canvas.addDrawingObj(label, arrays[label], (int)part, c, thickness, DrawingObjectType.Line);
                return 0;
            };

            Func<string, double, double, double, double, double, double, int> zeroBars = delegate(string label, double part, double r, double g, double b, double a, double thickness)
            {
                if (!arrays.ContainsKey(label))
                {
                    return 1;

                }

                Color c = new Color()
                {
                    R = (byte)r,
                    G = (byte)g,
                    B = (byte)b,
                    A = (byte)a
                };
                canvas.addDrawingObj(label, arrays[label], (int)part, c, thickness, DrawingObjectType.zVLines);

                return 0;
            };
            ////kdj('close','high','low',9,'k','d','j');

            Func<string, string, string, double, string, string, string, int> kdj
                = delegate(string close, string high, string low, double days, string k, string d, string j)
            {
                if (arrays.ContainsKey(close) && arrays.ContainsKey(high) && arrays.ContainsKey(low))
                {
                    double[] inputs0 = arrays[close];
                    double[] inputs1 = arrays[high];
                    double[] inputs2 = arrays[low];

                    double[] kArray;
                    double[] dArray;
                    double[] jArray;
                    MathUtil.calcKDJ((int)days, inputs0, inputs1, inputs2, out kArray, out dArray, out jArray);

                    addArray(k, kArray);
                    addArray(d, dArray);
                    addArray(j, jArray);

                    return 0;
                }
                return 1;
            };


            //draw candle line.
            Func<string, double, string, string, string, string, int> candleLine = delegate(string id, double part, string open, string close, string high, string low)
            {
                if (!arrays.ContainsKey(open) || !arrays.ContainsKey(close)
                    || !arrays.ContainsKey(high) || !arrays.ContainsKey(low))
                {
                    return 1;
                }

                canvas.addKLineObj(id, (int)part, arrays[open], arrays[close], arrays[high], arrays[low]);
                return 0;
            };

            Func<string, object, int> addConfig = delegate(string id, object value)
            {
                addConfigVal(id, value);
                return 0;
            };


            Func<string, int> runScript = delegate(string scriptName)
            {
                runJsScript(scriptName);
                return 0;
            };


            Func<string, double, string, double, double, double, double, double, int> vLines = delegate(string id, double part, string label, double r, double g, double b, double a, double thickness)
            {
                if (!arrays.ContainsKey(label))
                {
                    return 1;

                }

                Color c = new Color()
                {
                    R = (byte)r,
                    G = (byte)g,
                    B = (byte)b,
                    A = (byte)a
                };

                //
                canvas.addDrawingObj(id, arrays[label], (int)part, c, thickness, DrawingObjectType.vLines);
                return 0;
            };

            //
            Func<string, string, int> setDrawItemEventHandler = delegate(string id, string handlerName)
            {
                canvas.setDrawItemEventHandler(id, handlerName);
                return 0;

            };




            jint.SetFunction("addConfig", addConfig);
            jint.SetFunction("runScript", runScript);

            jint.SetFunction("setDrawItemEventHandler", setDrawItemEventHandler);
            

            jint.SetFunction("alert", alert);
            jint.SetFunction("ema", ema);
            jint.SetFunction("kdj", kdj);
            jint.SetFunction("diff", diff);
            jint.SetFunction("line", line);
            jint.SetFunction("zeroBars", zeroBars);
            jint.SetFunction("candleLine", candleLine);
            jint.SetFunction("vLines", vLines);


          
        }

        private void setDrawItemEventHandler_(string id, string handlerId)
        {
            //throw new NotImplementedException();
        }


        Dictionary<string, string> drawItemHandlers = new Dictionary<string, string>();
        protected JSEngine()
        {
            init();
        }

        string lastScriptName = null;
        private void runJsScript(string scriptName)
        {
            string path = "./Js/" + scriptName + ".js";
            if (canvas!=null)
            {
                canvas.clearDrawings();
            }
            execScript(path);
            lastScriptName = scriptName;
        }

        Dictionary<string, List<object>> configs = new Dictionary<string, List<object>>();

        public void addConfigVal(string label, object value)
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

        Dictionary<string,double[]> arrays=new Dictionary<string,double[]>();
        public void addArray(string label, double[] values)
        {
            if (arrays.ContainsKey(label))
            {
                arrays[label] = values;
                return;
            }
            arrays.Add(label, values);
            return;

        }

        

        Dictionary<string, string> sourceCodes = new Dictionary<string, string>();
        
        //
        private void execScript(string path)
        {
           
            if (!sourceCodes.ContainsKey(path))
            {
                if (!File.Exists(path))
                {
                    return;
                }


                var fstream = new FileStream(path, FileMode.Open);
                StreamReader reader = new StreamReader(fstream);
                var code = reader.ReadToEnd();
                sourceCodes.Add(path, code);
            }
           var src= sourceCodes[path];
            try
            {
                //just run.
                jint.Run(src);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }

            if (canvas!=null)
            {
                canvas.InvalidateVisual();
            }
        }


        DrawingCanvas canvas = null;
        internal void setDrawingCanvas(DrawingCanvas drawingCanvas)
        {
            this.canvas = drawingCanvas;

            //now can run some init script...
            initConfig();



        }


        bool configInited = false;

        void initConfig()
        {
            if (configInited)
            {
                return;
            }

            execScript("./Js/config.js");
            try
            {
                jint.CallFunction("config");
                jint.CallFunction("init");
            }
            catch (Exception e)
            {
            }

            configInited = true;


        }

        internal List<object> getTechList()
        {
           
            if (configs.ContainsKey("tech"))
            {
                return configs["tech"];
            }
            return null;
        }

        //
        public void onTechClick(string name)
        {
            try
            {
                jint.CallFunction("onTechClick", name);
            }
            catch (Exception e)
            {
            }

        }


        internal void runLast()
        {
            if (lastScriptName!=null)
            {
                runJsScript(lastScriptName);
            }
            
        }

        internal void call(string p,double open,double close,double high,double low,double vol,double amount)
        {
            if (p == null || p.Length == 0)
            {
                return;
            }

            try
            {
                var obj=jint.CallFunction(p, open, close, high, low, vol, amount);
                Console.WriteLine(obj);
            }
            catch (Exception e)
            {
            }

            
        }
    }
}
