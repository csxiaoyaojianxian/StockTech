using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;

namespace StockTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            if (File.Exists("./path.txt"))
            {
                var f = File.Open("./path.txt", FileMode.OpenOrCreate);
                StreamReader r = new StreamReader(f);
                path = r.ReadLine();
                pathTxtblk.Text = path;
                r.Close();
            }

            if (!Directory.Exists("./Data"))
            {
                Directory.CreateDirectory("./Date");
            }

            if (!Directory.Exists("./Data/Date"))
            {
                Directory.CreateDirectory("./Data/Date");
            }
            if (!Directory.Exists("./Data/Db"))
            {
                Directory.CreateDirectory("./Data/Db");
            }

            //loadImportedFiles();
        }

        private void loadImportedFiles()
        {
            
            string path = "./Day/";
            var l=Directory.EnumerateFiles(path);
            List<string> list = new List<string>();

            var f = File.Open("./date.txt", FileMode.OpenOrCreate);
            StreamWriter w = new StreamWriter(f);
            foreach (var i in l)
            {
                var sl = i.Split('/')[2];
                list.Add(sl);
                w.WriteLine(sl);
            }
            w.Close();
            f.Close();
        }

        //
        IEnumerable<string> szPaths = new List<string>();
        IEnumerable<string> shPaths = new List<string>();
        List<string> pathes = new List<string>();

        Thread workThread = null;
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            workThread = new Thread(new ThreadStart(delegate { startImport(); }));
            workThread.Name = "work_thread";
            workThread.Start();
        }

       


        //problem with new symbol added...
        //
        private void startImport()
        {

            loadSymbols();
            loadDates();
            constuctImportPathes();
          
            //
            import();

        }

        //
        private void loadDates()
        {

            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentProcess.Text = "导入已下载的日期表 ";
            });

            string p = "./Data/Date/date.txt";

            var f = File.Open(p, FileMode.Open);
            StreamReader r = new StreamReader(f);
            while (!r.EndOfStream)
            {
                string line=r.ReadLine();
                int dt=0;
                if (Int32.TryParse(line, out dt))
                {
                    datesImported.Add(dt);
                }

            }

        }

        //
        private void constuctImportPathes()
        {
            //not consider new IPO stock
            foreach (var i in symbols)
            {
                if (i.Contains("sh"))
                {
                    pathes.Add(path + "/vipdoc/sh/lday/" + i + ".day");
                }
                if (i.Contains("sz"))
                {
                    pathes.Add(path + "/vipdoc/sz/lday/" + i + ".day");
                }

            }
            

        }

        private void oldimportFirst()
        {
            string shPath = path + "/vipdoc/sh/lday";
            if (Directory.Exists(shPath))
            {
                shPaths = Directory.EnumerateFiles(shPath);
            }

            string szPath = path + "/vipdoc/sz/lday";
            if (Directory.Exists(szPath))
            {
                szPaths = Directory.EnumerateFiles(szPath);
            }

            foreach (var i in shPaths)
            {
                pathes.Add(i);
            }

            foreach (var i in szPaths)
            {
                pathes.Add(i);
            }
        }

        public List<int> datesImported = new List<int>();

        private void loadSymbols()
        {

            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentProcess.Text = "导入符号表! ";
            });


            //
            var f = File.Open("./Data/Date/symbols.txt", FileMode.OpenOrCreate);
            StreamReader r = new StreamReader(f);
            //
            while(!r.EndOfStream){
                var line=r.ReadLine();
                symbols.Add(line);
            }

            
            r.Close();
            f.Close();

           

        }

        //build two dim array...
        //
        private void import()
        {

            foreach (var p in pathes)
            {
                import(p);
            }

            //addSymbols();

            processDatePrices();

            saveDatePrices();

            saveDatefile("./Data/Date/date.txt");

            saveDbFiles();

            System.Windows.MessageBox.Show("导入完成了!");
        }

        //

        string symbolFilePath = "./Data/Db/symbols.txt";
        private void saveDbFiles()
        {

            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentProcess.Text = "写入日线数据库!" ;
                progressBar.Maximum = 100;
                progressBar.Value = 100;
            });

            saveDatefile("./Data/Db/date.txt");
            saveSymbolFile(symbolFilePath);

            string path = "./Data/Db/daydb";

            var dbfile = File.Open(path, FileMode.OpenOrCreate);
            BinaryWriter w = new BinaryWriter(dbfile);

            foreach (var i in dateprices)
            {

                int priceChunkSize = sizeof(int) * 5 + sizeof(float);
                int rowBytes = priceChunkSize * symbols.Count;//a row is in same date.

                foreach (var j in i.Value)
                {
                    w.Write(j.open);
                    w.Write(j.close);
                    w.Write(j.high);
                    w.Write(j.low);
                    w.Write(j.volume);
                    w.Write(j.amount);
                }

            }
            w.Close();
            dbfile.Close();

        }

        List<int> datesAll = new List<int>();

        private void saveDatefile(string path)
        {
            foreach (var i in datesImported)
            {
                if (!datesAll.Contains(i))
                {
                    datesAll.Add(i);
                }

            }

            foreach (var i in dateprices)
            {
                if (!datesAll.Contains(i.Key))
                {
                    datesAll.Add(i.Key);
                }
                
            }


            var f = File.Open(path, FileMode.Create);
            StreamWriter w = new StreamWriter(f);


            foreach (var i in datesAll)
            {
                w.WriteLine(i);
            }

            w.Close();
            f.Close();

        }


        //
        private void saveDatePrices()
        {
            //need sort by key.
            int processIndex = 0;
            foreach(var i in dateprices){
                var date = i.Key;
                string savePath = "./Data/Date/" + date;

                if (File.Exists(savePath))
                {
                    return;
                }


                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    textCurrentProcess.Text = "保存日期: " + date;
                    progressBar.Maximum = dateprices.Count;
                    progressBar.Value = processIndex;
                });
                processIndex++;




                var f=File.Open(savePath, FileMode.Create);
                BinaryWriter w = new BinaryWriter(f);
                var items=i.Value;
                foreach(var j in items){
                    w.Write(j.open);
                    w.Write(j.close);
                    w.Write(j.high);
                    w.Write(j.low);
                    w.Write(j.volume);
                    w.Write(j.amount);

                }
                w.Close();
                f.Close();
                
            }

        }

        
        private void processDatePrices()
        {
            int processIndex = 0;
            foreach (var item in dayprices)
            {

                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    textCurrentProcess.Text = "处理符号: " + item.Key;
                    progressBar.Maximum = dayprices.Count;
                    progressBar.Value = processIndex;
                });
                processIndex++;


                int symbolIndex = symbols.IndexOf(item.Key);
                var items = item.Value;

                foreach (var i in items)
                {
                    var dt = i.date;

                    if (datesImported.Contains(dt))
                    {
                        continue;//already in continue.
                    }

                    DatePrice[] prices = null;

                 

                    if (!dateprices.ContainsKey(dt))
                    {
                        var cnt = symbols.Count;
                        prices = new DatePrice[cnt];
                        dateprices.Add(dt, prices);
                    }
                    else
                    {
                        prices = dateprices[dt];
                    }

                    prices[symbolIndex].open = i.open;
                    prices[symbolIndex].close = i.close;
                    prices[symbolIndex].high = i.high;
                    prices[symbolIndex].low = i.low;
                    prices[symbolIndex].volume = i.volume;
                    prices[symbolIndex].amount = i.amount;

                }
            }
        }


        List<string> symbols = new List<string>();

         public struct DatePrice
        {
            public int open;
            public int high;
            public int low;
            public int close;
            public int volume;
            public float amount;
            
        }

        //first int is date
         Dictionary<int, DatePrice[]> dateprices = new Dictionary<int, DatePrice[]>();
         Dictionary<int, int> datepriceCount = new Dictionary<int, int>();

         Dictionary<int, List<string>> symbolsByCount = new Dictionary<int, List<string>>();

        private void addSymbols()
        {
            foreach (var i in dayprices)
            {
                if (symbols.Contains(i.Key))
                {
                    continue;
                }

                symbols.Add(i.Key);

                List<string> list = null;
                if (!symbolsByCount.ContainsKey(i.Value.Length))
                {
                    list = new List<string>();
                    symbolsByCount.Add(i.Value.Length, list);
                }
                else
                {
                    list = symbolsByCount[i.Value.Length];
                }
                list.Add(i.Key);
                
            }

            saveSymbolFile( "./Data/Date/symbols.txt");
            
        }

        private void saveSymbolFile(string p)
        {
           /*
            var f = File.Open(p, FileMode.Create);
            StreamWriter w = new StreamWriter(f);

            foreach (var i in dayprices)
            {
                w.WriteLine(i.Key);
            }

            w.Close();
            f.Close();
            * */

        }

        //
        public struct DayPrice
        {
            public int date;
            public int open;
            public int high;
            public int low;
            public int close;
            public float amount;
            public int volume;
        }


        Dictionary<string, DayPrice[]> dayprices = new Dictionary<string, DayPrice[]>();
        int importedIndex = 0;
        void import(string path)
        {
          

            var list=path.Split('.');

            var p = list[list.Length - 2].Split(new char[]{'\\','/'});
            var sy = p[p.Length - 1];

           

            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                textCurrentProcess.Text = "导入日线: " + sy;
                progressBar.Maximum = symbols.Count;
                progressBar.Value = importedIndex;
            });
            importedIndex++;
            if (!File.Exists(path))
            {
                return;
            }

            var fstream = File.Open(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(fstream);
            long itemCount = fstream.Length / 32;
            DayPrice[] items = new DayPrice[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                items[i].date = reader.ReadInt32();
                items[i].open = reader.ReadInt32();
                items[i].high = reader.ReadInt32();
                items[i].low = reader.ReadInt32();
                items[i].close = reader.ReadInt32();
                items[i].amount = reader.ReadSingle();
                items[i].volume = reader.ReadInt32();
                int reservation = reader.ReadInt32();
            }

            dayprices.Add(sy, items);

        }




        string path = null;
        private void buttonFileOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowDialog();

            path = dlg.SelectedPath;
            pathTxtblk.Text = path;

           var f= File.Open("./path.txt",FileMode.OpenOrCreate);
           StreamWriter w = new StreamWriter(f);
           w.WriteLine(path);
           w.Close();

        }
    }
}
