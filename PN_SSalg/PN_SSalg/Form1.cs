﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Diagnostics;

namespace PN_SSalg
{
    public partial class Form1 : Form
    {

        private int result;
        private SSalg ss;
        private BigInteger N;
        private BackgroundWorker bw;
       // private DateTime dt0, dt1;
        Stopwatch sw = new Stopwatch();

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

        private BigInteger ToNumber(string s)
        {
            BigInteger result = new BigInteger((int)(s[0] - '0'));

            for (int i = 1; i < s.Length; i++)
                result = 10 * result + (int)(s[i] - '0');

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text == "&Test")
                {
            N = ToNumber(textBox1.Text);
            bw = new BackgroundWorker();
            ss = new SSalg(1,bw);
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();
            while (!bw.IsBusy) { }
            button1.Text = "&Stop";
            textBox2.Text = string.Empty;
                }
                else
                    bw.CancelAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //dt0 = DateTime.Now;
            sw = Stopwatch.StartNew();
            result = ss.Composite(N, 10000);
            //dt1 = DateTime.Now;
            sw.Stop();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (result == 1)

                SetText("Number is prime\r\n");
            else if (result == 0)
                SetText("Number is not prime\r\n");
            else if (result == -1)
            {
                MessageBox.Show("SS algorithm cancelled", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            TimeSpan ts = sw.Elapsed;
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
