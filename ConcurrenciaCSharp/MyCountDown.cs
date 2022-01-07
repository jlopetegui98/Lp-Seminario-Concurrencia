namespace MyCountDown;

public class MyCountdownEvent{
    //contador que mantiene el numero de notificaciones que se necesitan recibir para desbloquear el countdown
    private int _counter;
    //valor inicial del contador
    private int _initialCount;
    //objeto que se utiliza para sincronizar usando la clase Monitor
    private static object _locker = new object();
    //constructor de la clase
    public MyCountdownEvent(int counter)
    {
        this._counter = counter;
        this._initialCount = counter;
    }
    //Propiedades
    public int InitialCount => _initialCount;
    public int CurrentCount => _counter;

    public bool IsSet => _counter == 0;
    //Implementacion del metodo wait
    public void Wait(){
        lock(_locker){
            //mientras el contador sea mayor que 0 espera notificacion del monitor a traves del objeto _locker
            while(this._counter > 0){
                Monitor.Wait(_locker);
            }
        }
    }
    //implementacion del metodo Signal
    public void Signal(){
        //Se decrementa el valor del contador
        AddCount(-1);
    }
    //implementacion del metodos Reset, devuelve el countdown a su estado original
    public void Reset(){
        lock(_locker) this._counter = this._initialCount;
    }
    //reinicia el countdown con un nuevo valor inicial
    public void Reset(int counter){
        lock(_locker) {
            this._counter = counter;
            this._initialCount = counter;
        }
    }
    //aumenta el valor del contador(disminuye si n es negativo)
    public void AddCount(int n = 1){
        lock(_locker){
            this._counter += n;
            //si el contador se hace 0 se notifica a trav\'es del objeto _locker a la hebra que espera por el countdown 
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
        new Thread (SaySomething).Start ("Yo soy la hebra 1");
        new Thread (SaySomething).Start ("Yo soy la hebra 2");
        new Thread (SaySomething).Start ("Yo soy la hebra 3");
 
        _countdown.Wait();   // Bloquea la ejecucion de la hebra hasta que se llame 3 veces el metodo Signal de _countdown
        Console.WriteLine ("Todas las hebras han terminado de hablar!");
    }
 
    static void SaySomething (object thing)
    {
        Thread.Sleep (1000);
        Console.WriteLine (thing);
        _countdown.Signal();
    }
}