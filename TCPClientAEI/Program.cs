using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace TCPClientAEI
{
    class Program
    {

        static void Main(string[] args)
        {
            //将此程序优先级设置为BlowNormal优先级
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            while (true)
            {
                try
                {
                    while (true)
                    {
                        RunClient();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"error code: {ex.Message}");
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"错误代码为: {ex.Message}");
                    Thread.Sleep(5000);
                    return;
                }
            }
        }
        //日志监听器变量
        private static DefaultTraceListener listener = null;
        //记录表示当前日期的字符串
        private static string logDate { get { return DateTime.Now.ToString("yyyyMMdd"); } }
        public static void  RunClient()
        {
            Socket socket = null;
            string oldLogTime = null;
            try
            {
                while (true)
                {
                    if (listener == null)
                    {
                        //开启、配置日志文件
                        listener = Trace.Listeners[0] as DefaultTraceListener;
                        if (listener == null)
                        {
                            //如果Listener中没有DefaultTraceListener,则创建一个
                            listener = new DefaultTraceListener();
                            Trace.Listeners.Add(listener);
                        }
                    }
                    if (oldLogTime != logDate)
                    {
                        //设置日志文件名,logDate变量保存上一次日志的日期
                        listener.LogFileName = $"TCPClientAEI{logDate}.log";
                        oldLogTime = logDate;
                        string deleteLogName = $@"{Directory.GetCurrentDirectory()}\TCPServer{DateTime.Now.AddDays(0).ToString("yyyyMMdd")}.log";
                        File.Delete(deleteLogName);
                    }

                    //配置socket套接字
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //配置远程终结点（remote end point）
                    IPAddress hostIP = IPAddress.Parse(Settings2.Default.HostIP);
                    IPEndPoint hostEP = new IPEndPoint(hostIP, Convert.ToInt32(Settings2.Default.HostPort));
                    //连接hostEP
                    //Trace.WriteLine($"\n\r{DateTime.Now}");
                    //Trace.WriteLine("connecting the server...");
                    Console.WriteLine($"{DateTime.Now}");
                    //连接远程主机，并将超时时间设置为10s
                    socket.Connect(hostEP);
                    socket.ReceiveTimeout = Settings2.Default.TimeOut;
                    socket.SendTimeout = Settings2.Default.TimeOut;
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine("server accepted");

                    //接收AEI文件。如果没有接到回应，则重发；最多重发5次
                    for (int i = 0; i < 5; i++)
                    {
                        if ( sendAEI(socket))
                        {
                            break;
                        }
                        Trace.WriteLine($"\n\r{DateTime.Now}");
                        Trace.WriteLine($"receive aei failure for {i + 1} times");
                        if (i == 4)
                        {
                            if (socket != null)
                            {
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Disconnect(true);
                                socket.Close();
                            }
                            return;
                        }
                    }
                    //关闭client
                    if (socket != null)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Disconnect(true);
                        socket.Close();
                    }
                    else
                    {
                        Thread.Sleep(20000);
                        continue;
                    }
                    Thread.Sleep(20000);
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"{DateTime.Now},socket exception:{ex.Message}");
            }
            catch (Exception ex)
            {
                if( ex.Message == "发送路径不存在，请重新配置")
                {
                    throw;
                }
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"restarting service，error message：{ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"服务遇到异常，正在重启...\n\r故障原因：{ex.Message}");
                if (socket != null)
                {
                    //socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                Thread.Sleep(10000);
                return;
            }
        }

        public static bool sendAEI( Socket client)
        {
            try
            {
                //检索各个filePath源文件夹是否有文件存在
                DirectoryInfo dr = new DirectoryInfo(Settings2.Default.sendFilePath);
                if (!dr.Exists)
                {
                    Console.WriteLine($"发送文件路径{Settings2.Default.sendFilePath}不存在,请重新配置");
                    Trace.WriteLine($"发送文件路径{Settings2.Default.sendFilePath}不存在，请重新配置");
                    throw new Exception($"发送路径不存在，请重新配置");
                    //return false;
                }
                FileInfo[] files = dr.GetFiles("*.AEI", SearchOption.AllDirectories);
                //如果该文件夹内有内容，依次把每个文件发送出去
                //先发送文件数量
                byte[] fileNum = new byte[5];
                fileNum = Encoding.UTF8.GetBytes(files.Length.ToString("D5"));
                client.Send(fileNum);
                //提示将要发送的文件种类和数量
                if (files.Length != 0)
                {
                    Console.WriteLine($"{DateTime.Now}");
                    Console.WriteLine($"将要发送{files.Length}个AEI文件");
                }
                foreach (FileInfo file in files)
                {
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    //打开文件
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fs.Read(fileContent, 0, (int)file.Length);
                    }
                    finally
                    {//关闭流
                        if( fs != null)
                        {
                             fs.Close();
                        }
                    }

                    //服务器端用来决定存放该文件路径的标志位（1位）
                    byte[] fileFolderFlag = new byte[1];
                    fileFolderFlag = Encoding.UTF8.GetBytes("1");
                    //文件名的长度
                    byte[] nameLength = Encoding.UTF8.GetBytes(file.Name.Length.ToString("D2"));
                    //文件名
                    byte[] name = Encoding.UTF8.GetBytes(file.Name);
                    //文件长度
                    byte[] length = Encoding.UTF8.GetBytes(file.Length.ToString("D10"));
                    //发送内容
                    byte[] send = new byte[nameLength.Length + name.Length + length.Length + fileContent.Length + 1];
                    //将五部分内容都复制到send中，一起发送
                    //public static void Copy(Array sourceArray,int sourceIndex,Array destinationArray,int destinationIndex,int length)
                    Array.Copy(fileFolderFlag, 0, send, 0, 1);
                    Array.Copy(nameLength, 0, send, 1, nameLength.Length);
                    Array.Copy(name, 0, send, nameLength.Length + 1, name.Length);
                    Array.Copy(length, 0, send, nameLength.Length + name.Length + 1, length.Length);
                    Array.Copy(fileContent, 0, send, nameLength.Length + name.Length + length.Length + 1, fileContent.Length);
                    //发送信息，并在成功后删除源文件
                    client.Send(send);
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                    //Console.WriteLine($"\n\r{DateTime.Now}");
                    //Console.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                }
                if (files.Length != 0)
                {
                    foreach (FileInfo file in files)
                    {
                        Console.WriteLine($"\n\r{DateTime.Now}");
                        Console.WriteLine($"已发送AEI文件{file.Name}个");
                        Trace.WriteLine($"\n\r{DateTime.Now}");
                        Trace.WriteLine($"send AEI files complete. file number: {file.Name}");
                    }
                }
                Trace.WriteLine($"\n\r{DateTime.Now}");
                //删除发送的文件，返回成功
                foreach ( FileInfo file in files)
                {
                    file.Delete();
                    Trace.WriteLine($"delete file: {file.Name}");
                }
                return true;
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file socket error");
                Trace.WriteLine($"error code :{ex.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"send file socket error");
                //Console.WriteLine($"error code :{ex.Message}");
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file: {ex.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"send file: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file failed.\n\rerror code: {ex.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"send file failed.\n\rerror code: {ex.Message}");
                return false;
            }
        }
    }
}
