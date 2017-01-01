//可视化股票技术分析软件

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StockTech.Data
{
    //定义节点类Node
    public class StockNode
    {
        //构造函数
        public StockNode(string name)
        {
            this.Name = name;
            this.Nodes = new List<StockNode>();
        }


        public string Name { get; set; }
        public List<StockNode> Nodes { get; set; }//节点集合
    }


    public class IndustFile
    {
        //first is category name,two level tree for simplicity (both programmer and user)
        public List<StockNode> Items
        {
            get
            {
                return this.items;
            }
        }

        List<StockNode> items = new List<StockNode>();
        static public IndustFile Inst
        {
            get
            {
                if (file == null)
                {
                    file = new IndustFile(PathConfig.IndustFilePath);
                }
                return file;
            }
        }
        static IndustFile file = null;

        protected IndustFile(string path)
        {
            readFile(path);
        }


        private void readFile(string path)
        {
            StreamReader reader = File.OpenText(path);
            string line = null;

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.Length == 0)
                {
                    continue;
                }

                StockNode root = new StockNode(line.Trim());
                items.Add(root);
                line = reader.ReadLine();
                while (line != null && line.Length > 0)
                {
                    string[] fields = line.Split('\t');
                    string field = line;
                    if (fields.Length > 0)
                    {
                        field = fields[0];
                    }

                    root.Nodes.Add(new StockNode(field));
                    line = reader.ReadLine();

                }

            }

        }


    }

}

