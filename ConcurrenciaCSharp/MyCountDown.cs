namespace MyCountDown;

public class MyCountdownEvent{
    private int _counter;
    private int _initialCount;

    private static object _locker = new object();
    public MyCountdownEvent(int counter)
    {
        this._counter = counter;
        this._initialCount = counter;
    }
    public int InitialCount => _initialCount;
    public int CurrentCount => _counter;

    public bool IsSet => _counter == 0;
    public void Wait(){
        lock(_locker){
            while(this._counter > 0){
                Monitor.Wait(_locker);
            }
        }
    }
    public void Signal(){
        AddCount(-1);
    }
    public void Reset(){
        lock(_locker) this._counter = this._initialCount;
    }

    public void Reset(int counter){
        lock(_locker) {
            this._counter = counter;
            this._initialCount = counter;
        }
    }
    public void AddCount(int n){
        lock(_locker){
            this._counter += n;
            if(this._counter == 0)
                Monitor.PulseAll(_locker);
            else if(this._counter > _initialCount)
                throw new Exception("El contador no puede exceder el valor inicial");
        }
    }   
}

public class CountdownExample
{
    static MyCountdownEvent _countdown = new MyCountdownEvent (3);
 
    public static void MainExample()
    {
        new Thread (SaySomething).Start ("I am thread 1");
        new Thread (SaySomething).Start ("I am thread 2");
        new Thread (SaySomething).Start ("I am thread 3");
 
        _countdown.Wait();   // Blocks until Signal has been called 3 times
        Console.WriteLine ("All threads have finished speaking!");
    }
 
    static void SaySomething (object thing)
    {
        Thread.Sleep (1000);
        Console.WriteLine (thing);
        _countdown.Signal();
    }
}