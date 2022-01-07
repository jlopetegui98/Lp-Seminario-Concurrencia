namespace MySemaphore;

public class MySemaphore{
    // contador del semaforo
    private int count;
    // numero maximo de hebras que pueden acceder al semaforo
    private int capacity;
    // objeto que se usara en el monitor
    private static object not_full = new object();

    // constructor de la clase
    public MySemaphore(int init_count, int max_count)
    {
        if (init_count > max_count)
            throw new Exception("El número de hebras disponibles debe ser menor que el número máximo permitido.");
        this.count = init_count;
        this.capacity = max_count;
    }

    // metodo WaitOne
    public void WaitOne(){
        //Se bloquea el objeto not_full
        lock(not_full)
        {   
            //mientras el contador sea 0 se espera una notificacion del objeto not_full
            while(this.count == 0 ){
            Monitor.Wait(not_full);
        }
            //como ya la hebra que ejecuta el metodo accedio al semaforo se decrementa el valor del contador en 1
            this.count -= 1;
            if(this.count < 0)
                throw new Exception("El número de hebras disponibles debe ser positivo.");
        }
    }
    // implementacion del metodo Release
    public int Release(int n = 1){
        lock(not_full){this.count += n;
        if(this.count > this.capacity)
            throw new Exception("El número de hebras disponibles debe ser menor o igual que la capacidad.");
        
        // se notifica a traves del objeto not_full a todas las hebras que esten esperando por el monitor
        Monitor.PulseAll(not_full);
        
        return this.count - n;}
    }


}

public class SemaphoreExample{
    
    private static MySemaphore _pool;

    private static int _padding;

    public static void MainExample()
    {
        _pool = new MySemaphore(0, 3);

        // Se crean 5 hebras con el metodo Worker
        for(int i = 1; i <= 5; i++)
        {
            Thread t = new Thread(new ParameterizedThreadStart(Worker));

            // Inicia la ejecucion de la hebra correspondiente pasandole el entero i
            t.Start(i);
        }

        // Se duerme el proceso durante medio segundo
        Thread.Sleep(500);

        
        Console.WriteLine("La hebra principal ejecuta Release(3). Libera los tres espacios del semaforo");
        _pool.Release(3);

        Console.WriteLine("Concluye la hebra principal.");
    }

    private static void Worker(object num)
    {
        // Cada hebra comienza a tratar de acceder al semaforo
        Console.WriteLine("La hebra {0} comienza " +
            "y espera por el semaforo.", num);
        _pool.WaitOne();

        // Este valor se utiliza para hacer la salida mas ordenada
        int padding = Interlocked.Add(ref _padding, 100);

        Console.WriteLine("La hebra {0} entra al semaforo.", num);
        
        // Cada hebra detiene sue ejcucion alrededor de 1 segundo
        //
        Thread.Sleep(1000 + padding);

        Console.WriteLine("La hebra {0} deja el semaforo.", num);
        Console.WriteLine("El contador del semaforo previo a la salida de la hebra {0}: {1}",
            num, _pool.Release());
    }
    //output Example:
    // La hebra 4 comienza y espera por el semaforo.
    // La hebra 3 comienza y espera por el semaforo.
    // La hebra 1 comienza y espera por el semaforo.
    // La hebra 2 comienza y espera por el semaforo.
    // La hebra 5 comienza y espera por el semaforo.
    // La hebra principal ejecuta Release(3). Libera los tres espacios del semaforo
    // Concluye la hebra principal.
    // La hebra 1 entra al semaforo.
    // La hebra 5 entra al semaforo.
    // La hebra 3 entra al semaforo.
    // La hebra 1 deja el semaforo.
    // La hebra 4 entra al semaforo.
    // El contador del semaforo previo a la salida de la hebra 1: 0
    // La hebra 5 deja el semaforo.
    // El contador del semaforo previo a la salida de la hebra 5: 0
    // La hebra 2 entra al semaforo.
    // La hebra 3 deja el semaforo.
    // El contador del semaforo previo a la salida de la hebra 3: 0
    // La hebra 4 deja el semaforo.
    // El contador del semaforo previo a la salida de la hebra 4: 1
    // La hebra 2 deja el semaforo.
    // El contador del semaforo previo a la salida de la hebra 2: 2
}