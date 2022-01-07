using MySemaphore;
using MyBarrier;
using MyCountDown;
using MonitorExample;

public class Program{
    public static void Main(string[] args){
        if(args.Length == 0)
            throw new Exception("Not argument received.");
        else if(args[0] == "Semaphore")
            SemaphoreExample.MainExample();
        else if(args[0] == "Barrier")
            BarrierExample.MainExample();
        else if(args[0] == "Countdown")
            CountdownExample.MainExample();
        else if(args[0] == "Monitor")
            MonExample.MainExample();
        else
        {
            throw new Exception("Invalid Argument.");
        }
    }
}