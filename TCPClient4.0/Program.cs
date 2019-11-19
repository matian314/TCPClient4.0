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
using System.Reflection;
using TADS文件传输客户端;

namespace TADS文件传输客户端
{
    class Program
    {
        static void Main(string[] args)
        {
            //将此程序优先级设置为BlowNormal优先级
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            Console.Title = "TADS文件传输客户端";
            while (true)
            {
                try
                {
                    while (true)
                    {
                        RunTcpClient();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"error code: {ex.Message}");
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"错误代码为: {ex.Message}");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                    Process.GetCurrentProcess().Kill();
                    return;
                }
            }
        }
        private static DefaultTraceListener listener = null;
        private static string logDate { get { return DateTime.Now.ToString("yyyyMMdd"); } }
        private static void RunTcpClient()
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
                        listener.LogFileName = $"TCPClient{logDate}.log";
                        oldLogTime = logDate;
                        string deleteLogName = $@"{Directory.GetCurrentDirectory()}\TCPServer{DateTime.Now.AddDays(0).ToString("yyyyMMdd")}.log";
                        File.Delete(deleteLogName);
                    }

                    //配置socket套接字
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //配置远程终结点（remote end point）
                    IPAddress hostIP = IPAddress.Parse(Settings.Default.HostIP);
                    IPEndPoint hostEP = new IPEndPoint(hostIP, Convert.ToInt32(Settings.Default.HostPort));
                    //绑定本地IP地址
                    //连接hostEP
                    //Trace.WriteLine($"\n\r{DateTime.Now}");
                    //Trace.WriteLine("connecting the server...");
                    //连接远程主机，并配置超时时间
                    socket.Connect(hostEP);
                    socket.ReceiveTimeout = Settings.Default.TimeOut;
                    socket.SendTimeout = Settings.Default.TimeOut;
                    //Trace.WriteLine($"\n\r{DateTime.Now}");
                    //Trace.WriteLine("server accepted");

                    //接收车号。如果没有接到回应，则重发；最多重发5次
                    for (int i = 0; i < 5; i++)
                    {
                        if (ReceiveTrainID(socket))
                        {
                            break;
                        }
                        //Console.WriteLine($"\n\r{DateTime.Now}");
                        //Console.WriteLine($"receive train number failure for {i + 1} times");
                        Trace.WriteLine($"\n\r{DateTime.Now}");
                        Trace.WriteLine($"receive train number failure for {i + 1} times");
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

                    //接收发送过来的文件
                    //如果接收不成功，重新发送
                    for (int i = 0; i < 5; i++)
                    {
                        if (SendFile(socket))
                        {
                            break;
                        }
                        Trace.WriteLine($"\n\r{DateTime.Now}");
                        Trace.WriteLine($"send file failure for {i + 1} times");
                        //Console.WriteLine($"\n\r{DateTime.Now}");
                        //Console.WriteLine($"send file failure for {i + 1} times");
                        //如果第五次都没有接收成功，直接退出
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
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"服务遇到异常，正在重启...\n\r故障原因：{ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"服务遇到异常，正在重启...\n\r故障原因：{ex.Message}");
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Process.GetCurrentProcess().Kill();

                return;
            }
        }
        public static bool SendFile(Socket client)
        {
            //变量声明
            //接收信息的配置
            try
            {
                //检索各个filePath源文件夹是否有文件存在
                DirectoryInfo dr_FS = new DirectoryInfo( Settings.Default.sendFSFilePath);
                DirectoryInfo dr_Sound = new DirectoryInfo( Settings.Default.sendSoundFilePath);
                DirectoryInfo dr_Error = new DirectoryInfo(Settings.Default.sendErrorFilePath);
                string datFilePath = Settings.Default.sendDatFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\" + oldTrainID + @"\FS";
                if( !Directory.Exists( datFilePath))
                {
                    Directory.CreateDirectory(datFilePath);
                }
                DirectoryInfo dr_Dat = new DirectoryInfo(datFilePath); 
                FileInfo[] files_FS = dr_FS.GetFiles("*.FS",SearchOption.AllDirectories);
                FileInfo[] files_Sound1 = dr_Sound.GetFiles("*.axl", SearchOption.AllDirectories);
                FileInfo[] files_Sound2 = dr_Sound.GetFiles("*.wav", SearchOption.AllDirectories);
                FileInfo[] files_Error = dr_Error.GetFiles("*.axl", SearchOption.AllDirectories);
                FileInfo[] files_Dat = dr_Dat.GetFiles("*.dat", SearchOption.TopDirectoryOnly);
                //如果该文件夹内有内容，依次把每个文件发送出去
                //先发送文件数量
                byte[] fileNum = new byte[5];
                fileNum = Encoding.UTF8.GetBytes((files_FS.Length + files_Sound1.Length + files_Sound2.Length + files_Error.Length + files_Dat.Length).ToString("D5"));
                client.Send(fileNum);
                //提示将要发送的文件种类和数量
                if(files_FS.Length + files_Sound1.Length + files_Sound2.Length + files_Error.Length + files_Dat.Length != 0)
                {
                    if(files_FS.Length != 1)
                    {
                        Console.WriteLine($"{DateTime.Now}");
                        Console.WriteLine($"将要发送{files_FS.Length}个.FS报文，{files_Sound1.Length}个.axl声音文件，{files_Error.Length}个故障轴承声音文件，{files_Dat.Length}个数据文件,{files_Sound2.Length}个.wav声音文件");
                        Trace.WriteLine($"{DateTime.Now}");
                        Trace.WriteLine($"going to send {files_FS.Length} .FS files，{files_Sound1.Length} axl sound files，{files_Error.Length} error axle files，{files_Dat.Length} data files，{files_Sound2.Length} wav sound files");
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now}");
                        Console.WriteLine($"将要{files_FS[0].Name}报文，{files_Sound1.Length}个.axl声音文件，{files_Error.Length}个故障轴承声音文件，{files_Dat.Length}个数据文件,{files_Sound2.Length}个.wav声音文件");
                        Trace.WriteLine($"{DateTime.Now}");
                        Trace.WriteLine($"going to send file: {files_FS[0].Name}，{files_Sound1.Length} axl sound files，{files_Error.Length} error axle files，{files_Dat.Length} data files，{files_Sound2.Length} wav sound files");
                    }
 
                }


                foreach (FileInfo file in files_FS)
                {
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    //打开文件
                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Read(fileContent, 0, (int)file.Length);
                    fs.Close();
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
                    byte[] send = new byte[nameLength.Length + name.Length + length.Length + fileContent.Length +1];
                    //将五部分内容都复制到send中，一起发送
                    //public static void Copy(Array sourceArray,int sourceIndex,Array destinationArray,int destinationIndex,int length)
                    Array.Copy(fileFolderFlag, 0, send,0,1);
                    Array.Copy(nameLength, 0, send, 1, nameLength.Length);
                    Array.Copy(name, 0, send, nameLength.Length+1, name.Length);
                    Array.Copy(length, 0, send, nameLength.Length + name.Length+1, length.Length);
                    Array.Copy(fileContent, 0, send, nameLength.Length + name.Length + length.Length+1, fileContent.Length);
                    //发送信息，并在成功后删除源文件
                    client.Send(send);
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                    //Console.WriteLine($"\n\r{DateTime.Now}");
                    //Console.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                }
                if (files_FS.Length != 0)
                {
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"已发送.FS文件{files_FS.Length}个");
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send .FS files complete. file number: {files_FS.Length}");
                }

                foreach (FileInfo file in files_Sound1)
                {
                    //打开文件
                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    fs.Read(fileContent, 0, (int)file.Length);
                    fs.Close();
                    //服务器端用来决定存放该文件路径的标志位（1位）
                    byte[] fileFolderFlag = new byte[1];
                    fileFolderFlag = Encoding.UTF8.GetBytes("2");
                    //文件名的长度
                    byte[] nameLength = Encoding.UTF8.GetBytes(file.Name.Length.ToString("D2"));
                    //文件名
                    byte[] name = Encoding.UTF8.GetBytes(file.Name);
                    //文件长度
                    byte[] length = Encoding.UTF8.GetBytes(file.Length.ToString("D10"));
                    //发送内容
                    byte[] send = new byte[nameLength.Length + name.Length + length.Length + fileContent.Length +1];
                    //将五部分内容都复制到send中，一起发送
                    //public static void Copy(Array sourceArray,int sourceIndex,Array destinationArray,int destinationIndex,int length)
                    Array.Copy(fileFolderFlag, 0, send, 0, 1);
                    Array.Copy(nameLength, 0, send, 1, nameLength.Length);
                    Array.Copy(name, 0, send, nameLength.Length+1, name.Length);
                    Array.Copy(length, 0, send, nameLength.Length + name.Length+1, length.Length);
                    Array.Copy(fileContent, 0, send, nameLength.Length + name.Length + length.Length+1, fileContent.Length);
                    //发送信息，并在成功后删除源文件
                    client.Send(send);
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                    //Console.WriteLine($"\n\r{DateTime.Now}");
                    //Console.WriteLine($"send file name{file.FullName},file length:{ file.Length / 1000 }KB");
                }
                if (files_Sound1.Length  != 0)
                {
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"已发送.aei声音文件{files_Sound1.Length}个");
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send .aei files complete. file number: {files_Sound1.Length}");
                }
                foreach (FileInfo file in files_Sound2)
                {
                    //打开文件
                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    fs.Read(fileContent, 0, (int)file.Length);
                    fs.Close();
                    //服务器端用来决定存放该文件路径的标志位（1位）
                    byte[] fileFolderFlag = new byte[1];
                    fileFolderFlag = Encoding.UTF8.GetBytes("5");
                    //文件名的长度
                    byte[] nameLength = Encoding.UTF8.GetBytes(file.Name.Length.ToString("D2"));
                    //文件名
                    byte[] name = Encoding.UTF8.GetBytes(file.Name);
                    //文件长度
                    byte[] length = Encoding.UTF8.GetBytes(file.Length.ToString("D10"));
                    //发送内容
                    byte[] send = new byte[nameLength.Length + name.Length + length.Length + fileContent.Length +1];
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
                if (files_Sound2.Length != 0)
                {
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"已发送.wav声音文件{files_Sound2.Length}个");
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send .wav files complete. file number: {files_Sound2.Length}");
                }
                foreach (FileInfo file in files_Error)
                {
                    //打开文件
                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    fs.Read(fileContent, 0, (int)file.Length);
                    fs.Close();
                    //服务器端用来决定存放该文件路径的标志位（1位）
                    byte[] fileFolderFlag = new byte[1];
                    fileFolderFlag = Encoding.UTF8.GetBytes("3");
                    //文件名的长度
                    byte[] nameLength = Encoding.UTF8.GetBytes(file.Name.Length.ToString("D2"));
                    //文件名
                    byte[] name = Encoding.UTF8.GetBytes(file.Name);
                    //文件长度
                    byte[] length = Encoding.UTF8.GetBytes(file.Length.ToString("D10"));
                    //发送内容
                    byte[] send = new byte[nameLength.Length + name.Length + length.Length + fileContent.Length +1];
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
                if (files_Error.Length != 0)
                {
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"已发送故障轴承声音文件{files_Error.Length}个");
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send error axle files complete. file number: {files_Error.Length}");
                }
                foreach (FileInfo file in files_Dat)
                {
                    //打开文件
                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //读取文件内容
                    byte[] fileContent = new byte[(int)file.Length];
                    fs.Read(fileContent, 0, (int)file.Length);
                    fs.Close();
                    //服务器端用来决定存放该文件路径的标志位（1位）
                    byte[] fileFolderFlag = new byte[1];
                    fileFolderFlag = Encoding.UTF8.GetBytes("4");
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
                if (files_Dat.Length != 0)
                {
                    Console.WriteLine($"\n\r{DateTime.Now}");
                    Console.WriteLine($"已发送dat数据文件{files_Dat.Length}个");
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"send dat files complete. file number: {files_Dat.Length}");
                }
                if (ReceiveConfirm(client))
                {//如果有文件要删除，进行记录
                    if(files_FS.Length + files_Sound1.Length + files_Sound2.Length + files_Error.Length + files_Dat.Length != 0)
                    {
                        Trace.WriteLine($"\n\r{DateTime.Now}\n\rdelete all the files");
                    }
                    //如果收到确认，删除发送的文件，返回成功
                    foreach (FileInfo file in files_Sound1)
                    {
                        file.Delete();
                        //Trace.WriteLine($"delete file: {file.Name}");
                    }
                    foreach(FileInfo file in files_FS)
                    {
                        file.Delete();
                        //Trace.WriteLine($"delete file: {file.Name}");
                    }
                    foreach (FileInfo file in files_Dat)
                    {
                        file.Delete();
                        //Trace.WriteLine($"delete file: {file.Name}");
                    }
                    foreach (FileInfo file in files_Error)
                    {
                        file.Delete();
                        //Trace.WriteLine($"delete file: {file.Name}");
                    }
                    foreach (FileInfo file in files_Sound2)
                    {
                        file.Delete();
                        //Trace.WriteLine($"delete file: {file.Name}");
                    }
                    return true;
                }
                else
                {
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine("server receive file failed\n\rgoing to send again");
                    return false;
                }
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file socket error");
                Trace.WriteLine($"error code :{ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"send file socket error");
                Console.WriteLine($"error code :{ex.Message}");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Process.GetCurrentProcess().Kill();
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file: {ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"send file: {ex.Message}");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Process.GetCurrentProcess().Kill();
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send file failed.\n\rerror code: {ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"send file failed.\n\rerror code: {ex.Message}");
                return false;
            }
        }

        private static string oldTrainID;
        private static bool ReceiveTrainID(Socket socket)
        {
            FileStream fs = null;
            StreamWriter ws = null;
            try
            {
                //接收车号信息，固定车号为11位
                byte[] revBuffer = new byte[11];
                socket.Receive(revBuffer, 11, SocketFlags.None);
                string TrainID = Encoding.UTF8.GetString(revBuffer);
                if (oldTrainID == TrainID)
                {
                    SendConfirm(socket, true);
                    return true;
                }
                oldTrainID = TrainID;

                //获取接收车号的文件路径,将新车号写入
                string filePath = Settings.Default.clientTrainIDPath;
                fs = new FileStream(filePath, FileMode.Create);
                ws = new StreamWriter(fs);
                ws.Write(TrainID);
                ws.Flush();
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"更新过车序列号（trainID）:{TrainID}");
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"update trainID:{TrainID}");
                SendConfirm(socket, true);
                return true;
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive trainID socket error");
                Trace.WriteLine($"error code :{ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"receive trainID socket error");
                Console.WriteLine($"error code :{ex.Message}");
                //关闭流
                if (ws != null)
                {
                    ws.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Process.GetCurrentProcess().Kill();
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive trainID: {ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"receive trainID: {ex.Message}");

                Thread.Sleep(TimeSpan.FromSeconds(10));
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Process.GetCurrentProcess().Kill();
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive trainID failed.\n\rerror code: {ex.Message}");
                Console.WriteLine($"\n\r{DateTime.Now}");
                Console.WriteLine($"receive trainID failed.\n\rerror code: {ex.Message}");
                return false;
            }
            finally
            {
                //关闭流
                if (ws != null )
                {
                    ws.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private static bool sendTrainID(Socket socket)
        {
            //读取车号信息，车号为11位
            byte[] sendBuffer = new byte[11];
            //通过读写车号文件，获取trainID
            string filePath = Settings.Default.clientTrainIDPath;
            FileStream fs = null;
            StreamReader ws = null;
            string trainID;
            try
            {
                fs = new FileStream(filePath, FileMode.Open);
                ws = new StreamReader(fs);
                trainID = ws.ReadLine();
                //发送trainID值
                sendBuffer = Encoding.UTF8.GetBytes(trainID);
                socket.Send(sendBuffer);
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send trainID: {trainID}");

                return ReceiveConfirm(socket);
            }
            catch (FileNotFoundException)
            {
                Trace.WriteLine($"send trainID failure:\n\rfile: {filePath} not founde");
                Console.WriteLine($"send trainID failure:\n\rfile: {filePath} not founde");
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"send trainID failure:\n\rerror code: {ex.Message}");
                Console.WriteLine($"send trainID failure:\n\rerror code: {ex.Message}");
                return false;
            }
            finally
            {//关闭流
                if (ws != null)
                {
                    ws.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static bool ReceiveFile(Socket client)
        {
            //字节数组，一次接收数据的长度为 1024 字节  
            byte[] receiveBuffer = new byte[1024];

            //接收信息的配置
            string filePath = Settings.Default.receiveFSFilePath;
            FileStream fs = null;
            try
            {

                //接收头部文件，读取字节数是根据配置文件中的Settings.Default.receivefileNameLength属性
                //函数原型public int Receive(byte[] buffer,int size,SocketFlags socketFlags)
                //返回值为接收到的字节数
                //首先确定接收要接收多少个文件
                client.Receive(receiveBuffer, 5, SocketFlags.None);
                int fileNumber = Int32.Parse(Encoding.UTF8.GetString(receiveBuffer, 0, 5));
                for (int i = 0; i < fileNumber; i++)
                {
                    //变量声明
                    //返回本次接收内容的字节数  
                    int bytes = 0;
                    //文件的长度，不包含报文的头部体
                    int fileLength = 0;
                    //已经接受的字节数
                    int receivedLength = 0;
                    //最先发送的2个字节代表文件名的长度（不足10用0补齐）
                    bytes = client.Receive(receiveBuffer, 2, SocketFlags.None);
                    int fileNameLength = Int32.Parse(Encoding.UTF8.GetString(receiveBuffer, 0, 2));
                    //根据前两个字节代表文件名长度接收文件名
                    bytes = client.Receive(receiveBuffer, fileNameLength, SocketFlags.None);
                    string fileName = Encoding.UTF8.GetString(receiveBuffer, 0, bytes);
                    //如果提供的文件名和正则表达式不匹配，则返回失败

                    //创建文件流，然后让文件流来根据路径创建一个文件
                    //Create模式是创建或覆盖
                    fs = new FileStream(filePath + fileName, FileMode.Create);

                    //接收文件长度，格式为D10,单个文件最长为2^10字节（2GB）

                    bytes = client.Receive(receiveBuffer, 10, 0);
                    //将读取的字节数转换为长度
                    fileLength = Convert.ToInt32(Encoding.UTF8.GetString(receiveBuffer, 0, bytes));

                    //接收正式的文件内容
                    while (receivedLength < fileLength)
                    {//如果要接收的内容一次接收不完，就接收1024字节
                     //否则就接收全部剩余的字节
                        if (receivedLength + receiveBuffer.Length <= fileLength)
                        {
                            bytes = client.Receive(receiveBuffer, receiveBuffer.Length, 0);
                            receivedLength += bytes;
                            fs.Write(receiveBuffer, 0, bytes);
                        }
                        else
                        {
                            bytes = client.Receive(receiveBuffer, fileLength - receivedLength, 0);
                            receivedLength += bytes;
                            fs.Write(receiveBuffer, 0, bytes);
                        }
                    }
                    //保存接收的内容
                    fs.Flush();
                    Trace.WriteLine($"\n\r{DateTime.Now}");
                    Trace.WriteLine($"receiving file: {fileName}");
                    Trace.WriteLine($"file length: { fileLength / 1000.0}KB");
                    //Console.WriteLine($"\n\r{DateTime.Now}");
                    //Console.WriteLine($"receiving file: {fileName}");
                    //Console.WriteLine($"file length: { fileLength / 1000.0}KB");
                }
                SendConfirm(client, true);
                return true;
            }
            catch (SocketException e)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive file socket exception");
                Trace.WriteLine($"error message: {e.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"receive file socket exception");
                //Console.WriteLine($"error message: {e.Message}");
                throw;
            }
            catch (ObjectDisposedException e)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive file socket exception");
                Trace.WriteLine($"error message: {e.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"receive file socket exception");
                //Console.WriteLine($"error message: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive file failed");
                Trace.WriteLine($"error message: {e.Message}");
                //Console.WriteLine($"\n\r{DateTime.Now}");
                //Console.WriteLine($"receive file failed");
                //Console.WriteLine($"error message: {e.Message}");
                SendConfirm(client, false);
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private static bool ReceiveConfirm(Socket socket)
        {
            try
            {
                byte[] confirm = new byte[1];
                socket.Receive(confirm, 1, SocketFlags.None);
                return (Encoding.UTF8.GetString(confirm) == "1");
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive confirm socket error");
                Trace.WriteLine($"error code :{ex.Message}");
                throw;
            }
            catch (ObjectDisposedException)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive confirm: socket closed");
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"receive check failure. error code: {ex.Message}");
                return false;
            }
        }

        private static void SendConfirm(Socket socket, bool isSuccess)
        {//确认信息，1是成功，0是失败
            try
            {
                byte[] sendBuffer = new byte[1];
                if (isSuccess)
                {
                    sendBuffer = Encoding.UTF8.GetBytes("1");
                }
                else
                {
                    sendBuffer = Encoding.UTF8.GetBytes("0");
                }
                socket.Send(sendBuffer);
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send confirm socket error");
                Trace.WriteLine($"error code :{ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"\n\r{DateTime.Now}");
                Trace.WriteLine($"send check failure. error code: {ex.Message}");
                return;
            }

        }
    }
}
