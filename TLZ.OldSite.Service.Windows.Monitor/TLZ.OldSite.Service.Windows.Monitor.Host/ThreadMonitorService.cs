using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using TLZ.Helper;
using TLZ.MongoDB;
using TLZ.OldSite.DB.MongoDB.Model;

namespace TLZ.OldSite.Service.Windows.Monitor.Host
{
    /// <summary>
    /// 服务器性能监控
    /// </summary>
    public class ThreadMonitorService : IDisposable
    {
        #region 属性
        ProcessHelper prohelper;
        LogWritesHelper logHelper;
        Thread threadSecond;
        Thread threadDay;
        Thread threadSystemPing;
        Thread threadProcessCpu;
        #endregion

        #region 构造方法
        public ThreadMonitorService()
        {
            prohelper = new ProcessHelper();
            logHelper = new LogWritesHelper();
        }
        #endregion

        public void MonitorPerformance()
        {
            int interval = 10000;
            //此线程针对10秒一次查询
            threadSecond = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        //系统内存
                        MongoDBHelper.Insert<SystemMemories>(prohelper.GetSystemMemory(), MongoConnType.Project);
                        //系统Cpu
                        MongoDBHelper.Insert<SystemCpu>(prohelper.GetSystemCpu(), MongoConnType.Project);
                        //系统网络
                        MongoDBHelper.Insert<SystemInternet>(prohelper.GetSystemInternet(), MongoConnType.Project);
                        #region 系统网络 方法二
                        //NetInfo netinfo = new NetInfo();
                        //string strNetName;
                        //PerformanceCounter pc;
                        //NetInfo.NetPerformanceStruct netInfoStruct;



                        //List<NetInfo.NetInfoBaseStruct> lNetInfoBase = netinfo.GetNetInfoBase();

                        //strNetName = lNetInfoBase[0].NetName;

                        //#region 输出
                        //Console.Write("网卡名称：" + lNetInfoBase[0].NetName + "\n");
                        //Console.Write("IP地址IPV4：" + lNetInfoBase[0].IPAddress[0] + "\n");
                        //Console.Write("IP地址IPV6：" + lNetInfoBase[0].IPAddress[1] + "\n");
                        //Console.Write("MAC地址：" + lNetInfoBase[0].MACAddress + "\n");
                        //Console.Write("默认网关：" + lNetInfoBase[0].DefaultIPGateway + "\n");
                        //Console.Write("子网掩码：" + lNetInfoBase[0].IPSubnet + "\n");
                        //#endregion
                        //while (true)
                        //{
                        //    netInfoStruct = netinfo.GetNetPerformance(strNetName);
                        //    Console.Write("--------------------------开始---------------------------\n");
                        //    Console.Write("每秒字节总数：" + netInfoStruct.BytesTotal.ToString() + "\n");
                        //    Console.Write("每秒发送字节数：" + netInfoStruct.BytesSent.ToString() + "\n");
                        //    Console.Write("每秒接收字节数：" + netInfoStruct.BytesReceived.ToString() + "\n");
                        //    Console.Write("每秒包总数：" + netInfoStruct.PacketsTotal.ToString() + "\n");
                        //    Console.Write("每秒包发送数：" + netInfoStruct.PacketsSent.ToString() + "\n");
                        //    Console.Write("每秒发送单播包数：" + netInfoStruct.PacketsSentUnicast.ToString() + "\n");
                        //    Console.Write("每秒发送非单播包数：" + netInfoStruct.PacketsSentNonUnicast.ToString() + "\n");
                        //    Console.Write("丢弃的发送包数：" + netInfoStruct.PacketsSentDiscarded.ToString() + "\n");
                        //    Console.Write("错误的发送包数：" + netInfoStruct.PacketsSentErrors.ToString() + "\n");
                        //    Console.Write("每秒包接收数：" + netInfoStruct.PacketsReceived.ToString() + "\n");
                        //    Console.Write("每秒单播包接收数：" + netInfoStruct.PacketsReceivedUnicast.ToString() + "\n");
                        //    Console.Write("每秒非单播包接收总数：" + netInfoStruct.PacketsReceivedNonUnicast.ToString() + "\n");
                        //    Console.Write("丢弃的接收包数：" + netInfoStruct.PacketsReceivedDiscarded.ToString() + "\n");
                        //    Console.Write("错误的接收包数：" + netInfoStruct.PacketsReceivedErrors.ToString() + "\n");
                        //    Console.Write("--------------------------结束---------------------------\n");
                        //    Thread.Sleep(1000);
                        //}
                        #endregion
                        //进程内存
                        var processMemories = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("进程内存设置")), MongoConnType.Center);
                        if (processMemories != null)
                        {
                            var Model = prohelper.GetProcessMemories(processMemories.ProcessName);
                            if (!string.IsNullOrWhiteSpace(Model.ProcessName))
                            {
                                MongoDBHelper.Insert<ProcessMemories>(Model, MongoConnType.Project);
                            }
                        }
                        //进程网络
                        var processInternet = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("进程网络设置")), MongoConnType.Center);
                        if (processInternet != null)
                        {
                            var Model = prohelper.GetProcessInternet(processInternet.ProcessName, interval);
                            if (!string.IsNullOrWhiteSpace(Model.ProcessName))
                            {
                                MongoDBHelper.Insert<ProcessInternet>(Model, MongoConnType.Project);
                            }
                        }
                        Thread.Sleep(interval);
                    }
                    catch (Exception e)
                    {

                        logHelper.Write(new Msg(e.Message, MsgType.Error));
                        logHelper.Dispose();
                    }
                }
            });
            threadSecond.Start();

            //此线程针对一天一次查询
            threadDay = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        //系统硬盘
                        var systemHddList = prohelper.GetSystemHdd();
                        foreach (var item in systemHddList)
                        {
                            MongoDBHelper.Insert<SystemHdd>(item, MongoConnType.Project);
                        }
                        Thread.Sleep(86400000);
                    }
                    catch (Exception e)
                    {
                        logHelper.Write(new Msg(e.Message, MsgType.Error));
                        logHelper.Dispose();
                    }
                }
            });
            threadDay.Start();

            //进程Cpu需要对Cpu一段时间内的运行进行计算，需单独处理
            threadProcessCpu = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var processCpu = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("进程Cpu设置")), MongoConnType.Center);
                        if (processCpu != null)
                        {
                            if (Process.GetProcessesByName(processCpu.ProcessName).Length > 0)
                            {
                                using (var pro = Process.GetProcessesByName(processCpu.ProcessName)[0])
                                {
                                    //上次记录的CPU时间
                                    var prevCpuTime = TimeSpan.Zero;
                                    while (true)
                                    {
                                        //当前时间
                                        var curTime = pro.TotalProcessorTime;
                                        //间隔时间内的CPU运行时间除以逻辑CPU数量
                                        var value = (curTime - prevCpuTime).TotalMilliseconds / interval / Environment.ProcessorCount * 100;
                                        prevCpuTime = curTime;
                                        MongoDBHelper.Insert<ProcessCpu>(new ProcessCpu
                                        {
                                            ProcessName = processCpu.ProcessName,
                                            ProcessUsageRate =Math.Round(value,1),
                                            DateTimeNow = DateTime.Now
                                        }, MongoConnType.Project);
                                        Thread.Sleep(interval);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logHelper.Write(new Msg(e.Message, MsgType.Error));
                        logHelper.Dispose();
                    }
                }
            });

            threadProcessCpu.Start();
            //进程Cpu

        }

        public void MonitorPing()
        {
            string systemAddress = string.Empty;
            if (ConfigurationManager.AppSettings["SystemAddress"] != null)
                systemAddress = ConfigurationManager.AppSettings["SystemAddress"];
            else
                throw new Exception("未配置对应的系统区域AppSetting");

            threadSystemPing = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var databaseModels = MongoDBHelper.Select<DataBaseServerConfigures>(Query.EQ("SystemAddress", BsonValue.Create(systemAddress)), MongoConnType.Center);
                        if (databaseModels.Count > 0)
                        {

                            foreach (var item in databaseModels)
                            {
                                var model = prohelper.GetSystemPing(item.DataBaseName);
                                model.SystemAddress = item.SystemAddress;
                                MongoDBHelper.Insert<SystemPing>("SystemPing" + item.DataBaseName,
                                    model, MongoConnType.Center);
                            }
                        }
                        Thread.Sleep(10000);
                    }
                    catch (Exception e)
                    {
                        logHelper.Write(new Msg(e.Message, MsgType.Error));
                        logHelper.Dispose();
                    }
                }
            });
            threadSystemPing.Start();
        }

        #region Parallel.Invoke 在windowsServices中直接使用会引起超时错误
        //public void Test()
        //{
        //    Parallel.Invoke(
        //        () =>
        //        {
        //            #region 系统内存
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("系统内存设置")), MongoConnType.Center);
        //                    if (dataModel != null)
        //                    {
        //                        MongoDBHelper.Insert<SystemMemories>(prohelper.GetSystemMemory(), MongoConnType.Project);
        //                        Thread.Sleep(dataModel.TntervalSecond);
        //                    }
        //                    else
        //                    {
        //                        MongoDBHelper.Insert<SystemMemories>(prohelper.GetSystemMemory(), MongoConnType.Project);
        //                        Thread.Sleep(10000);
        //                    }
        //                }
        //                catch (Exception e)
        //                {

        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        },
        //        () =>
        //        {
        //            #region 系统Cpu
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("系统Cpu设置")), MongoConnType.Center);
        //                    if (dataModel != null)
        //                    {

        //                        MongoDBHelper.Insert<SystemCpu>(prohelper.GetSystemCpu(), MongoConnType.Project);
        //                        Thread.Sleep(dataModel.TntervalSecond);
        //                    }
        //                    else
        //                    {
        //                        MongoDBHelper.Insert<SystemCpu>(prohelper.GetSystemCpu(), MongoConnType.Project);
        //                        Thread.Sleep(10000);
        //                    }
        //                }
        //                catch (Exception e)
        //                {

        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        },
        //        () =>
        //        {
        //            #region 系统硬盘
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("系统硬盘设置")), MongoConnType.Center);
        //                    var systemHddList = prohelper.GetSystemHdd();
        //                    if (dataModel != null)
        //                    {
        //                        foreach (var item in systemHddList)
        //                        {
        //                            MongoDBHelper.Insert<SystemHdd>(item, MongoConnType.Project);
        //                        }
        //                        Thread.Sleep(dataModel.TntervalSecond);
        //                    }
        //                    else
        //                    {
        //                        foreach (var item in systemHddList)
        //                        {
        //                            MongoDBHelper.Insert<SystemHdd>(item, MongoConnType.Project);
        //                        }
        //                        Thread.Sleep(1000000);
        //                    }
        //                }
        //                catch (Exception e)
        //                {

        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        },
        //        () =>
        //        {
        //            #region 系统网络
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("系统网络设置")), MongoConnType.Center);
        //                    if (dataModel != null)
        //                    {
        //                        MongoDBHelper.Insert<SystemInternet>(prohelper.GetSystemInternet(), MongoConnType.Project);

        //                        Thread.Sleep(dataModel.TntervalSecond);
        //                    }
        //                    else
        //                    {
        //                        MongoDBHelper.Insert<SystemInternet>(prohelper.GetSystemInternet(), MongoConnType.Project);

        //                        Thread.Sleep(1000000);
        //                    }
        //                }
        //                catch (Exception e)
        //                {

        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        },
        //        () =>
        //        {
        //            #region 进程内存
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("进程内存设置")), MongoConnType.Center);
        //                    if (dataModel != null)
        //                    {
        //                        if (!string.IsNullOrWhiteSpace(prohelper.GetProcessMemories(dataModel.ProcessName).ProcessName))
        //                        {
        //                            MongoDBHelper.Insert<ProcessMemories>(prohelper.GetProcessMemories(dataModel.ProcessName), MongoConnType.Project);
        //                            Thread.Sleep(dataModel.TntervalSecond);
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        },
        //        () =>
        //        {
        //            #region 进程Cpu
        //            ActivityConfigures dataModel = null;
        //            while (true)
        //            {
        //                try
        //                {
        //                    dataModel = MongoDBHelper.Get<ActivityConfigures>(Query.EQ("ActivityName", BsonValue.Create("进程Cpu设置")), MongoConnType.Center);
        //                    if (dataModel != null)
        //                    {
        //                        if (!string.IsNullOrWhiteSpace(prohelper.GetProcessCpu(dataModel.ProcessName).ProcessName))
        //                        {
        //                            MongoDBHelper.Insert<ProcessCpu>(prohelper.GetProcessCpu(dataModel.ProcessName), MongoConnType.Project);
        //                            Thread.Sleep(dataModel.TntervalSecond);
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            #endregion
        //        }   //进程网络 暂缓
        //        );
        //}
        #endregion

        public void Dispose()
        {
            threadSecond.Abort();
            threadDay.Abort();
            threadSystemPing.Abort();
            threadProcessCpu.Abort();
        }
    }
}
