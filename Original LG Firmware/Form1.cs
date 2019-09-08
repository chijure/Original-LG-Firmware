using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace Original_LG_Firmware
{
    public partial class Form1 : Form
    {
        private bool cancel = false;
        private bool open = false;
        private WebClient webClient = new WebClient();
        private string path;

        public Form1()
        {
            InitializeComponent();
        }

        public string LoadPage(string adress)
        {
            WebClient webClient = new WebClient();
            string str;
            try
            {
                str = new StreamReader(webClient.OpenRead(adress)).ReadToEnd();
                webClient.Dispose();
            }
            catch
            {
                str = (string)null;
                webClient.Dispose();
            }
            return str;
        }
        public string Reverse(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public bool SaveAs(string name)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All files (*.*)|*.*";
            saveFileDialog.Title = "Save LG ROM";
            saveFileDialog.FileName = name;
            saveFileDialog.InitialDirectory = Environment.SpecialFolder.DesktopDirectory.ToString();
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return false;
            path = saveFileDialog.FileName;
            return true;
        }

        public void DownloadFile(string url)
        {
            int index = url.Length - 1;
            string str = "";
            for (; url.ElementAt<char>(index) != '/'; --index)
                str += (object)url.ElementAt<char>(index).ToString();
            if (!SaveAs(Reverse(str)))
                return;


            label4.Visible = true;
            progressBar1.Visible = true;
            button1.Visible = false;
            button2.Visible = true;
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(url), path);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (!cancel && !open)
            {
                string str = "";
                int index = path.Length - 1;
                while (path.ElementAt<char>(index) != '\\')
                    --index;
                for (; index >= 0; --index)
                    str += (object)path.ElementAt<char>(index).ToString();
                string fileName = Reverse(str);
                MessageBox.Show("Download completed!");
                Process.Start(fileName);
                open = true;
            }
            else if (cancel)
                File.Delete(path);
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            label4.Visible = false;
            button1.Visible = true;
            button2.Visible = false;
        }

        public string ClearXML(string xml)
        {
            int num = 0;
            while (num < xml.Length && xml.ElementAt<char>(num) != '<')
                ++num;
            return xml.Remove(0, num);
        }
        public void GetROM()
        {
            string url = "";
            string xml1 = LoadPage("http://csmg.lgmobile.com:9002/svc/popup/model_check.jsp?model=LG" + textBox1.Text + "&esn=" + textBox2.Text);
            string s1;
            try
            {
                s1 = ClearXML(xml1);
            }
            catch
            {
                MessageBox.Show("Unsuccessful in connecting to server or server returned null string!");
                return;
            }
            if (s1.Length > 0)
            {
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(s1)))
                {
                    try
                    {
                        xmlReader.ReadToFollowing("esn");
                        url = xmlReader.ReadElementContentAsString();
                    }
                    catch
                    {
                        MessageBox.Show("Unsuccessful in connecting to server or phone informations is wrong!");
                        return;
                    }
                }
                string xml2 = LoadPage("http://csmg.lgmobile.com:9002/csmg/b2c/client/auth_model_check2.jsp?esn=" + url);
                string s2;
                try
                {
                    s2 = ClearXML(xml2);
                }
                catch
                {
                    MessageBox.Show("Unsuccessful in connecting to server or server returned null string!");
                    return;
                }
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(s2)))
                {
                    try
                    {
                        xmlReader.ReadToFollowing("sw_url");
                        url = xmlReader.ReadElementContentAsString();
                    }
                    catch
                    {
                        MessageBox.Show("Unsuccessful in connecting to server or phone informations is wrong!");
                        return;
                    }
                }
                DownloadFile(url);
            }
            else
            {
                MessageBox.Show("Unsuccessful in connecting to server or phone informations is wrong!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.TextLength > 1 && textBox2.TextLength > 1)
            {
                open = false;
                cancel = false;
                GetROM();
            }
            else
            {
                MessageBox.Show("Insert model and serial phone!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cancel = true;
            button1.Visible = true;
            button2.Visible = false;
            webClient.CancelAsync();
        }
    }
}
