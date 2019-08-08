using System.Windows;
using System.IO;
using System.DirectoryServices;
using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace WpfApplication4
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class setup : Window
    {
        public setup()
        {
            InitializeComponent();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Thread thread1 = new Thread(new ThreadStart(Thread1));
            thread1.Start();
        }
        static void Thread1()
        {
            string file_JO = System.AppDomain.CurrentDomain.BaseDirectory + "JoinDomain.dll";
            //AddSecurityControll2Folder(@"c:\Users\Administrator");
            DirectoryEntry ob = new DirectoryEntry("WinNT://" + Environment.MachineName);
            DirectoryEntry obJo = ob.Children.Add("Jo", "User");
            obJo.Invoke("Put", "PasswordExpired", 0);
            obJo.Invoke("Put", "UserFlags", 66049);
            obJo.CommitChanges();
            ob.Children.Find("Administrators", "Group").Invoke("Add", obJo.Path.ToString());
            RegistryKey regUAC = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
            regUAC.SetValue("EnableLUA", "0", RegistryValueKind.DWord);
            RegistryKey regWinLogin = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
            regWinLogin.SetValue("DefaultUserName", "Jo", RegistryValueKind.String);
            regWinLogin.SetValue("Shell", "c:\\JoinDomain.exe", RegistryValueKind.String);
            regWinLogin.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
            regWinLogin.SetValue("DefaultPassword", "", RegistryValueKind.String);
            File.Copy(file_JO, "c:\\JoinDomain.exe");
            //MessageBox.Show("安装成功！即将重启。");
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C shutdown -r -t 0");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            Process.Start(psi);

        }


        //private void Window_ContentRendered(object sender, EventArgs e)
        //    {
        //        string file_JO = System.AppDomain.CurrentDomain.BaseDirectory + "JoinDomain.dll";
        //        if (Environment.UserName == "Administrator")
        //        {
        //            if (File.Exists(file_JO))
        //            {
        //                //AddSecurityControll2Folder(@"c:\Users\Administrator");
        //                DirectoryEntry ob = new DirectoryEntry("WinNT://" + Environment.MachineName);
        //                DirectoryEntry obJo = ob.Children.Add("Jo", "User");
        //                obJo.Invoke("Put", "PasswordExpired", 0);
        //                obJo.CommitChanges();
        //                ob.Children.Find("Administrators", "Group").Invoke("Add", obJo.Path.ToString());
        //                RegistryKey regUAC = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
        //                regUAC.SetValue("EnableLUA", "0", RegistryValueKind.DWord);
        //                RegistryKey regWinLogin = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
        //                regWinLogin.SetValue("DefaultUserName", "Jo", RegistryValueKind.String);
        //                regWinLogin.SetValue("Shell", "c:\\JoinDomain.exe", RegistryValueKind.String);
        //                regWinLogin.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
        //                File.Copy(file_JO, "c:\\JoinDomain.exe");
        //                MessageBox.Show("安装成功！即将重启。");
        //                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C shutdown -r -t 0");
        //                psi.WindowStyle = ProcessWindowStyle.Hidden;
        //                psi.CreateNoWindow = true;
        //                Process.Start(psi);
        //            }
        //            else
        //            {
        //                MessageBox.Show("老板，麻烦把JoinDomain.dll文件和本程序放同一个目录下。");
        //                this.Close();
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("老板，麻烦用Administrator用户登录安装。");
        //            this.Close();
        //        }










        //    //    private void AddSecurityControll2Folder(string dirPath)
        //    //{
        //    //    //获取文件夹信息
        //    //    DirectoryInfo dir = new DirectoryInfo(dirPath);
        //    //    //获得该文件夹的所有访问权限
        //    //    System.Security.AccessControl.DirectorySecurity dirSecurity = dir.GetAccessControl(AccessControlSections.All);
        //    //    //设定文件ACL继承
        //    //    InheritanceFlags inherits = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
        //    //    //添加ereryone用户组的访问权限规则 完全控制权限
        //    //    FileSystemAccessRule everyoneFileSystemAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
        //    //    //添加Users用户组的访问权限规则 完全控制权限
        //    //    FileSystemAccessRule usersFileSystemAccessRule = new FileSystemAccessRule("Users", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
        //    //    bool isModified = false;
        //    //    dirSecurity.ModifyAccessRule(AccessControlModification.Add, everyoneFileSystemAccessRule, out isModified);
        //    //    dirSecurity.ModifyAccessRule(AccessControlModification.Add, usersFileSystemAccessRule, out isModified);
        //    //    //设置访问权限
        //    //    dir.SetAccessControl(dirSecurity);
        //    //}
        //}
    }
}
