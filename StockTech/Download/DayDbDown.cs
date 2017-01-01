using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using StockTech.Py;

namespace StockTech.Download
{
    public class DayDbDown
    {
        static DayDbDown inst = null;
        public static DayDbDown Inst
        {
            get
            {
                if (inst == null)
                {
                    inst = new DayDbDown();
                }
                return inst;
            }
        }
        //数据的可见性
        protected DayDbDown()
        {
            //
            downUrl = StockEngine.Inst.getStr("stockUrl");
            downSymbolsUrl = downUrl + "/symbols.txt";
            downDateUrl = downUrl + "/date.txt";

        }

        string downUrl =null;
        string downSymbolsUrl = null;
        string downDateUrl = null;

        string dbFilePath = "./Data/Db/daydb";
        string dbDatesFilePath = "./Data/Db/daydb.dt";
        string symbolsFilePath = "./Data/Db/symbols.txt";
        string datesToDownFilePath = "./Data/Db/date.txt";

        List<int> dateAll = new List<int>();
        List<int> datesInDb = new List<int>();
        List<int> datesNeedDown = new List<int>();
        List<string> symbols = new List<string>();

        FileStream dbFileStream = null;
        int downIndex = -1;//down action
        int totalDownloaded = 0;//downloaded...

        Thread workThread = null;
        internal void startDown()
        {
            if (downUrl == null )
            {
                return;
            }

            workThread = new Thread(new ThreadStart(delegate { start(); }));
            workThread.Name = "work_thread";
            workThread.Start();
        }

        bool downloding = false;
        private void start()
        {
            if (downloding)
            {
                return;
            }
            downloding = true;
            dbFileStream = File.Open(dbFilePath, FileMode.OpenOrCreate);


            if (File.Exists(dbDatesFilePath))
            {
                var f = File.Open(dbDatesFilePath, FileMode.Open);
                BinaryReader r = new BinaryReader(f);
                int len = (int)f.Length;
                int pos = 0;
                while (pos < len)
                {
                    var i = r.ReadInt32();
                    datesInDb.Add(i);
                    pos += sizeof(int);

                }
                r.Close();
                f.Close();

            }

            downDatesFile();

            if (datesNeedDown.Count == 0)
            {
                if (OnNoNewData != null)
                {
                    OnNoNewData();
                }

                return;
            }

            downSymbolsFile();

            downNext();


        }

        private void downNext()
        {
            downIndex++;
            if (downIndex < datesNeedDown.Count)
            {
                down(datesNeedDown[downIndex]);
            }
            else
            {
            }

        }

        private void downDatesFile()
        {
            WebClient client = new WebClient();
            client.DownloadFile(new Uri(downDateUrl), datesToDownFilePath);


            var f = File.Open(datesToDownFilePath, FileMode.Open);
            var r = new StreamReader(f);

            while (true)
            {
                var line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                int dt = 0;
                if (Int32.TryParse(line, out dt))
                {
                    dateAll.Add(dt);

                }
            }

            f.Close();


            foreach (var i in dateAll)
            {
                if (!datesInDb.Contains(i))
                {
                    datesNeedDown.Add(i);
                }

            }


        }



        private void downSymbolsFile()
        {
            WebClient client = new WebClient();
            client.DownloadFile(new Uri(downSymbolsUrl), symbolsFilePath);

            var f = File.Open(symbolsFilePath, FileMode.Open);
            var r = new StreamReader(f);

            while (true)
            {
                var line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                symbols.Add(line);
            }
            f.Close();
        }

        //
        public delegate void DateDownHandler(int date,int val,int max);
        public event DateDownHandler OnDateDown;

        public delegate void DownFinishHandler();
        public event DownFinishHandler OnDownFinish;

        public delegate void NoNewDataHandler();
        public event NoNewDataHandler OnNoNewData;

        private void down(int i)
        {
            if (!datesNeedDown.Contains(i))
            {
                totalDownloaded++;
                if (OnDateDown != null)
                {
                    OnDateDown(i, totalDownloaded, datesNeedDown.Count);
                }

                downNext();
                return;
            }


            WebClient client = new WebClient();
            string url = downUrl + "/" + i;
            client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
            client.DownloadDataAsync(new Uri(url), i);

        }

        void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            totalDownloaded++;
            var date = (int)e.UserState;
            if (OnDateDown != null)
            {
                OnDateDown(date, totalDownloaded, datesNeedDown.Count);
            }

            if (e.Error != null)
            {
                if (totalDownloaded == datesNeedDown.Count)
                {
                    downFinished();
                }
                downNext();
                return;
            }




            if (e.Result == null || e.Result.Length == 0)
            {
                if (totalDownloaded == datesNeedDown.Count)
                {
                    downFinished();
                }
                downNext();
                return;
            }


            //
            int dateIndex = dateAll.IndexOf(date);
            int priceChunkSize = sizeof(int) * 5 + sizeof(float);
            int rowBytes = priceChunkSize * symbols.Count;//a row is in same date.

            int offset = rowBytes * dateIndex;

            datesInDb.Add(date);

            dbFileStream.Seek(offset, SeekOrigin.Begin);
            dbFileStream.Write(e.Result, 0, e.Result.Length);


            if (totalDownloaded == datesNeedDown.Count)
            {
                downFinished();
                return;
            }


            downNext();

        }

        private void downFinished()
        {
            downloding = false;
            saveDatesDownFile();

            dbFileStream.Close();

            if(OnDownFinish!=null){
                OnDownFinish();
            }

           

        }


        private void saveDatesDownFile()
        {
            var f = File.Open(dbDatesFilePath, FileMode.OpenOrCreate);
            BinaryWriter wt = new BinaryWriter(f);
            foreach (var i in datesInDb)
            {
                wt.Write(i);
            }

            wt.Close();
            f.Close();

        }




    }
}
