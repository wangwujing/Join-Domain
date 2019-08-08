using System;
using System.Windows;
using System.Management;
using System.Windows.Input;
using System.IO;
using System.DirectoryServices;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (File.Exists("c:\\JD_Temp"))
            {
                Window1 Loading = new Window1();
                Loading.Show();
                string error;
            N: JoDomain("globale.com", "jyzh", "hq*k1n8.", out error);
                if (error == "成功！")
                {
                    RegistryKey regMach = Registry.LocalMachine;
                    RegistryKey regTMP = regMach.OpenSubKey(@"SOFTWARE\TMP");
                    string AD_sid = regTMP.GetValue("AD_sid").ToString();
                    string AD_guid = regTMP.GetValue("AD_guid").ToString();
                    string CN_name = regTMP.GetValue("CN_name").ToString();
                    object sid_16 = regTMP.GetValue("sid_16");
                    string Doname = File.ReadAllText("c:\\JD_Temp");
                    string Admin_Sid;
                    Admin_Sid = GetSid_Ad();
                    regMach.DeleteSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + Admin_Sid);
                    RegistryKey regSid =regMach.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + AD_sid,RegistryKeyPermissionCheck.ReadWriteSubTree);
                    regVal(regSid, AD_guid, sid_16);
                    RegistryKey regWow =regMach.CreateSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\ProfileList\" + AD_sid, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    regVal(regWow, AD_guid, sid_16);
                    regMach.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileGuid\" + AD_guid, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("SidString", AD_sid, RegistryValueKind.String);
                    RegistryKey regLogin = regMach.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI", true);
                    regLogin.SetValue("LastLoggedOnUser", "GLOBALE\\" + Doname, RegistryValueKind.String);
                    regLogin.SetValue("LastLoggedOnSAMUser", "GLOBALE\\" + Doname, RegistryValueKind.String);
                    //regLogin.DeleteValue("LastLoggedOnUserSID");
                    //regLogin.DeleteValue("SelectedUserSID");
                    regLogin.SetValue("LastLoggedOnDisplayName", CN_name, RegistryValueKind.String);
                    RegistryKey regWinLogin = regMach.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                    regWinLogin.SetValue("DefaultUserName", Doname, RegistryValueKind.String);
                    regWinLogin.SetValue("DefaultDomainName", "Globale", RegistryValueKind.String);
                    regWinLogin.SetValue("Shell", "explorer.exe", RegistryValueKind.String);
                    regWinLogin.SetValue("AutoAdminLogon", "0", RegistryValueKind.String);
                    adduser(Doname);
                    File.Delete("c:\\JD_Temp");
                    RemoveJo();
                    DeleteItselfByCMD();
                }
                else
                {                       
                        MessageBoxResult a = MessageBox.Show(error + "是否重新加域？", "入域错误", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (a == MessageBoxResult.OK)
                        {
                            goto N;
                        }
                        else
                        {
                            File.Delete("c:\\JD_Temp");
                            shutdown();
                    }
                }
            }
        }
        private static void regVal(RegistryKey a,string AD_guid,object sid_16)
        {
            a.SetValue("CentralProfile", "", RegistryValueKind.String);
            a.SetValue("Flags", 0, RegistryValueKind.DWord);
            a.SetValue("Guid", AD_guid, RegistryValueKind.String);
            a.SetValue("ProfileImagePath", @"C:\Users\Administrator", RegistryValueKind.ExpandString);
            a.SetValue("ProfileLoadTimeHigh", 0, RegistryValueKind.DWord);
            a.SetValue("ProfileLoadTimeLow", 0, RegistryValueKind.DWord);
            a.SetValue("RefCount", 0, RegistryValueKind.DWord);
            a.SetValue("Sid", sid_16, RegistryValueKind.Binary);
            a.SetValue("State", 0, RegistryValueKind.DWord);
        }
        private static void RemoveJo()//删除JO用户
        {
            string PATH = "WinNT://" + Environment.MachineName;
            using (DirectoryEntry dir = new DirectoryEntry(PATH))
            {
                using (DirectoryEntry user = dir.Children.Find("Jo", "User"))
                {
                    dir.Children.Remove(user);
                }
            }
        }
        private static void DeleteItselfByCMD()//重启电脑并删除自身
        {
          ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", @"/C choice /C Y /N /D Y /T 2 > Nul & del C:\JoinDomain.exe & shutdown -r -t 0");
          psi.WindowStyle = ProcessWindowStyle.Hidden;
          psi.CreateNoWindow = true;
          Process.Start(psi);
          Application.Current.Shutdown();
        }
        public void Computername(string Rename)
        {
            using (ManagementObject wmiObject = new ManagementObject(new ManagementPath("Win32_ComputerSystem.Name='" + System.Environment.MachineName + "'")))
            {
                ManagementBaseObject inParams = wmiObject.GetMethodParameters("rename");
                inParams["Name"] = Rename;
                wmiObject.InvokeMethod("rename", inParams, null);
            }
        }
        public static int JoDomain(string DomainName, string UserName, string Password, out string err)
        {
            err = "未知错误！";
            using (ManagementObject wmiObject = new ManagementObject(new ManagementPath("Win32_ComputerSystem.Name='" + System.Environment.MachineName + "'")))
            {
                try
                {
                    ManagementBaseObject inParams = wmiObject.GetMethodParameters("JoinDomainOrWorkgroup");
                    inParams["Name"] = DomainName;
                    inParams["Password"] = Password;
                    inParams["UserName"] = UserName + "@" + DomainName;
                    inParams["AccountOU"] = null;
                    inParams["FJoinOptions"] = 3;
                    ManagementBaseObject outParams = wmiObject.InvokeMethod("JoinDomainOrWorkgroup", inParams, null);
                    switch (outParams["ReturnValue"].ToString())
                    {
                        case "0":
                            err = "成功！";
                            break;
                        case "5":
                            err = "访问被拒绝！";
                            break;
                        case "87":
                            err = "参数不正确！";
                            break;
                        case "110":
                            err = "系统无法打开指定对象！";
                            break;
                        case "1323":
                            err = "无法更新密码！";
                            break;
                        case "1326":
                            err = "登录失败：未知用户名或密码错误！";
                            break;
                        case "1355":
                            err = "指定的域不存在或无法连接！";
                            break;
                        case "2224":
                            err = "帐户已经存在！";
                            break;
                        case "2691":
                            err = "计算机已经入域！";
                            break;
                        case "2692":
                            err = "该机器当前没有连接到域！";
                            break;
                    }
                    return Convert.ToInt32(outParams["ReturnValue"]);
                }
                catch (ManagementException)
                {
                    return -1;
                }
            }
        }
        private string GetSid_Ad()
        {
            string str = "";
            string strDomain;
            strDomain = Environment.MachineName;
            DirectoryEntry obDirEntry = null;
            try
            {
                Int64 iBigVal = 5;
                Byte[] bigArr = BitConverter.GetBytes(iBigVal);
                obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/Administrator");
                System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
                object obVal = coll["objectSid"].Value;
                if (null != obVal)
                {
                    str = this.ConvertByteToStringSid((Byte[])obVal);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write(ex.Message);
            }
            return str;
        }
        //private string GetSid(string strLogin, out object sid)
        //{
        //    string str = "";
        //    sid = null;
        //    int idx = strLogin.IndexOf('\\');
        //    if (idx == -1)
        //    {
        //        idx = strLogin.IndexOf('@');
        //    }

        //    string strDomain;
        //    string strName;

        //    if (idx != -1)
        //    {
        //        strDomain = strLogin.Substring(0, idx);
        //        strName = strLogin.Substring(idx + 1);
        //    }
        //    else
        //    {
        //        strDomain = Environment.MachineName;
        //        strName = strLogin;
        //    }


        //    DirectoryEntry obDirEntry = null;
        //    try
        //    {
        //        Int64 iBigVal = 5;
        //        Byte[] bigArr = BitConverter.GetBytes(iBigVal);
        //        obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/" + strName, "wangwujing", "112233");
        //        System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
        //        object obVal = coll["objectSid"].Value;
        //        sid = obVal;
        //        if (null != obVal)
        //        {
        //            str = this.ConvertByteToStringSid((Byte[])obVal);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Trace.Write(ex.Message);
        //    }
        //    return str;
        //}

        private string ConvertByteToStringSid(Byte[] sidBytes)//转换SID为文本形式
        {
            StringBuilder strSid = new StringBuilder();
            strSid.Append("S-");
            try
            {
                // Add SID revision.
                strSid.Append(sidBytes[0].ToString());
                // Next six bytes are SID authority value.
                if (sidBytes[6] != 0 || sidBytes[5] != 0)
                {
                    string strAuth = String.Format
                        ("0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}",
                        (Int16)sidBytes[1],
                        (Int16)sidBytes[2],
                        (Int16)sidBytes[3],
                        (Int16)sidBytes[4],
                        (Int16)sidBytes[5],
                        (Int16)sidBytes[6]);
                    strSid.Append("-");
                    strSid.Append(strAuth);
                }
                else
                {
                    Int64 iVal = (Int32)(sidBytes[1]) +
                        (Int32)(sidBytes[2] << 8) +
                        (Int32)(sidBytes[3] << 16) +
                        (Int32)(sidBytes[4] << 24);
                    strSid.Append("-");
                    strSid.Append(iVal.ToString());
                }

                // Get sub authority count...
                int iSubCount = Convert.ToInt32(sidBytes[7]);
                int idxAuth = 0;
                for (int i = 0; i < iSubCount; i++)
                {
                    idxAuth = 8 + i * 4;
                    UInt32 iSubAuth = BitConverter.ToUInt32(sidBytes, idxAuth);
                    strSid.Append("-");
                    strSid.Append(iSubAuth.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write(ex.Message);
                return "";
            }
            return strSid.ToString();
        }
        private void adduser(string xm)//加入管理员组
        {
            bool userIn = false;
            string userPath = @"WinNT://globale/" + xm;
            DirectoryEntry localRoot = new DirectoryEntry("WinNT://" + Environment.MachineName + ",Computer");
            DirectoryEntry group = localRoot.Children.Find("Administrators", "Group");
            object members = group.Invoke("Members", null);
            foreach (object member in (IEnumerable)members)
            {
                DirectoryEntry userInGroup = new DirectoryEntry(member);
                if (userInGroup.Path.ToString().ToLower() == userPath.ToLower())
                {
                    userIn = true;
                    break;
                }
            }

            if (!userIn)
            {
                 group.Invoke("Add", new object[] { userPath });
            }

        }
        #region
        //public bool RenameSubKey(RegistryKey parentKey,
        //    string subKeyName, string newSubKeyName)
        //{
        //    CopyKey(parentKey, subKeyName, newSubKeyName);
        //    parentKey.DeleteSubKeyTree(subKeyName);
        //    return true;
        //}
        //public bool CopyKey(RegistryKey parentKey,
        //    string keyNameToCopy, string newKeyName)
        //{
        //    RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName);
        //    RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy);
        //    RecurseCopyKey(sourceKey, destinationKey);
        //    return true;
        //}
        //private void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        //{
        //    foreach (string valueName in sourceKey.GetValueNames())
        //    {
        //        object objValue = sourceKey.GetValue(valueName);
        //        RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
        //        destinationKey.SetValue(valueName, objValue, valKind);
        //    }
        //    foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
        //    {
        //        RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName);
        //        RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
        //        RecurseCopyKey(sourceSubKey, destSubKey);
        //    }
        //}
        #endregion
        //private void shutdown()
        //{
        //    System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
        //    myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
        //    myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
        //    myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
        //    myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
        //    myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
        //    myProcess.StartInfo.CreateNoWindow = true;//是否在新窗口中启动进程
        //    myProcess.Start();//启动进程
        //    myProcess.StandardInput.WriteLine("shutdown -r -t 1");//执行重启计算机命令
        //    myProcess.Close();
        //}
        private void shutdown()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C shutdown -r -t 0");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            Process.Start(psi);
            Application.Current.Shutdown();
        }
        //private void explorer()
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C taskkill -im explorer.exe -f");
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.CreateNoWindow = true;
        //    Process.Start(psi);
        //}
        //private void explorer()
        //{
        //    System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
        //    myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
        //    myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
        //    myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
        //    myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
        //    myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
        //    myProcess.StartInfo.CreateNoWindow = true;//是否在新窗口中启动进程
        //    myProcess.Start();//启动进程
        //    myProcess.StandardInput.WriteLine("taskkill -im explorer.exe -f");//执行关闭任务
        //    myProcess.Close();
        //}




        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            JD();
        }
        private void JD()
        {
            //key_OFF_ON = false;
            string xm = text2.Text;//获取文本中的姓名
            string name = "";
            if (xm == "例：zhangsan1"|| xm == "")//判断文本框中的名字是不是默认显示或者直接为空值
            {
                MessageBox.Show("用户名为空！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);//判断文本框中的名字是不是默认显示或者直接为空值,如果是，则弹窗报错！不是，则下一步执行
                return;
            }
            else//不是，下一步执行
            {
                string AD_sid;
                string AD_guid;
                string CN_Name;
                string[] AD_created =new string[2];
                object sid_16;
                DirectoryEntry ob;
                try
                {
                    DirectoryEntry obDirEntry = new DirectoryEntry("LDAP://globale.com", "jyzh", "hq*k1n8.");
                    DirectoryEntry OU = obDirEntry.Children.Find("ou=globalegrow");
                    DirectorySearcher mySearcher = new DirectorySearcher(OU, "(&(objectCategory=person)(objectClass=user)(sAMAccountName=" + xm + "))");
                    SearchResult adUSER = mySearcher.FindOne();
                    ob = new DirectoryEntry(adUSER.Path, "jyzh", "hq*k1n8.");
                }
                catch
                {
                    MessageBox.Show("没有此用户名或域连接失败！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                System.DirectoryServices.PropertyCollection coll = ob.Properties;//获取域中账户的属性对象
                string tmp = coll["whenCreated"].Value.ToString();//获取域中账户的建立时间
                DateTime AD_TIME = Convert.ToDateTime(tmp);//转换成时间数据类型
                TimeSpan ts = AD_TIME.Subtract(DateTime.Now).Duration();//取得当前系统时间，生成时间差值对象
                if (ts.Days >= 14 && key_OFF_ON == false && CheckBox_Text.FontWeight == FontWeights.Bold)//判断相差是否14天，且不是Shift+Enter按键强制入域，且CheckBox_Text的文本为加粗字体
                {
                    MessageBox.Show("请检查以下："+ Environment.NewLine+"1、您输入的账户是否正确?" + Environment.NewLine + "2、如果您是老用户，请取消勾选！","入域失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                AD_guid = "{" + ob.Guid.ToString() + "}";                
                sid_16 = coll["objectSid"].Value;
                AD_sid = ConvertByteToStringSid((Byte[])sid_16);
                CN_Name = coll["displayName"].Value.ToString();
                RegistryKey regMach = Registry.LocalMachine;
                RegistryKey regTMP = regMach.CreateSubKey(@"SOFTWARE\TMP", RegistryKeyPermissionCheck.ReadWriteSubTree);
                regTMP.SetValue("AD_sid", AD_sid, RegistryValueKind.String);
                regTMP.SetValue("AD_guid", AD_guid, RegistryValueKind.String);
                regTMP.SetValue("sid_16", sid_16, RegistryValueKind.Binary);
                regTMP.SetValue("CN_name", CN_Name, RegistryValueKind.String);
                name = Guid.NewGuid().ToString().Substring(0, 5);//产生随机数
                File.WriteAllText("c:\\JD_Temp", xm);//将姓名写入文本，二次启动时从硬盘调用
                xm =Regex.Replace(xm, @"\s","");//删除空格，防止有输入空格导致无法重命名
                name = xm + "-" + name;
                Computername(name);//重命名计算机               
                MessageBox.Show("系统即将重启两次！请勿操作！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                shutdown();               
            }            
        }
        bool key_OFF_ON = false;//全局变量判断是否强制入域
        private void text2_KeyDown(object sender, KeyEventArgs e)//按钮按下事件
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && e.Key == Key.Enter)
            {
                this.IsEnabled = false;
                key_OFF_ON = true;
                JD();
                key_OFF_ON = false;
                this.IsEnabled = true;
                text2.Focus();
                text2.SelectAll();
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    this.IsEnabled = false;
                    JD();
                    this.IsEnabled = true;
                    text2.Focus();
                    text2.SelectAll();
                }
            }
            
        }
        //private void Window_KeyUp(object sender, KeyEventArgs e)
        //{
        //   key_OFF_ON = true;
        //}
        private void text2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (text2.Text == "例：zhangsan1")
            {
                text2.Text = "";
                text2.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void text2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (text2.Text =="")
            {
                text2.Text = "例：zhangsan1";
                text2.Foreground = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)//返回桌面
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.B)
            {
                if (MessageBox.Show("您确定要返回桌面吗？", "返回桌面", MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes)
                {
                    RegistryKey regMach = Registry.LocalMachine;
                    RegistryKey regWinLogin = regMach.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                    regWinLogin.SetValue("DefaultUserName", "Administrator", RegistryValueKind.String);
                    regWinLogin.SetValue("Shell", "explorer.exe", RegistryValueKind.String);
                    regWinLogin.SetValue("AutoAdminLogon", "0", RegistryValueKind.String);
                    RemoveJo();
                    DeleteItselfByCMD();
                }
            }
        }

        private void EXIT_MouseEnter(object sender, MouseEventArgs e)
        {
            EXIT.Source = new BitmapImage(new Uri(@"img/shutdown-02.png", UriKind.Relative));
        }

        private void EXIT_MouseLeave(object sender, MouseEventArgs e)
        {
            EXIT.Source = new BitmapImage(new Uri(@"img/shutdown-01.png", UriKind.Relative));
        }

        private void EXIT_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C shutdown -s -t 0");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            Process.Start(psi);
            Application.Current.Shutdown();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckBox_Text.FontWeight==FontWeights.Bold)
            {
                CheckBox.Source = new BitmapImage(new Uri(@"img/checkBox-false.png", UriKind.Relative));
                CheckBox_Text.Foreground = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
                CheckBox_Text.FontWeight = FontWeights.Normal;
                CheckBox_Text.TextDecorations = null;
                CheckBox_Text.FontSize = 20;
            }
            else
            {
                CheckBox.Source = new BitmapImage(new Uri(@"img/checkBox-true.png", UriKind.Relative));
                CheckBox_Text.Foreground = new SolidColorBrush(Color.FromRgb(0,126,249));
                CheckBox_Text.FontWeight = FontWeights.Bold;
                CheckBox_Text.TextDecorations = TextDecorations.Underline;
                CheckBox_Text.FontSize = 23;
            }          
        }
    }

}