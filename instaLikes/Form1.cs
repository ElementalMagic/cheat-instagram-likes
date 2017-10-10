using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace лойсы_инст
{
    public partial class Form1 : Form
    {
        int i = 0;
        Encoding enc = Encoding.UTF8;
        int kolvo,count;
        public static string str;
        Thread[] thr;
        string[] urls;       
        public Form1()
        {
            InitializeComponent();
            loadParam();
            pictureBox1.Image = new Bitmap(Image.FromFile("ins.png"), pictureBox1.Width, pictureBox1.Height);
        }

        private void loadParam()
        {
            FileStream file = new FileStream(@"text.txt", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader read = new StreamReader(@"text.txt", enc);
            try
            {
                while (!read.EndOfStream)
                {
                    string Item1 = read.ReadLine();
                    if (Item1.Length > 2)
                    {
                        comboBox1.Items.Add(Item1);
                        checkedListBox1.Items.Add(Item1);
                    }
                }
            }
            catch { MessageBox.Show("Ошибочка вышла.\nСоре"); }
            read.Close();
            file.Close();
        }
        private string FindID(string address)
        {
            HttpWebRequest req;
            HttpWebResponse resp;
            StreamReader sr;
            string content;
            req = (HttpWebRequest)WebRequest.Create(address);
            resp = (HttpWebResponse)req.GetResponse();
            sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("windows-1251"));
            content = sr.ReadToEnd();
            sr.Close();
            Regex myReg = new Regex("\"" + @"\d+" + "\"");
            MatchCollection matches = myReg.Matches(content);           
            string first = matches[3].Value;
            first=first.Remove(0, 1);
            first=first.Remove(first.Length-1, 1);
            string second = matches[4].Value;
            second=second.Remove(0, 1);
            second=second.Remove(second.Length-1, 1);
            string result = string.Format("http://75.102.21.228/add?id={0}_{1}", first, second);
            return result;        
        }
        private void FindImage(object address)
        {
            try {
                pictureBox1.Image = new Bitmap(Image.FromFile("load.png"), pictureBox1.Width, pictureBox1.Height);
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                string content;
                req = (HttpWebRequest)WebRequest.Create(address as string);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("windows-1251"));
                content = sr.ReadToEnd();
                sr.Close();
                Regex regn = new Regex("\"og:image\" content=\"" + @"\S+" + "jpg");
                MatchCollection matches2 = regn.Matches(content);
                string res = matches2[0].Value;
                res = res.Remove(0, 20);
                var request = WebRequest.Create(res);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    Image img = Bitmap.FromStream(stream);
                    Bitmap bm = new Bitmap(img, pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.Image = bm;
                }
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (checkBox1.Checked)
            {
                case true:
                    if (kolvo == 0) kolvo = 20000;
                    thr = new Thread[kolvo];                    
                    urls = new string[checkedListBox1.CheckedItems.Count];
                    int cont=0;                    
                    foreach (var chck in checkedListBox1.CheckedItems)
                    {
                        urls[cont] = FindID(chck.ToString());
                        cont++;
                    }
                    timer2.Enabled = true;
                    timer2.Start();
                    timer2.Interval = hScrollBar1.Value;
                    break;
                case false:                   
                    str = comboBox1.SelectedItem.ToString();
                    if (kolvo == 0) kolvo = 20000;
                    thr = new Thread[kolvo];
                    timer1.Enabled = true;
                    timer1.Start();
                    timer1.Interval = hScrollBar1.Value;
                    break;
            }            
        }
        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();
            timer2.Enabled = false;
            timer2.Stop();
            MessageBox.Show(count.ToString());
        }       
        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            if (i == 15000)
            {
                Array.Clear(thr, 0, 20000);
                thr = new Thread[20000];
                i = 0;
            }
            i++;
            thr[i] = new Thread(new ParameterizedThreadStart(xer));
            thr[i].Name = i.ToString();
            thr[i].Start(str);           
        }


        private void button3_Click(object sender, EventArgs e)
        {
            ButtonVoidAdd();
        }     
        private void ButtonVoidAdd()
        {
            FileStream file1 = new FileStream(@"text.txt", FileMode.Append, FileAccess.Write);
            StreamWriter wrt = new StreamWriter(file1, enc);
            string AddAddress = textBox1.Text;
            wrt.WriteLine(AddAddress);
            wrt.Close();
            comboBox1.Items.Add(AddAddress);
            checkedListBox1.Items.Add(AddAddress);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            textBox3.Text = hScrollBar1.Value.ToString();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                hScrollBar1.Value = Int32.Parse(textBox3.Text);
            }
            catch { }
        }

        delegate void Del(string text);

        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach(string rqUrl in urls)
            {
                count++;
                if (i == 15000)
                {
                    Array.Clear(thr, 0, 20000);
                    thr = new Thread[20000];
                    i = 0;
                }
                i++;
                thr[i] = new Thread(new ParameterizedThreadStart(xer));
                thr[i].Name = i.ToString();
                thr[i].Start(rqUrl);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) { textBox1.Clear(); }
            if(e.KeyCode == Keys.Enter)
            {
                Regex reg = new Regex(@"\S+");
                string text = reg.Match(textBox1.Text).ToString();
                textBox1.Text = text;
                ButtonVoidAdd(); textBox1.Clear();
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Thread proc = new Thread(new ParameterizedThreadStart(FindImage));
                proc.Start(checkedListBox1.SelectedItem.ToString());
            }
            catch { }
        }

        private void checkedListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                int index = checkedListBox1.SelectedIndex;
                FileStream file1 = new FileStream(@"text.txt", FileMode.Open, FileAccess.Read);
                StreamReader read = new StreamReader(file1, enc);
                string text = read.ReadToEnd();
                Regex reg = new Regex(checkedListBox1.SelectedItem.ToString());
                text = reg.Replace(text,"");
                read.Close();
                file1 = new FileStream(@"text.txt", FileMode.Truncate, FileAccess.Write);
                StreamWriter wrt = new StreamWriter(file1, enc);
                wrt.WriteLine(text);
                wrt.Close();
                checkedListBox1.Items.Remove(checkedListBox1.SelectedItem);
                comboBox1.Items.RemoveAt(index);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Thread th = new Thread(new ParameterizedThreadStart(FindImage));
            th.Start(comboBox1.SelectedItem.ToString());           
        }
    
        private void xer(object adr)
        {
            string adres = (string)adr;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(adres);
            req.GetResponseAsync();
            Thread.Sleep(10000);
            req.Abort();
            object locker1 = new object();
            lock (locker1)
            {
                textBox1.Invoke(new Del((s) => textBox2.Text = s), Thread.CurrentThread.Name);
            }
        }
    }
}
