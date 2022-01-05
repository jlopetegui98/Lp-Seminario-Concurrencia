namespace MySemaphore;

public class MySemaphore{
    private int count;
    private int capacity;
    // private object not_empty;
    private static object not_full = new object();

    public MySemaphore(int init_count, int max_count)
    {
        if (init_count > max_count)
            throw new Exception("El número de hebras disponibles debe ser menor que el número máximo permitido.");
        this.count = init_count;
        this.capacity = max_count;
        // this.not_empty = new object();
        // this.not_full = new object();
    }
    public void WaitOne(){
        lock(not_full)
        {   while(this.count == 0 ){
            Monitor.Wait(not_full);
        }
        
            this.count -= 1;
            if(this.count < 0)
                throw new Exception("El número de hebras disponibles debe ser positivo.");
        }
    }
    public bool WaitOne(int time){
        return true;
    }
    public int Release(int n = 1){
        lock(not_full){this.count += n;
        if(this.count > this.capacity)
            throw new Exception("El número de hebras disponibles debe ser menor o igual que la capacidad.");
        
        Monitor.PulseAll(not_full);
        
        return this.count - n;}
    }


}

public class SemaphoreExample{
            // A semaphore that simulates a limited resource pool.
    //
    private static MySemaphore _pool;

    // A padding interval to make the output more orderly.
    private static int _padding;

    public static void MainExample()
    {
        // Create a semaphore that can satisfy up to three
        // concurrent requests. Use an initial count of zero,
        // so that the entire semaphore count is initially
        // owned by the main program thread.
        //
        _pool = new MySemaphore(0, 3);

        // Create and start five numbered threads. 
        //
        for(int i = 1; i <= 5; i++)
        {
            Thread t = new Thread(new ParameterizedThreadStart(Worker));

            // Start the thread, passing the number.
            //
            t.Start(i);
        }

        // Wait for half a second, to allow all the
        // threads to start and to block on the semaphore.
        //
        Thread.Sleep(500);

        // The main thread starts out holding the entire
        // semaphore count. Calling Release(3) brings the 
        // semaphore count back to its maximum value, and
        // allows the waiting threads to enter the semaphore,
        // up to three at a time.
        //
        Console.WriteLine("Main thread calls Release(3).");
        _pool.Release(3);

        Console.WriteLine("Main thread exits.");
    }

    private static void Worker(object num)
    {
        // Each worker thread begins by requesting the
        // semaphore.
        Console.WriteLine("Thread {0} begins " +
            "and waits for the semaphore.", num);
        _pool.WaitOne();

        // A padding interval to make the output more orderly.
        int padding = Interlocked.Add(ref _padding, 100);

        Console.WriteLine("Thread {0} enters the semaphore.", num);
        
        // The thread's "work" consists of sleeping for 
        // about a second. Each thread "works" a little 
        // longer, just to make the output more orderly.
        //
        Thread.Sleep(1000 + padding);

        Console.WriteLine("Thread {0} releases the semaphore.", num);
        Console.WriteLine("Thread {0} previous semaphore count: {1}",
            num, _pool.Release());
    }
}