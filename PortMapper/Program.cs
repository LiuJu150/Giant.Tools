using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PortMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("PortMapper LocalIP:LocalPort RemoteIP:RemotePort");
                Console.WriteLine("PortMapper 0.0.0.0:80 192.168.1.123:8080");
                return;
            }
            if (args.Length != 2) return;
            if (args[0].Split(':').Length != 2 || args[1].Split(':').Length != 2) return;

            var localArg = args[0].Split(':');
            var localIp = localArg[0];
            var localPort = int.Parse(localArg[1]);

            var remoteArg = args[1].Split(':');
            var remoteIp = remoteArg[0];
            var remotePort = int.Parse(remoteArg[1]);
            Console.WriteLine($"本地端口{args[0]} --> 远程端口 {args[1]}");
            //默认 127.0.0.1：8080 转发到 127.0.0.1：80
            var PortMapperSvc = new PortMapperService(localIp, localPort, remoteIp, remotePort);
            PortMapperSvc.Start();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// user => tcp1 => tcp2
    /// </summary>
    public class PortMapperService
    {
        public int LocalPort { get; set; }
        public string LocalIP { get; set; }
        public int RemotePort { get; set; }
        public string RemoteIP { get; set; }
        private Socket LocalSocket { get; set; }
        public PortMapperService(string localIp, int localPort, string remoteIp, int remotePort)
        {
            this.LocalIP = localIp;
            this.LocalPort = localPort;
            this.RemoteIP = remoteIp;
            this.RemotePort = remotePort;
        }

        public void Start()
        {
            //服务器IP地址  
            IPAddress ip = IPAddress.Parse(LocalIP);
            this.LocalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); ;
            this.LocalSocket.Bind(new IPEndPoint(ip, LocalPort));
            this.LocalSocket.Listen(10000);
            Console.WriteLine("启动监听{0}成功", this.LocalSocket.LocalEndPoint.ToString());
            Thread myThread = new Thread(Listen);
            myThread.Start(this.LocalSocket);
        }
        public void Stop()
        {
            this.LocalSocket.Close();
        }

        //监听客户端连接
        private void Listen(object obj)
        {
            Socket serverSocket = (Socket)obj;
            IPAddress ip = IPAddress.Parse(RemoteIP);
            while (true)
            {
                Socket tcp1 = serverSocket.Accept();
                Socket tcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcp2.Connect(new IPEndPoint(ip, RemotePort));
                //目标主机返回数据
                ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), new SwapSock
                {
                    FromSocket = tcp2,
                    ToSocket = tcp1
                });
                //中间主机请求数据
                ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), new SwapSock
                {
                    FromSocket = tcp1,
                    ToSocket = tcp2
                });
            }
        }
        ///两个 tcp 连接 交换数据，一发一收
        public void SwapMsg(object obj)
        {
            SwapSock mSocket = (SwapSock)obj;
            while (true)
            {
                try
                {
                    byte[] result = new byte[1024];
                    int num = mSocket.ToSocket.Receive(result, result.Length, SocketFlags.None);
                    if (num == 0) //接受空包关闭连接
                    {
                        if (mSocket.FromSocket.Connected)
                        {
                            mSocket.FromSocket.Close();
                        }
                        if (mSocket.ToSocket.Connected)
                        {
                            mSocket.ToSocket.Close();
                        }
                        break;
                    }
                    mSocket.FromSocket.Send(result, num, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (mSocket.FromSocket.Connected)
                    {
                        mSocket.FromSocket.Close();
                    }
                    if (mSocket.ToSocket.Connected)
                    {
                        mSocket.ToSocket.Close();
                    }
                    break;
                }
            }
        }

    }

    public class SwapSock
    {
        public Socket FromSocket { get; set; }
        public Socket ToSocket { get; set; }
    }
}
