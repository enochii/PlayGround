using System;
using System.Threading;


class Program {
    struct Rule {
        public State from, to;
        public char ch;
        public Rule(State from, char ch, State to) {
            this.from = from;
            this.ch = ch;
            this.to = to;
        }
    }
    enum State{
        A = 1, B, C, D, E, F, 
    }

    static Rule[] rules = {
        new Rule(State.A, '<', State.B),
        new Rule(State.B, '>', State.C),
        new Rule(State.C, '<', State.D),

        new Rule(State.A, '>', State.E),
        new Rule(State.E, '<', State.F),
        new Rule(State.F, '>', State.D),
        new Rule(State.D, '_', State.A),
    };

    // Mutex mutex = new Mutex();
    static Object cond = new object();


    static int current = (int)State.A;
    static int thrd = 1;
    
    static int next(char c) {
        for(int i=0; i<rules.Length; i++) {
            Rule rule = rules[i];
            if(current == (int)rule.from && rule.ch == c) {
                return (int)rule.to;
            }
        }
        return 0;
    }


    static void fish_before(char c) {
        Monitor.Enter(cond);
        while(next(c) == 0 || thrd == 0) {
            Monitor.Wait(cond);
        }
        thrd--; // 变成 0
        Monitor.Exit(cond);
    }

    static void fish_after(char c) {
        Monitor.Enter(cond);

        // update
        current = next(c);
        thrd++;

        Monitor.PulseAll(cond);
        Monitor.Exit(cond);
    }

    static String roles = "<>_";

    static void worker() {
        char c = Thread.CurrentThread.Name[0];
        while(true) {
            fish_before(c);
            Console.Write(c);
            if(c=='_') Console.WriteLine();
            fish_after(c);
        }
    }

    public static void Main() {
        foreach(char c in roles) {
            var t = new Thread(worker);
            t.Name = ""+c;
            t.Start();
        }
    }
}