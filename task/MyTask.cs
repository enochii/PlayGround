using System;
using System.Threading;


public class MyTask<TResult> {
    // 实现 wait 的逻辑
    ManualResetEvent mre;

    // 函数指针，代表对应的任务
    Func<Object, TResult> task;
    // 参数
    Object args;

    // 返回值
    public TResult result;
    // 任务是否完成
    bool finished;

    public MyTask(Func<Object, TResult> task, Object args) {
        this.task = task;
        this.args = args;
        this.result = default(TResult);
        this.finished = false;
        this.mre = new ManualResetEvent(false);
    }

    public bool IsFinish() { return finished; }

    public void Start() {
        new Thread(() => {
            this.result = task(args);
            this.finished = true;
            
            mre.Set();
            // Console.WriteLine("mre.Set()");
        }).Start();
    }

    public void Wait() {
        mre.WaitOne();
    }
}


// Main Method
class Program {
    static int Calculate(Object arg) {
        // 执行一个耗时操作
        Thread.Sleep(2000);

        int num = (int) arg;
        int sum = 0;
        for(int i = 0;i<num;i++) sum += i;
        return sum;
    } 
    
    public static void Main() {
        MyTask<int> task = new MyTask<int>(Calculate, 10);

        Console.WriteLine("开始执行任务");
        task.Start();
        
        Console.WriteLine("调用 Start() 后返回，任务还没执行结束\n---------------------------------");

        task.Wait();
        Console.WriteLine("任务执行结束，结果为：{0}", task.result);
    }
}