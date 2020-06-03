// @Author: Sch001
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SCH001 {
    // 读写锁
    public class RWLock {
        // lock object
        private object cond = new object();

        // 读线程数目
        private int readingCnt = 0;
        
        // 是否有写者，因为只会有一个写者正在写，所以用 bool 类型即可
        private bool writing  = false;

        // 此时正在等待的写线程数目
        // 这是为了避免写饥饿
        private int writeWaitingCnt = 0;

        // read lock
        // 只有满足以下所有条件，读者才能获取 lock
        // - 没有写者正在写
        // - 没有写者等待
        public void AcquireReaderLock() {
            Monitor.Enter(cond);
            //判断是否满足条件
            while(writeWaitingCnt > 0 || writing) {
                Monitor.Wait(cond);
            }
            // 更新状态
            readingCnt ++;
            Monitor.Exit(cond);
        }
        // read store
        public void ReleaseReaderLock() {
            Monitor.Enter(cond);

            // 更新状态，读者退出
            readingCnt --;
            Monitor.PulseAll(cond);// 唤醒所有人
            Monitor.Exit(cond);
        }

        //------------------//
        // 没有读者或写者占用锁时，才可以获取 lock
        public void AcquireWriterLock() {
            Monitor.Enter(cond);
            // 这样写者才不会饥饿
            writeWaitingCnt ++; // 当前有写者正在等待
            // 这样后续的读线程会注意到有写者正在等待，就不会一直读

            while(readingCnt > 0 || writing) {
                Monitor.Wait(cond);
            }
            writeWaitingCnt--; // 结束等待
            writing = true; // 接下来轮到当前写者了
            Monitor.Exit(cond);
        }
        public void ReleaseWriterLock() {
            Monitor.Enter(cond);

            writing = false;
            Monitor.PulseAll(cond);
            Monitor.Exit(cond);
        }

        // ------------- timeouts version -------------- //
        public void AcquireWriterLock(int timeout) {
            AcquireLockTimeoutBase(timeout, AcquireWriterLock);
        }

        public void AcquireReaderLock(int timeout) {
            AcquireLockTimeoutBase(timeout, AcquireReaderLock);
        }

        // ------------ timeouts helper --------------- //
        private delegate void AqLock();
        private static void AcquireLockTimeoutBase(int timeout, AqLock aqLock) {
            //
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            // CancellationToken token=new CancellationToken();
            
            bool isdone = false;
            // 直接调 timer 异常不是在 main thread 抛出的
            // 无法捕捉
            // var timer = new Timer(statusChecker.checkDone, null, timeout, Timeout.Infinite);
            Task.Factory.StartNew(() =>
           {
                aqLock();
                isdone = true;
           },
           tokenSource.Token);
            //
            Thread.Sleep(timeout);
            if(!isdone) {
                // 超时取消任务
                // 抛异常
                tokenSource.Cancel();
                throw new ApplicationException();
            }
        }


        // private class StatusChecker {
        //     public bool isdone = false;
        //     // check if its true
        //     public void checkDone(/*Object placeHolder*/) {
        //         if(!isdone) {
        //             // increment
        //             throw new ApplicationException("Time out!");
        //         } else {
        //             // Console.WriteLine("YES");
        //         }
        //     }
        //     public void setTrue() {
        //         isdone = true;
        //     }        
        // }

        // public static void Main() {
        //     // OK
        // //    AcquireLockTimeoutBase(1200, aqLockStub); 
        // }

        // test stub
        private static void aqLockStub() {
            Thread.Sleep(1000);
        }
    }
}