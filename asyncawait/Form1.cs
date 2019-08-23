using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace asyncawait
{
    public partial class Form1 : Form
    {
        Stopwatch stopwatch = new Stopwatch();
        public delegate void UpdateTxt(string msg);
        //定义一个委托变量
        public UpdateTxt updateTxt;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 实例化委托
             updateTxt = new UpdateTxt(UpdateTxtMethod);
        }
        public void UpdateTxtMethod(string msg)
        {
            richTextBox1.AppendText(msg + "\r\n");
            richTextBox1.ScrollToCaret();
            richTextBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            Test();
            Invoke(updateTxt, "end");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            TestAsync();
            Invoke(updateTxt, "end");
        }
        public void Test()
        {
            Thread.Sleep(3000);
            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            Invoke(updateTxt, "同步完成"+timespan.TotalMilliseconds);
            stopwatch.Start();

        }
        public async void TestAsync()
        {
            await Task.Delay(30000);
            Invoke(updateTxt, "异步完成");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            TestAsync();
            Test();
            Invoke(updateTxt, "end");
        }

        private async void button4_ClickAsync(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            await TestStringAsync();
            Test();
            richTextBox1.AppendText("end");
        }
        public async Task<string> TestStringAsync()
        {
            await Task.Delay(3000);
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            Invoke(updateTxt, "异步完成" + timespan.TotalMilliseconds);
            return "异步完成";
        }

        private async void button5_ClickAsync(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            Test();
            await TestStringAsync();
            Invoke(updateTxt, "end");
        }

        private async void button6_ClickAsync(object sender, EventArgs e)
        {

            stopwatch.Start();
            richTextBox1.Clear();
            Invoke(updateTxt, "程序开始");
            // BeginInvoke(updateTxt, "程序开始");
            //var testTask= TestStringAsync();
            await TestStringAsync();
            Test();
            //await testTask;
            Invoke(updateTxt, "end");
        }

        private async void button7_ClickAsync(object sender, EventArgs e)
        {
            var tasks = new List<Task>();
            tasks.Add(AAsync());
            tasks.Add(BAsync());
            await Task.WhenAll(tasks);

            var tt = "aaa";
        }
        public async Task<bool> AAsync()
        {
            await Task.Delay(3);
            return true;
        }
        public async Task<bool> BAsync()
        {
            await Task.Delay(3);
            return false;
        }
        Nu nu=new Nu { Num=0};
        private async void button8_ClickAsync(object sender, EventArgs e)
        {
            nu.Num = 0;
            stopwatch.Restart();
            List<Task> lisTask = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                lisTask.Add(DoSomething());
            }
            await Task.WhenAll(lisTask);
        }
        public async Task DoSomething()
        {
            Nu nu1 = new Nu { Num = 0 };
            await Task.Run(() => {
                for (int i = 0; i < 20000; i++)
                {
                    nu1.Num++;
                }
            });
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            Invoke(updateTxt, nu1.Num.ToString() + "   " + timespan.TotalMilliseconds);
            stopwatch.Start();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            nu.Num = 0;
            stopwatch.Restart();
            for (int i = 0; i < 400000; i++)
            {
                nu.Num++;
            }
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            Invoke(updateTxt, nu.Num.ToString() + "   " + timespan.TotalMilliseconds);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            stopwatch.Restart();
            nu.Num = 0;
            MyThread myThread = new MyThread();
            myThread.nu = nu;
            for (int i = 0; i < 20; i++)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(DoThread);
                bw.WorkerSupportsCancellation = true;
                bw.RunWorkerAsync();
            }
        }
        public void DoThread(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 20000; i++)
            {
                nu.Add();
            }
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            Invoke(updateTxt, nu.Num.ToString() + "   " + timespan.TotalMilliseconds);
            stopwatch.Start();
        }
    }
    class Nu {
        private int num = 0;
        public int Num { get=> num; set=>num=value; }
        public int Add() { return Num++; }
    }

    class MyThread {
        public Nu nu { get; set; }
        public void DoThread(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 20000; i++)
            {
                nu.Add();
            }
        }
    }
}
