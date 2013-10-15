using CommonLib;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace ChatRoomClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket sokConnection = null;
        UserClient myClient{get;set;}
        Thread ShowInfoThread;
        delegate void TbShowHandler(string s);
        TbShowHandler ShowInfoHandler;
        public MainWindow()
        {
            InitializeComponent();
            ShowInfoThread = new Thread(new ThreadStart(Recevie));
            ShowInfoHandler = new TbShowHandler(ShowInfo);
        }

        
        private void BtConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Connect();
                ShowInfoThread.Start();
            }
            catch (Exception ex)
            {

                ShowInfo(ex.Message);
            }
        }

        private void Connect()
        {
            IPAddress ip=IPAddress.Parse(TbIP.Text.Trim());
            IPEndPoint ipep=new IPEndPoint(ip,int.Parse(TbPort.Text.Trim()));
            sokConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                sokConnection.Connect(ipep);
                myClient = new UserClient(sokConnection);
                
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message);                
            }           
        }
        private void Recevie()
        {
            while (true)
            {               
                try
                {
                    byte[] buf = new byte[1024 * 1024 * 5];
                    int length = sokConnection.Receive(buf);
                    string s=MessageAnlysize(buf, length);
                    
                    TbResult.Dispatcher.Invoke(ShowInfoHandler, s);
                }
                catch (Exception ex)
                {
                    TbResult.Dispatcher.Invoke(ShowInfoHandler, ex.Message);
                    break;
                }
            }
        }

        private void ShowInfo(string s)
        {
            TbResult.AppendText(s + "\r\n");
        }

        private void BtSend_Click(object sender, RoutedEventArgs e)
        {
            string s=TbSend.Text.Trim();
            byte[] temp = Encoding.UTF8.GetBytes(s);
            byte[] buf = new byte[temp.Length+3];
            buf[0] = (byte)ProtocolKind.BroadCast;
            buf[1] = 0;
            buf[2] = (byte)myClient.port;
            
            Buffer.BlockCopy(temp, 0, buf, 3, temp.Length);
            sokConnection.Send(buf);
        }
        private string MessageAnlysize(byte[] buf, int length)
        {
            ProtocolKind pk = (ProtocolKind)buf[0];
            int fromId = buf[2];
            string s;
            switch (pk)
            {
                case ProtocolKind.BroadCast:                                  
                    s = fromId.ToString() + "对所有人：" + Encoding.UTF8.GetString(buf, 3, length-3);
                    break;
                case ProtocolKind.SingleChat:                                        
                    s=fromId.ToString()+"对我："+Encoding.UTF8.GetString(buf, 3, length-3);
                    break;
                    
                //case ProtocolKind.FileTrans:
                //    break;
                //case ProtocolKind.BigFileTrans:
                //    break;
                default:
                    s = "";
                    break;
            }
            return s;
        }
        
    }
}
