using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace PN_Naive
{
    public partial class Form1 : Form
    {
        private bool result;
        private NaiveAlg naive;
        private BigInteger N;
        private BackgroundWorker bw;
        private DateTime dt0, dt1;

        public Form1()
        {
            InitializeComponent();
        }

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox2.Text += text;
            }
        }

        private BigInteger Horner(string s)
        {
            BigInteger result = new BigInteger((int)(s[0] - '0'));

            for (int i = 1; i < s.Length; i++)
                result = 10 * result + (int)(s[i] - '0');

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            N = Horner(textBox1.Text);
            bw = new BackgroundWorker();
            naive = new NaiveAlg();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();
            while (!bw.IsBusy) { }
            button1.Text = "&Stop";
            textBox2.Text = string.Empty;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            dt0 = DateTime.Now;
            result = naive.Composite(N);
            dt1 = DateTime.Now;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (result == true)

                SetText("Number is prime\r\n");
            else
                SetText("Number is not prime\r\n");

            TimeSpan ts = dt1 - dt0;
            string text = string.Empty;

            text += ts.Hours.ToString("D2") + ":";
            text += ts.Minutes.ToString("D2") + ":";
            text += ts.Seconds.ToString("D2") + ".";
            text += ts.Milliseconds.ToString("D3");
            SetText(text);
            button1.Text = "&Test";
        }
    }
}
