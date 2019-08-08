using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApplication4
{
    public partial class Window1 : Window
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);
        public Window1()
        {
            string file_JO = System.AppDomain.CurrentDomain.BaseDirectory + "JoinDomain.dll";
            if (Environment.UserName == "Administrator")
            {
                if (!File.Exists(file_JO))
                {
                    MessageBox.Show("老板，麻烦把JoinDomain.dll文件和本程序放同一个目录下。");
                    Environment.Exit(0);
                }
                else
                {
                    if (!InternetGetConnectedState(0, 0))
                    {
                        MessageBox.Show("没有查找到有效的网卡！","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                MessageBox.Show("老板，麻烦用Administrator用户登录安装。");
                Environment.Exit(0);
            }
        }
        public static string GetClientLocalIPv4Address()//获取IPV4的地址
        {
            string strLocalIP = string.Empty;
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                for (int i = 0; i < ipHost.AddressList.Length; i++)
                {
                    if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        strLocalIP = ipHost.AddressList[i].ToString();
                    }
                }
                return strLocalIP;
            }
            catch(Exception ex)
            {
                MessageBox.Show("获取IPV4的地址出错，信息："+ex.Message);
                return "";
            }
        }
        public static void SetIPAddress(string[] ip, string[] submask, string[] getway, string[] dns)//设置IP地址，掩码，网关和DNS
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            foreach (ManagementObject mo in moc)
            {
                //如果没有启用IP设置的网络设备则跳过
                if (!(bool)mo["IPEnabled"])
                    continue;

                //设置IP地址和掩码
                if (ip != null && submask != null)
                {
                    inPar = mo.GetMethodParameters("EnableStatic");
                    inPar["IPAddress"] = ip;
                    inPar["SubnetMask"] = submask;
                    outPar = mo.InvokeMethod("EnableStatic", inPar, null);
                }

                //设置网关地址
                if (getway != null)
                {
                    inPar = mo.GetMethodParameters("SetGateways");
                    inPar["DefaultIPGateway"] = getway;
                    outPar = mo.InvokeMethod("SetGateways", inPar, null);
                }

                //设置DNS地址
                if (dns != null)
                {
                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = dns;
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                }
            }
        }
        public static string getMacAddr_Local()//获取MAC地址，且转换：至-
        {
            string madAddr = null;
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    if (Convert.ToBoolean(mo["IPEnabled"]) == true)
                    {
                        madAddr = mo["MacAddress"].ToString();
                        madAddr = madAddr.Replace(':', '-');
                    }
                    mo.Dispose();
                }
                if (madAddr == null)
                {
                    return "unknown";
                }
                else
                {
                    return madAddr;
                }
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
        public static bool IsPingIP(string strIP)//判断地址是否通
        {
            try
            {
                //创建Ping对象
                Ping ping = new Ping();
                //接受Ping返回值
                PingReply reply = ping.Send(strIP, 3000);
                //Ping通
                return true;
            }
            catch
            {
                //Ping失败
                return false;
            }
        }
        private string GetGateway()
        {
            //网关地址
            string strGateway = "";
            //获取所有网卡
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //遍历数组
            foreach (var netWork in nics)
            {
                //单个网卡的IP对象
                IPInterfaceProperties ip = netWork.GetIPProperties();
                //获取该IP对象的网关
                GatewayIPAddressInformationCollection gateways = ip.GatewayAddresses;
                foreach (var gateWay in gateways)
                {
                    //如果能够Ping通网关
                    if (IsPingIP(gateWay.Address.ToString()))
                    {
                        //得到网关地址
                        strGateway = gateWay.Address.ToString();
                        //跳出循环
                        break;
                    }
                }

                //如果已经得到网关地址
                if (strGateway.Length > 0)
                {
                    //跳出循环
                    break;
                }
            }

            //返回网关地址
            return strGateway;
        }
        public string get_ip()//获取输入框的IP
        {
            if (ip0.Text == null || ip1.Text == null || ip2.Text == null || ip3.Text == null)
            {
                return null;
            }
            else
            {
                string ipaddr = ip0.Text.Trim() + '.' + ip1.Text.Trim() + '.' + ip2.Text.Trim() + '.' + ip3.Text.Trim();
                return ipaddr;
            }
        }
        public string get_Gateway()//获取输入框的网关
        {
            if (Gateway0.Text == null || Gateway1.Text == null || Gateway2.Text == null || Gateway3.Text == null)
            {
                return null;
            }
            else
            {
                string Gateway = Gateway0.Text.Trim() + '.' + Gateway1.Text.Trim() + '.' + Gateway2.Text.Trim() + '.' + Gateway3.Text.Trim();
                return Gateway;
            }
        }

        private void textChange(object sender, TextChangedEventArgs e)//文本输入框的算法
        {
            if (assignment)//判断全局变量是否处于赋值状态
            {
                TextBox tb = (TextBox)sender;
                Regex rgx = new Regex(@"^(2[0-4]\d|25[0-5]|[0|1]?\d\d?|(2[0-4]\d|25[0-5]|[0|1]?\d\d?\.))$");
                string ss = tb.Text;
                string ss1 = "";
                if (ss.Length > 0)
                {
                    ss1 = ss.Remove(ss.Length - 1, 1);
                }
                if (ss == "0")
                {
                    tb.Text = "";
                }
                if (!rgx.IsMatch(ss))
                {
                    tb.Text = ss1;
                    tb.Select(tb.Text.Length, 0);
                }
                else
                {
                    int temp = ss.IndexOf(".");
                    if (temp > -1 && sender != this.Gateway3)
                    {
                        ss = ss.Replace(".", "");
                        tb.Text = ss;
                        FrameworkElement fsource = e.Source as FrameworkElement;
                        fsource.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        TextBox hh = (TextBox)Keyboard.FocusedElement;
                        hh.SelectAll();
                    }
                    if (sender == this.Gateway3)
                    {
                        ss = ss.Replace(".", "");
                        tb.Text = ss;
                        tb.Select(tb.Text.Length, 0);
                    }
                    if (ss.Length >= 3 && sender != this.Gateway3)
                    {
                        FrameworkElement fsource = e.Source as FrameworkElement;
                        fsource.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        TextBox hh = (TextBox)Keyboard.FocusedElement;
                        hh.SelectAll();
                    }
                }
            }            
        }

        private void text_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if(e.Key==Key.Back)
            {
                if (tb.Text=="")
                {
                    FrameworkElement fsource = e.Source as FrameworkElement;
                    fsource.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    TextBox hh = (TextBox)Keyboard.FocusedElement;
                    hh.Select(hh.Text.Length, 0);
                }
            }
        }
        public static void EnableDHCP()//开启DHCP
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                //如果没有启用IP设置的网络设备则跳过
                if (!(bool)mo["IPEnabled"])
                    continue;
                //重置DNS为空
                mo.InvokeMethod("SetDNSServerSearchOrder", null);
                //开启DHCP
                mo.InvokeMethod("EnableDHCP", null);
            }
        }
        bool assignment = false;//设置全局变量是否处于赋值状态。
        public void show_IP()//获取本地IP、网关、MAC地址显示至界面
        {
            mac.Content = "本机MAC:" + getMacAddr_Local();
            string ip = GetClientLocalIPv4Address();
            string gateway = GetGateway();
            string[] ip_item = Regex.Split(ip, "\\.");
            string[] gateways = Regex.Split(gateway, "\\.");
            assignment = false;
            ip0.Text = ip_item[0];
            ip1.Text = ip_item[1];
            ip2.Text = ip_item[2];
            ip3.Text = ip_item[3];
            Gateway0.Text = gateways[0];
            Gateway1.Text = gateways[1];
            Gateway2.Text = gateways[2];
            Gateway3.Text = gateways[3];
            assignment = true;
        }
        public static bool IS_DHCP()//是否开启DHCP
        {
            NetworkInterface[] adapter = NetworkInterface.GetAllNetworkInterfaces();
            IPInterfaceProperties ipp = adapter[0].GetIPProperties();
            IPv4InterfaceProperties ip4ip = ipp.GetIPv4Properties();
            return ip4ip.IsDhcpEnabled;
        }
        public void IS_SHOW_TEXT(bool a)//是否启用阅读文本
        {
            ip0.IsReadOnly = a;
            ip1.IsReadOnly = a;
            ip2.IsReadOnly = a;
            ip3.IsReadOnly = a;
            Gateway0.IsReadOnly = a;
            Gateway1.IsReadOnly = a;
            Gateway2.IsReadOnly = a;
            Gateway3.IsReadOnly = a;
            if (a)
            {
                text_bj.Source = new BitmapImage(new Uri(@"IMG/text-no-bg.png", UriKind.Relative));
                text_bj2.Source = new BitmapImage(new Uri(@"IMG/text-no-bg.png", UriKind.Relative));
                TEXT_color(Color.FromRgb(161,161,161));
                TEXT_Thickness(new Thickness(0));
                TEXT_Cursor(Cursors.Arrow);
                TEXT_Focusable(false);
            }
            else
            {
                text_bj.Source = new BitmapImage(new Uri(@"IMG/text-bg.png", UriKind.Relative));
                text_bj2.Source = new BitmapImage(new Uri(@"IMG/text-bg.png", UriKind.Relative));
                TEXT_color(Colors.Black);
                TEXT_Thickness(new Thickness(0, 0, 0, 1));
                TEXT_Cursor(Cursors.IBeam);
                TEXT_Focusable(true);
            }
        }
        public void TEXT_Focusable(bool a)
        {
            ip0.Focusable = a;
            ip1.Focusable = a;
            ip2.Focusable = a;
            ip3.Focusable = a;
            Gateway0.Focusable = a;
            Gateway1.Focusable = a;
            Gateway2.Focusable = a;
            Gateway3.Focusable = a;
        }
 
        public void TEXT_Cursor(Cursor a)
        {
            
            ip0.Cursor = a;
            ip1.Cursor = a;
            ip2.Cursor = a;
            ip3.Cursor = a;
            Gateway0.Cursor = a;
            Gateway1.Cursor = a;
            Gateway2.Cursor = a;
            Gateway3.Cursor = a;
        }
        public void TEXT_Thickness(Thickness a)
        {
            ip0.BorderThickness = a;
            ip1.BorderThickness = a;
            ip2.BorderThickness = a;
            ip3.BorderThickness = a;
            Gateway0.BorderThickness = a;
            Gateway1.BorderThickness = a;
            Gateway2.BorderThickness = a;
            Gateway3.BorderThickness = a;
        }
        public void TEXT_color(Color a)
        {            
            IP1.Foreground = new SolidColorBrush(a);
            IP2.Foreground = new SolidColorBrush(a);
            ip0.Foreground = new SolidColorBrush(a);
            ip1.Foreground = new SolidColorBrush(a);
            ip2.Foreground = new SolidColorBrush(a);
            ip3.Foreground = new SolidColorBrush(a);
            Gateway0.Foreground = new SolidColorBrush(a);
            Gateway1.Foreground = new SolidColorBrush(a);
            Gateway2.Foreground = new SolidColorBrush(a);
            Gateway3.Foreground = new SolidColorBrush(a);
            L1.Foreground = new SolidColorBrush(a);
            L2.Foreground = new SolidColorBrush(a);
            L3.Foreground = new SolidColorBrush(a);
            L4.Foreground = new SolidColorBrush(a);
            L5.Foreground = new SolidColorBrush(a);
            L6.Foreground = new SolidColorBrush(a);
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (IS_DHCP())
            {
                IS_SHOW_TEXT(true);
            }
            else
            {
                ip_ComboBox.SelectedIndex = 1;
            }
            show_IP();
        }

        private void ip_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                //事件响应
                if (ip_ComboBox.SelectedIndex == 0)
                {
                    IS_SHOW_TEXT(true);
                    if (!IS_DHCP())
                    {
                        EnableDHCP();
                        Enter.Content = "获取IP中...";
                        Enter.IsEnabled = false;
                        ip_ComboBox.IsEnabled = false;
                        ip0.Text = "";
                        ip1.Text = "";
                        ip2.Text = "";
                        ip3.Text = "";
                        Gateway0.Text = "";
                        Gateway1.Text = "";
                        Gateway2.Text = "";
                        Gateway3.Text = "";
                        var t = DateTime.Now.AddMilliseconds(4000);//延迟4秒执行
                        while (DateTime.Now < t)
                            DispatcherHelper.DoEvents();
                        ip_ComboBox.IsEnabled = true;
                        Enter.Content = "安装";
                        Enter.IsEnabled = true;
                    }
                    show_IP();
                }
                else
                {
                    ip0.Text = "";
                    ip1.Text = "";
                    ip2.Text = "";
                    ip3.Text = "";
                    Gateway0.Text = "";
                    Gateway1.Text = "";
                    Gateway2.Text = "";
                    Gateway3.Text = "";
                    IS_SHOW_TEXT(false);
                }
            }  
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ip0.Text == ""|| ip1.Text == ""|| ip2.Text == "" || ip3.Text == "" || Gateway0.Text == "" || Gateway1.Text == "" || Gateway2.Text == "" || Gateway3.Text == "")
            {
                MessageBox.Show("请完整输入IP或网关！");
            }
            else
            {
                if (ip_ComboBox.SelectedIndex == 1)
                {
                    string[] ip = { get_ip() };
                    string[] submask = { "255.255.255.0" };
                    string[] Gateway = { get_Gateway() };
                    string[] DNS = { "10.40.1.2", "10.35.0.2" };
                    SetIPAddress(ip, submask, Gateway, DNS);
                    Enter.Content = "更改IP中...";
                    Enter.IsEnabled = false;
                    ip_ComboBox.IsEnabled = false;
                    IS_SHOW_TEXT(true);
                IP_A: var t = DateTime.Now.AddMilliseconds(4000);//延迟4秒执行
                    while (DateTime.Now < t)
                        DispatcherHelper.DoEvents();
                    if (IsPingIP("baidu.com"))
                    {
                        Hide();
                        setup aa = new setup();
                        aa.Show();
                    }
                    else
                    {
                        if (MessageBox.Show("请联系网络小组同事调控网络！是否重试？", "无法连接网络！", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            goto IP_A;
                        }
                    }
                    ip_ComboBox.IsEnabled = true;
                    Enter.IsEnabled = true;
                    Enter.Content = "安装";
                    IS_SHOW_TEXT(false);
                }
                else
                {
                    Hide();
                    setup aa = new setup();
                    aa.Show();
                }
                
            }
        }
        public static class DispatcherHelper//延迟5秒执行方法
        {
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
                try { Dispatcher.PushFrame(frame); }
                catch (InvalidOperationException) { }
            }
            private static object ExitFrames(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        } 

        private void EXIT_ICO_MouseEnter(object sender, MouseEventArgs e)
        {
            EXIT.Source=new BitmapImage(new Uri(@"IMG/EXIT-01.png", UriKind.Relative));
        }

        private void EXIT_MouseLeave(object sender, MouseEventArgs e)
        {
            EXIT.Source = new BitmapImage(new Uri(@"IMG/EXIT.png", UriKind.Relative));
        }

        private void EXIT_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
