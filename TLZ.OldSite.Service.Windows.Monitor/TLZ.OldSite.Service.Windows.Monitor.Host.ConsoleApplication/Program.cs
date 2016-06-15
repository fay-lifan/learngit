using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using TLZ.Helper;
using TLZ.MongoDB;
using TLZ.OldSite.DB.MongoDB.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TLZ.OldSite.Service.Windows.Monitor.Host.ConsoleApplication
{
    class Program
    {
        /// <summary>
        /// 系统进程帮助类
        /// </summary>
        static ProcessHelper prohelper;
        static string systemAddress;
        static LogWritesHelper logHelper;
        static Program()
        {
            logHelper = new LogWritesHelper();
            prohelper = new ProcessHelper();
            
            if (ConfigurationManager.AppSettings["SystemAddress"] != null)
            {
                systemAddress = ConfigurationManager.AppSettings["SystemAddress"];
            }
            else
                throw new Exception("未配置对应的系统区域AppSetting");
        }
        static void Main(string[] args)
        {
            using (var pro = Process.GetProcessesByName("QQ")[0])
            {
                //间隔时间（毫秒）
                int interval = 10000;
                //上次记录的CPU时间
                var prevCpuTime = TimeSpan.Zero;
                while (true)
                {
                    //当前时间
                    var curTime = pro.TotalProcessorTime;
                    //间隔时间内的CPU运行时间除以逻辑CPU数量
                    var value = (curTime - prevCpuTime).TotalMilliseconds / interval / Environment.ProcessorCount * 100;
                    prevCpuTime = curTime;
                    //输出
                    Console.WriteLine(value);

                    Thread.Sleep(interval);
                }
            }
            //#region 插入数据
            //for (int i = 0; i < 20; i++)
            //{
            //    MongoDBHelper.Insert<ActivityConfigures>(
            //                   new ActivityConfigures()
            //                   {
            //                       ActivityName = i.ToString(),
            //                       ProcessName = "",
            //                       Threshold = (double)10000,
            //                       TntervalSecond = 10000,
            //                       isEmail = false,
            //                       isSMS = false,
            //                       DateTimeNow = DateTime.Now
            //                   }, MongoConnType.Center);
            //}            
            //Console.ReadKey();
            ////ThreadMonitorService test = new ThreadMonitorService();
            //#endregion
            //while (true)
            //{
                
            
            //string pingrst = string.Empty;

            //Process p = new Process();
            //p.StartInfo.FileName = "cmd.exe";
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardInput = true;
            //p.StartInfo.RedirectStandardOutput = true;
            //p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.CreateNoWindow = true;
            //p.Start();
            //p.StandardInput.WriteLine("ping -n 1 103.235.223.120");
            //p.StandardInput.WriteLine("exit");
            //string strRst = p.StandardOutput.ReadToEnd();

            //if (strRst.IndexOf("Ping 统计信息:") != -1)
            //{
            //    pingrst = "请求成功";
            //}
            //else if (strRst.IndexOf("请求超时。") != -1)
            //    pingrst = "请求超时";
            //else if (strRst.IndexOf("请求找不到主机") != -1)
            //    pingrst = "请求找不到主机";
            //else if (strRst.IndexOf("不知名主机") != -1)
            //    pingrst = "请求无法解析主机";
            //else
            //    pingrst = strRst;
            //p.Close();

            //var model = new SystemPing() { };

            //strRst = Regex.Replace(strRst, @"\s", "");

            //string ziJie = strRst.Substring(strRst.IndexOf("字节=")+3);
            //model.PingByte = double.Parse(ziJie.Substring(0, ziJie.IndexOf("时间=")));
            //string shiJian = strRst.Substring(strRst.IndexOf("时间=") + 3);
            //model.PingDate = double.Parse(shiJian.Substring(0, shiJian.IndexOf("TTL=") - 2));
            //string ttl = strRst.Substring(strRst.IndexOf("TTL=") + 4);
            //model.PingTTL = double.Parse(ttl.Substring(0, ttl.IndexOf("103.235.223.120的Ping统计信息:")));
            //string yiFaSong = strRst.Substring(strRst.IndexOf("已发送=") + 4);
            //model.PingFaSong = double.Parse(yiFaSong.Substring(0, yiFaSong.IndexOf("已接收=")-1));
            //string yiJieShou = strRst.Substring(strRst.IndexOf("已接收=") + 4);
            //model.PingJieShou = double.Parse(yiJieShou.Substring(0, yiJieShou.IndexOf("丢失=") - 1));
            //string diuShi = strRst.Substring(strRst.IndexOf("丢失=") + 3);
            //model.PingDiuShi = double.Parse(diuShi.Substring(0, diuShi.IndexOf("(")));
            //string diuShiLv = strRst.Substring(strRst.IndexOf("丢失=") + 3);
            //model.PingDiuShiLv = diuShiLv.Substring(diuShiLv.IndexOf("(")+1, diuShiLv.IndexOf("丢失)") - 2);
            //logHelper.Write(new Msg(strRst, MsgType.Error));
            //logHelper.Dispose();
            //Console.Write(strRst);
            //}
        }
    }
}
