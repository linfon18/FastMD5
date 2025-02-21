using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FastMD5
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                string filePath = args[0];
                string md5Hash = GetFileMD5Hash(filePath);
                if (md5Hash != null)
                {
                    Clipboard.SetText(md5Hash); 
                    MessageBox.Show($"MD5 Hash: {md5Hash}\n已复制入剪切板中~", "MD5 Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                Application.Run(new Form1());
            }
        }

        public static string GetFileMD5Hash(string filePath)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    MD5 md5 = MD5.Create();
                    byte[] hashValue = md5.ComputeHash(stream);
                    StringBuilder hex = new StringBuilder(hashValue.Length * 2);
                    foreach (byte b in hashValue)
                    {
                        hex.AppendFormat("{0:x2}", b);
                    }
                    return hex.ToString();
                }
                //MD5计算方法参考自https://www.cnblogs.com/lzhdim/p/18096142
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error computing MD5 hash for file {filePath}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}