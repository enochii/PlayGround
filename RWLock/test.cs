// The complete code is located in the ReaderWriterLock class topic.
using System;
using System.Threading;
using SCH001;


public class Driver {
   public static void Main() {
      CorrectnessTest.BaseTest();
      CorrectnessTest.InfiniteReader();
      //
      PerformanceTest.PMain();
   }
}

public class CorrectnessTest {
   // TEST CASE 1
   const int numThreads = 26;
   static RWLock rWLock;
   static int resource;
   // BaseTest 主要是开启多个写进程，看是否能达到互斥
   public static void BaseTest() {
      //
      Console.WriteLine("============== BaseTest ==============");
      rWLock = new RWLock();
      resource = 0;

      Thread[] t = new Thread[numThreads+1];
      t[0] = new Thread(new ThreadStart(ReaderProc)); t[0].Start();
      //
      for (int i = 1; i <= numThreads; i++){
         t[i] = new Thread(new ThreadStart(WriterProc));
         t[i].Start();
      }
      for(int i=0;i<numThreads+1;i++) t[i].Join();
      Console.WriteLine("Expected: {0}, Actual: {1}\n", numThreads*1000, resource);
   }
   private static void ReaderProc() {
      // 读 
      for(int i=0;i<1000;i++) {
         rWLock.AcquireReaderLock();
         // Thread.Sleep(2);
         rWLock.ReleaseReaderLock();
      }
   }

   private static void WriterProc() {
      for(int i=0;i<1000;i++) {
         rWLock.AcquireWriterLock();
         resource ++;
         rWLock.ReleaseWriterLock();
      }
   }



   // TEST CASE 2
   // 多读者，看是否会发生写饥饿
   // 当写者写完后，让读者也终止，程序结束
   // 考虑到实际，这里开启了多个写进程并发写
   // 同时开启一个写线程，测试是否写线程会超过等待时间，即是否会发生写饥饿

   // TODO: 实践中发现这种 多读一写 效率很低
   // 感觉主要是为了避免写饥饿，过于限制读线程
   // 多个线程被唤醒后继续 sleep
   public static void InfiniteReader() {
      rWLock = new RWLock();
      resource = 0;

      Console.WriteLine("============== InfiniteReader ==============");
      Thread[] t = new Thread[numThreads+1];
      // 读者开始
      int readerThread = 5;
      for (int i = 1; i <= readerThread; i++){
         t[i] = new Thread(new ThreadStart(ReaderProc));
         t[i].Start();
      }
      // 写者开始
      t[0] = new Thread(new ThreadStart(WriterProcWithTimeout)); 
      t[0].Start();
      for(int i=0;i<readerThread+1;i++) t[i].Join();
      Console.WriteLine("Expected: {0}, Actual: {1}", 1*50, resource);
   }

   private static void WriterProcWithTimeout() {
      int timeout = 500;
      for(int i=0;i<50;i++) {
         // 不要超时
         try {
               rWLock.AcquireWriterLock(timeout);
               resource++;
               rWLock.ReleaseWriterLock();
            }
            catch (ApplicationException) {
               // 超时
               Console.WriteLine("Hungry Writer TIMEOUT {0}", timeout);
            }
            // try block end
         }
   }
}


// 参考 ReaderWriterLock 官网示例
public class PerformanceTest
{
   static RWLock rwl = new RWLock();
   // Define the shared resource protected by the ReaderWriterLock.
   static int resource = 0;

   // 在这里做测试的 config 修改
   const int readerTimeout = 1200;
   const int writerTimeout = 2500;
   const int numThreads = 8;
   
   static bool running = true;

   // Statistics.
   static int readerTimeouts = 0;
   static int writerTimeouts = 0;
   static int reads = 0;
   static int writes = 0;

   public static void PMain()
   {
      Console.WriteLine("============== PerformanceTest ==============");
      // 开启多线程，性能测试入口
      Thread[] t = new Thread[numThreads];
      for (int i = 0; i < numThreads; i++){
         t[i] = new Thread(new ThreadStart(PerformanceThreadProc));
         t[i].Name = new String(Convert.ToChar(i + 65), 1);
         t[i].Start();
         if (i >= numThreads/2)
            Thread.Sleep(300);
      }

      // 设置标志位，终止还在 run 的读写线程
      running = false;
      for (int i = 0; i < numThreads; i++)
         t[i].Join();

      // Display
      Console.WriteLine("\n{0} reads, {1} writes, {2} reader time-outs, {3} writer time-outs.",
            reads, writes, readerTimeouts, writerTimeouts);
      Console.Write("Press ENTER to exit... ");
      Console.ReadLine();
   }

   static void PerformanceThreadProc()
   {
      Random rnd = new Random();

      // 随机开启读写线程
      while (running) {
         double action = rnd.NextDouble();
         if (action < .7)
            ReadFromResource(readerTimeout);
         else
            WriteToResource(rnd, writerTimeout);
      }
   }

   // 获取并释放读者锁，发生超时会取消操作
   static void ReadFromResource(int timeOut)
   {
      try {
         rwl.AcquireReaderLock(timeOut);
         try {
            Display("reads resource value " + resource);
            Interlocked.Increment(ref reads);
         }
         finally {
            rwl.ReleaseReaderLock();
         }
      }
      catch (ApplicationException) {
         // 读者超时
         Interlocked.Increment(ref readerTimeouts);
      }
   }

   // 获取并释放写者锁，发生超时会取消操作
   static void WriteToResource(Random rnd, int timeOut)
   {
      try {
         rwl.AcquireWriterLock(timeOut);
         try {
            resource = rnd.Next(500);
            Display("writes resource value " + resource);
            Interlocked.Increment(ref writes);
         }
         finally {
            rwl.ReleaseWriterLock();
         }
      }
      catch (ApplicationException) {
         // 超时
         Interlocked.Increment(ref writerTimeouts);
      }
   }
    
    // Helper method
   static void Display(string msg)
   {
      Console.Write("Thread {0} {1}.       \r", Thread.CurrentThread.Name, msg);
   }
}
