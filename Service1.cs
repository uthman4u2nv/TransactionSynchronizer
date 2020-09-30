using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace StorekeeperSynchronizer
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            DebugMode();
            WriteToFile("Service is ran at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(this.SelectTransOnQueue);
            timer.Interval = 5000; //number in milisecinds  
            timer.Enabled = true;
        }
        private void SelectTransOnQueue(object sender, ElapsedEventArgs e)
        {
            TransSender i = new TransSender();
            int count = i.CountPendingTrans();
            Thread tr1 = null;
            Thread tr2 = null;
            Thread tr3 = null;
            Thread tr4 = null;
            Thread tr5 = null;
            Thread tr6 = null;
            Thread tr7 = null;
            Thread tr8 = null;
            Thread tr9 = null;
            Thread tr10 = null;


            if (count > 0)
            {
                try
                {

                    tr1 = new Thread(delegate () { i.PushTransactions(); });
                    tr1.Start();

                    //tr2 = new Thread(delegate () { i.PushTransactions(1); });
                    //tr2.Start();

                    //tr3 = new Thread(delegate () { i.PushTransactions(2); });
                    //tr3.Start();

                    //tr4 = new Thread(delegate () { i.PushTransactions(3); });
                    //tr4.Start();

                    //tr5 = new Thread(delegate () { i.PushTransactions(4); });
                    //tr5.Start();

                    //tr6 = new Thread(delegate () { i.PushTransactions(5); });
                    //tr6.Start();

                    //tr7 = new Thread(delegate () { i.PushTransactions(6); });
                    //tr7.Start();

                    //tr8 = new Thread(delegate () { i.PushTransactions(7); });
                    //tr8.Start();

                    //tr9 = new Thread(delegate () { i.PushTransactions(8); });
                    //tr9.Start();

                    //tr10 = new Thread(delegate () { i.PushTransactions(9); });
                    //tr10.Start();




                }
                catch (Exception ex)
                {
                    var err22 = new LogUtility.Error()
                    {
                        ErrorDescription = "Error Code:" + ex.InnerException + " Response Message:" + ex.Message + Environment.NewLine + ex.Message,
                        ErrorTime = DateTime.Now,
                        ModulePointer = "Transaction Synchng Error",
                        StackTrace = ex.InnerException.ToString()
                    };
                    LogUtility.ActivityLogger.WriteErrorLog(err22);
                }
            }
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
        [Conditional("DEBUG_SERVICE")]
        private static void DebugMode()
        {
            Debugger.Break();
        }
    }
}
