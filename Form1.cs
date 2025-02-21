using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.Linq;
using Microsoft.Win32;

namespace FastMD5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsElevated())
            {
                try
                {
                    RestartAsAdmin();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法以管理员身份重新启动程序：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            try
            {
                string menuText = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(menuText))
                {
                    MessageBox.Show("请输入有效的菜单名称！", "无效输入", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string targetPath = Path.Combine(userDir, "FastMD5.exe");
                File.Copy(Application.ExecutablePath, targetPath, overwrite: true);

                string menuFilePath = Path.Combine(userDir, "FastMD5_MenuName.ini");
                File.WriteAllText(menuFilePath, menuText);
                File.SetAttributes(menuFilePath, FileAttributes.Hidden);

                using (RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(@"*\shell", true))
                {
                    foreach (string subKeyName in shellKey.GetSubKeyNames())
                    {
                        if (subKeyName == menuText)
                        {
                            shellKey.DeleteSubKeyTree(subKeyName);
                            break;
                        }
                    }

                    using (RegistryKey key = shellKey.CreateSubKey(menuText))
                    {
                        key.SetValue("", menuText, RegistryValueKind.String);
                        key.SetValue("Icon", targetPath + ",0", RegistryValueKind.String);

                        using (RegistryKey commandKey = key.CreateSubKey("command"))
                        {
                            commandKey.SetValue("", $"\"{targetPath}\" \"%1\"", RegistryValueKind.String);
                        }
                    }
                }

                MessageBox.Show("右键菜单项已成功更新！", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加菜单项失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!IsElevated())
            {
                try
                {
                    RestartAsAdmin();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法以管理员身份重新启动程序：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            try
            {
                string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string menuFilePath = Path.Combine(userDir, "FastMD5_MenuName.ini");

                if (!File.Exists(menuFilePath))
                {
                    MessageBox.Show("未找到菜单名称记录文件，无法确定要删除的菜单项。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string menuText = File.ReadAllText(menuFilePath).Trim();

                using (RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(@"*\shell", true))
                {
                    if (shellKey != null && shellKey.GetSubKeyNames().Contains(menuText))
                    {
                        shellKey.DeleteSubKeyTree(menuText);
                        MessageBox.Show($"右键菜单项 '{menuText}' 已成功移除！", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"未找到名为 '{menuText}' 的右键菜单项。", "未找到菜单项", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                string targetPath = Path.Combine(userDir, "FastMD5.exe");
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                File.Delete(menuFilePath); // 删除菜单名称记录文件
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsElevated()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin()
        {
            string exePath = Application.ExecutablePath;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Verb = "runas",
                UseShellExecute = true
            };
            Process.Start(startInfo);
            Application.Exit();
       }
      private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}