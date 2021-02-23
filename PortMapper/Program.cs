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

            Console.WriteLine($"本地端口{args[0]} --> 远程端口 {args[1]}");

            var config = new MapperConfig();
            var localArg = args[0].Split(':');
            config.LocalIP = localArg[0];
            config.LocalPort = int.Parse(localArg[1]);

            var remoteArg = args[1].Split(':');
            config.RemoteIP = remoteArg[0];
            config.RemotePort = int.Parse(remoteArg[1]);
            var PortMapperSvc = new PortMapperService(config);
            PortMapperSvc.Start();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// user => tcp1 => tcp2
    /// </summary>
    public class PortMapperService
    {
        public MapperConfig Config { get; set; }
        private Socket LocalSocket { get; set; }
        public PortMapperService(MapperConfig config)
        {
            this.Config = config;
        }

        public void Start()
        {
            //服务器IP地址  
            IPAddress ip = IPAddress.Parse(this.Config.LocalIP);
            this.LocalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); ;
            this.LocalSocket.Bind(new IPEndPoint(ip, this.Config.LocalPort));
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
            IPAddress ip = IPAddress.Parse(this.Config.RemoteIP);
            while (true)
            {
                Socket tcp1 = serverSocket.Accept();
                Socket tcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcp2.Connect(new IPEndPoint(ip, this.Config.RemotePort));
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
    /// <summary>
    /// 映射配置
    /// </summary>
    public class MapperConfig
    {
        /// <summary>
        /// 本地IP
        /// </summary>
        public string LocalIP { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int LocalPort { get; set; }
        /// <summary>
        /// 远程IP
        /// </summary>
        public string RemoteIP { get; set; }
        /// <summary>
        /// 远程端口
        /// </summary>
        public int RemotePort { get; set; }
    }
}
