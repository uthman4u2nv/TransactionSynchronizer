using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StorekeeperSynchronizer
{
    class TransSender
    {
        public int CountPendingTrans()
        {
            int count = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection())
                {
                    //con.ConnectionString = ConfigurationManager.AppSettings["MySQLCon"];
                    //con.ConnectionString = "Server=172.21.1.35; Database=jaiz_db; Uid=appsoluser; Pwd=@ppSol@Jaiz;";
                    con.ConnectionString = ConfigurationManager.AppSettings["constring"];
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT Count(*) FROM routing WHERE routeStatus='0'";

                    count = Convert.ToInt32(cmd.ExecuteScalar());



                }
            }
            catch (Exception ex)
            {
                var err2 = new LogUtility.Error()
                {
                    ErrorDescription = ex.Message + Environment.NewLine + ex.InnerException,
                    ErrorTime = DateTime.Now,
                    ModulePointer = "Error Counting Transactions",
                    StackTrace = ex.StackTrace

                };
                LogUtility.ActivityLogger.WriteErrorLog(err2);
            }

            return count;
        }
        private void updateTrans(int status, int transID)
        {
            try
            {
                //This is my connection string i have assigned the database file address path  
                string MyConnection2 = ConfigurationManager.AppSettings["constring"];
                //This is my update query in which i am taking input from the user through windows forms and update the record.  
                string Query = "update routing set routeStatus='" + status + "' where routeID='" + transID + "';";
                //This is  MySqlConnection here i have created the object and pass my connection string.  
                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
                MyReader2 = MyCommand2.ExecuteReader();

                while (MyReader2.Read())
                {
                }
                MyConn2.Close();//Connection closed here  
            }
            catch (Exception ex)
            {
                var err2 = new LogUtility.Error()
                {
                    ErrorDescription = ex.Message + Environment.NewLine + ex.InnerException,
                    ErrorTime = DateTime.Now,
                    ModulePointer = "Error Updating Route",
                    StackTrace = ex.StackTrace

                };
                LogUtility.ActivityLogger.WriteErrorLog(err2);
            }
        }
        private void updateSMS(int status, string smsID)
        {
            try
            {
                //This is my connection string i have assigned the database file address path  
                string MyConnection2 = ConfigurationManager.AppSettings["constring"];
                //This is my update query in which i am taking input from the user through windows forms and update the record.  
                string Query = "update sms set smsStatus='" + status + "' where smsID='" + smsID + "';";
                //This is  MySqlConnection here i have created the object and pass my connection string.  
                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
                MyReader2 = MyCommand2.ExecuteReader();

                while (MyReader2.Read())
                {
                }
                MyConn2.Close();//Connection closed here  
            }
            catch (Exception ex)
            {
                var err2 = new LogUtility.Error()
                {
                    ErrorDescription = ex.Message + Environment.NewLine + ex.InnerException,
                    ErrorTime = DateTime.Now,
                    ModulePointer = "Error Sending SMS",
                    StackTrace = ex.StackTrace

                };
                LogUtility.ActivityLogger.WriteErrorLog(err2);
            }
        }
        public void WriteLog(string message,string subject)
        {
            var err2 = new LogUtility.Error()
            {
                ErrorDescription = message,
                ErrorTime = DateTime.Now,
                ModulePointer = subject,
                //StackTrace = ex.StackTrace

            };
            LogUtility.ActivityLogger.WriteErrorLog(err2);
        }
        public class PostingResult
        {
            public string status { get; set; }
            public string message { get; set; }
        }

        public static bool chk_con()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public async void PushTransactions()
        {
            bool chkConn = false;

            try
            {
                chkConn = chk_con();
                if (chkConn) { 
                    var client = new HttpClient();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (MySqlConnection con = new MySqlConnection())
                {
                    //con.ConnectionString = ConfigurationManager.AppSettings["MySQLCon"];
                    //con.ConnectionString = "Server=172.21.1.35; Database=jaiz_db; Uid=appsoluser; Pwd=@ppSol@Jaiz;";
                    con.ConnectionString = ConfigurationManager.AppSettings["constring"];
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT * FROM routing WHERE routeStatus='0'";
                    // cmd.Parameters.Add("username", MySqlDbType.VarChar).Value = username;


                    var url = "";
                    var routeJSON = "";
                    int transID;

                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        //rdr.Read();


                        while (rdr.Read())
                        {
                            transID = Convert.ToInt16(rdr["routeID"].ToString());
                            url = rdr["routeURL"].ToString();
                            routeJSON = rdr["routeJSON"].ToString();

                            //update the transactions to prevent double picking
                            updateTrans(2, transID);
                            //var rs = JsonConvert.SerializeObject(t);
                            WriteLog(routeJSON,"Transaction Picked for posting");


                            var content = new StringContent(routeJSON, Encoding.UTF8, "application/json");


                            HttpResponseMessage result = null;
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ServicePointManager.ServerCertificateValidationCallback = new
                          RemoteCertificateValidationCallback
                          (
                             delegate { return true; }
                          );
                            result = client.PostAsync(url, content).Result;


                            var r = result.Content.ReadAsStringAsync().Result.Replace("\\", "");
                            var rr = JsonConvert.DeserializeObject<PostingResult>(r);


                            if (rr.status == "00")
                            {
                                var m = "Status:" + rr.status + " Message:" + rr.message;
                                WriteLog(m,"Transaction Synching Successful");
                                //updateMessage(1, smsID);
                                updateTrans(1, transID);
                                //Console.WriteLine("Success");
                            }
                            else
                            {
                                // updateMessage(2, smsID);
                                var m = "Status:" + rr.status + " Message:" + rr.message;
                                WriteLog(m,"Transaction Synching Failed");
                                updateTrans(0, transID);
                                //Console.WriteLine("Error");
                            }
                        }
                    }
                }
            }else
            {
                    WriteLog(DateTime.Now+":No internet connection detected, waiting to try again","Internet Connection Error");
            }

            }
            catch (Exception ex)
            {
                var err2 = new LogUtility.Error()
                {
                    ErrorDescription = ex.Message + Environment.NewLine + ex.InnerException,
                    ErrorTime = DateTime.Now,
                    ModulePointer = "Error Sending Transactions",
                    StackTrace = ex.StackTrace

                };
                LogUtility.ActivityLogger.WriteErrorLog(err2);
            }


        }
    }
}
