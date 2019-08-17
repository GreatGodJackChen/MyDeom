using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ThreadDeom.Models;

namespace ThreadDeom.Controllers
{
    public class HomeController : Controller
    {
        //定义委托 参数double类型
        delegate double CalculateMethod(double Diameter);
        //声明委托
        static CalculateMethod calcMethod;

        static double result = 0;

        public IActionResult Index()
        {
            //1.无参数无返回值线程
            ThreadStart threadStart = new ThreadStart(Calculate);
            Thread thread = new Thread(threadStart);
            thread.Start();
            //2.需要传递单个参数
            //ParameterThreadStart的定义为void ParameterizedThreadStart(object state)??使用这个这个委托定义的线程的启动函数可以接受一个输入参数,具体例子如下
            ParameterizedThreadStart threadStart2 = new ParameterizedThreadStart(Calculate);
            Thread thread2 = new Thread(threadStart2);
            thread2.Start(0.9);
            //2.2虽然通过把需要的参数包装到一个类中,委托ParameterizedThreadStart就可以传递多个参数,但是由于这个委托的传入参数是object,所以不可避免的需要进行参数转换
            //使用线程类
            MyThread t = new MyThread(5);
            ThreadStart threadStart2_2 = new ThreadStart(t.Calculate);
            Thread thread2_2 = new Thread(threadStart2_2);
            thread2_2.Start();
            //3.这种方法把参数传递变成了属性共享,想传递多少个变量都可以,从封装上讲,把逻辑和逻辑涉及的数据封装在一起,也很不错,
            //这个方法还有一个聪明的变体,利用了匿名方法,这种变体连独立的类都省掉了,我现在给出这个方法
            double Diameter = 6;
            double Result = 0;
            Thread ta = new Thread(new ThreadStart(delegate ()  //委托匿名方法
            {
                Thread.Sleep(2000);
                Result = Diameter * Math.PI;
                Console.WriteLine("匿名 Calculate  End, Diameter is {0},Result is {1}", Diameter, Result); ;
            }));
            ta.Start();
            //4.需要传递参数且需要返回参数(.net core mvc不支持)
            //使用委托的异步调用方法和回调
            //首先我们要把需要异步调用的方法定义为一个委托,然后利用BeginInvoke来异步调用,
            //BeginInvoke的第一个参数就是直径,第二个是当线程执行完毕后的调用的方法
            //calcMethod = new CalculateMethod(Calculate); //声明具体委托方法 一个double参数的方法
            //calcMethod.BeginInvoke(5, new AsyncCallback(TaskFinished), null);
            //5.线程池
            //线程虽然是个好东西,但是也是个资源消耗大户,许多时候,我们需要用多线程,但是又不希望线程的数量过多,这就是线程池的作用,.Net为我们提供了现成的线程池ThreadPool
            //首先定义一个WaitCallback委托,WaitCallback的格式是void WaitCallback(object state),也就是说你的方法必须符合这个格式,接着调用QueueUserWorkItem,将这个任务加入线程池,当县城池有空闲线时,将会调度并运行你的代码
            //每一个进程都有一个线程池, 线程池的默认大小是25,我们可以通过SetMaxThreads方法来设置其最大值.
            //[注]由于每个进程只有一个线程池,所以如果是在iis进程,或者sqlserver的进程中使用线程池,并且需要设置线程池的最大容量的话,会影响到iis进程或sql进程,所以这两种情况下要特别小心
            WaitCallback w = new WaitCallback(Calculate);
            ThreadPool.QueueUserWorkItem(w, 1.0);
            ThreadPool.QueueUserWorkItem(w, 2.0);
            ThreadPool.QueueUserWorkItem(w, 3.0);
            ThreadPool.QueueUserWorkItem(w, 4.0);
            //设置初始化线程
            //线程默认初始化大小是有CPU和操作系统决定的。 其中线程数最小不能小于CPU的核数
            //ThreadPool.SetMaxThreads(8, 8);//最小也是CPU核数
            //ThreadPool.SetMinThreads(4, 4);
            //获取线程池当前设置 ，默认设置取决于操作系统和CPU 
            int workerThreads = 0;
            int ioThreads = 0;
            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            Console.WriteLine(String.Format("可创建最大线程数: {0};    最大 I/O 线程: {1}", workerThreads, ioThreads));

            //6.果线程类作为公共库来提供,对编写事件的人要求会相对较高,那么有什么更好的办法呢?
            //其实在.Net2.0中微软自己实现这个模式,制作了Backgroundworker这个类
            System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            // 定义需要在子线程中干的事情
            bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_DoWork);
            //定义执行完毕后需要做的事情
            bw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            //开始执行
            bw.RunWorkerAsync();

            return View();
        }
        //thread
        public void Calculate()
        {

            double Diameter = 0.5;

            Console.WriteLine("TTTTTTTTTTTTTTTTTTheThe perimeter Of Circle with a Diameter of {0} is {1}", Diameter, Diameter * Math.PI);

        }
        //thread2
        public void Calculate(object arg)
        {

            double Diameter = (double)arg;

            Console.Write("2TTTTTTTTTTTTTTTTTThe perimeter Of Circle with a Diameter of {0} is {1}", Diameter, Diameter * Math.PI);

        }

        ///<summary>
        ///线程调用的函数
        ///<summary> 
        public static double Calculate(double Diameter)
        {
            return Diameter * Math.PI;
        }
        ///<summary>
        ///线程完成之后回调的函数
        ///<summary>
        public static void TaskFinished(IAsyncResult asyncResult)
        {
            result = calcMethod.EndInvoke(asyncResult);
        }
        static void bw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)

        {
            Console.WriteLine("Complete" + Thread.CurrentThread.ManagedThreadId.ToString());
        }
        static void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)

        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    public class MyThread
    {
        public double Diameter = 10;
        public double Result = 0;
        public MyThread(int Diameter)

        {
            this.Diameter = Diameter;
        }
        public void Calculate()
        {
            Console.WriteLine("Calculate Start");
            Thread.Sleep(2000);
            Result = Diameter * Math.PI; ;
            Console.WriteLine("Calculate End, Diameter is {0},Result is {1}", this.Diameter, Result);
        }
    }
}
