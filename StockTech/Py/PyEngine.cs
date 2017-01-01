using System;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;

namespace StockTech.Py
{
    class PyEngine
    {
        static PyEngine inst = null;
        public static PyEngine Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new PyEngine();
                }
                return inst;
            }
        }

        ScriptEngine engine = null;

        protected PyEngine()
        {
            engine = Python.CreateEngine();
        }

        DrawingCanvas canvas = null;
        internal void initCanvas(DrawingCanvas canvas)
        {
            this.canvas = canvas;
            StockEngine.Inst.init(canvas);

            //init stock engine by python config.
            initConfig();
        }

        Dictionary<string, ScriptScope> scopes = new Dictionary<string, ScriptScope>();
         private void initConfig()
        {
            //加载Python配置脚本
             loadScript("config");
        }

         public void loadScript(string scriptName)
         {
             string path = "./Py/" + scriptName + ".py";

             if (scopes.ContainsKey(scriptName))
             {
                 return;//already load
             }
             if (!File.Exists(path))
             {
                 return;
             }

             //canvas.clearDrawings();
             var scope = engine.CreateScope();
             scope.SetVariable("stk", StockEngine.Inst );
             scopes.Add(scriptName, scope);

             var fstream = new FileStream(path, FileMode.Open);
             StreamReader reader = new StreamReader(fstream);
             var code = reader.ReadToEnd();
             var source = engine.CreateScriptSourceFromString(code);
             try
             {
                 source.Execute(scope);
             }
             catch (Exception e)
             {
                 
             }

         }


         string techName = null;
        internal void onTechClick(string name)
        {
            if (!scopes.ContainsKey("config"))
            {
                return;
            }

            var scope = scopes["config"];

            var onTechChecked = scope.GetVariable<Func<string, int>>("onTechChecked");

            techName = name;
            if (onTechChecked != null)
            {
                onTechChecked(name);
            }

        }

        internal List<object> getTechList()
        {
            return StockEngine.Inst.getTechList();
        }

        internal void runLast()
        {
        }

        internal void addArray(string p, double[] vals)
        {
            StockEngine.Inst.addArray(p, vals);
        }

        internal void reload(string name)
        {
            onTechClick(name);
        }

        internal void contextMenuClick(string name)
        {
            if (!scopes.ContainsKey("config"))
            {
                return;
            }

            var scope = scopes["config"];

            var func = scope.GetVariable<Func<string,int>>("onContextMenu");
            if (func != null)
            {
                func(name);
            }

        }


        internal void addArray(string l, double[] vals, int itemCount)
        {
            StockEngine.Inst.addArray(l, vals, itemCount);
        }

        internal void loadTech()
        {
            if (techName != null)
            {
                onTechClick(techName);
            }

        }

        internal void init()
        {
            loadScript("init");
        }


    }
}
