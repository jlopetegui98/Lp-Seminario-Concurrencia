using MySemaphore;
using MyBarrier;
using MyCountDown;

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
        else
        {
            throw new Exception("Invalid Argument.");
        }
    }
}