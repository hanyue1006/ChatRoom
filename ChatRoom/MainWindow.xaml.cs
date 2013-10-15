using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatRoom
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket sokWelcome=null;
        Dictionary<int, UserClient> dictConnection;
        Thread ThreadLinsening;
        Thread[] ThreadRec;
        const int MaxUsers = 50;        
        delegate void TbShowHandler(string s);
        int ClientPort { get; set; }
        TbShowHandler ShowInfoHandler;
        int connectNum;
        
        public MainWindow()
        {
            InitializeComponent();
            ThreadLinsening = new Thread(new ThreadStart(Linsening));            
            ThreadLinsening.IsBackground = true;            
            ShowInfoHandler = new TbShowHandler(ShowInfo);
            dictConnection = new Dictionary<int, UserClient>();
            ThreadRec = new Thread[MaxUsers];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLinsen();
                ThreadLinsening.Start();
                ShowInfo("开始监听");
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message);
            }
        }             

        private void StartLinsen()
        {
            IPAddress ip = IPAddress.Parse(TbIP.Text.Trim());
            IPEndPoint ipep = new IPEndPoint(ip, int.Parse(TbPort.Text.Trim()));
            sokWelcome = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sokWelcome.Bind(ipep);
            sokWelcome.Listen(10);
        }
        #region 监听客户端并创建链接
        /// <summary>
        /// 监听线程
        /// </summary>
        private void Linsening()
        {
            while (true)
            {                
                UserClient userClient=new UserClient(sokWelcome.Accept());
                Thread.Sleep(100);
                connectNum++;
                dictConnection.Add(userClient.port, userClient);
                ThreadRec[connectNum] = new Thread(new ParameterizedThreadStart(Recevie));
                ThreadRec[connectNum].IsBackground = true;
                ThreadRec[connectNum].Start(userClient.port);
                TbResult.Dispatcher.Invoke(ShowInfoHandler, "Connected " + dictConnection[userClient.port].ipep.ToString());
            }
        }       
        #endregion
        
        private void ShowInfo(string s)
        {
            TbResult.AppendText(s + "\r\n");
        }

        private void BtSend_Click(object sender, RoutedEventArgs e)
        {
            byte[] temp = Encoding.UTF8.GetBytes(TbSend.Text.Trim());
            byte[] buf=new byte[temp.Length+3];
            buf[0] = (byte)ProtocolKind.BroadCast;
            buf[1] = 0;
            buf[2] = 0;

            Buffer.BlockCopy(temp, 0, buf, 3, temp.Length);
            foreach (var item in dictConnection)
            {
                item.Value.sokConnection.Send(buf);
            }
            
        }

        /// <summary>
        /// 接收线程
        /// </summary>
        /// <param name="port">链接的ID号</param>
        private void Recevie(object port)
        {
            int portID = (int)port;
            while (true)
            {
                try
                {                    
                    byte[] buf = new byte[1024 * 1024 * 5];
                    int length = dictConnection[portID].sokConnection.Receive(buf);
                    byte[] bf = new byte[length];
                    Buffer.BlockCopy(buf, 0, bf, 0, length);
                    if (length>0)
                    {
                        ClientPort = portID;
                        string s = Encoding.UTF8.GetString(buf, 3, length-3);
                        
                        
                        TbResult.Dispatcher.Invoke(ShowInfoHandler, dictConnection[ClientPort].ipep.ToString() + "：" + s);
                        MessageAnlysize(bf, length);                        
                    }
                }
                catch (Exception ex)
                {
                    TbResult.Dispatcher.Invoke(ShowInfoHandler, ex.Message);
                    break;
                }
            }
        }
        
        private void MessageAnlysize(byte[] buf,int length)
        {
            ProtocolKind pk = (ProtocolKind)buf[0];                        
            switch (pk)
	        {
                case ProtocolKind.BroadCast:
                    {
                        foreach (var item in dictConnection)
                        {
                            item.Value.sokConnection.Send(buf);
                        }                        
                        break;  
                    }
                case ProtocolKind.SingleChat:
                    {
                        int singleChatId = buf[1];
                        dictConnection[singleChatId].sokConnection.Send(buf);
                        break;
                    }
                default:
                    {
                        break;
                    }
	        }
            
        }




        
    }
}
