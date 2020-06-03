using System;
using System.Threading;

class Program {
    static object cond = new object();

    static int H = 0, O = 0;
    static int thrd = 1;

    static void before(char c) {
        Monitor.Enter(cond);
        while(
            (!(c=='H'&&H<2) && !(c=='O'&&O<1)) || thrd == 0
        ) Monitor.Wait(cond);

        thrd --;
        Monitor.Exit(cond);
    }

    static void after(char c) {
        Monitor.Enter(cond);
        
        if(c== 'H') H++;
        else O++;

        if(H+O==3) {
            H=O=0;
            Console.WriteLine();
        }

        thrd++;
        
        Monitor.PulseAll(cond);
        Monitor.Exit(cond);
    }

    static void worker() {
        var ch = Thread.CurrentThread.Name[0];
        while(true) {
            before(ch);
            Console.Write(ch);
            after(ch);
        }
    }

    static void Main() {
        String[] roles = {"H", "O"};
        foreach (var s in roles) {
            var t = new Thread(worker);
            t.Name = s;
            t.Start();
        }
    }
}