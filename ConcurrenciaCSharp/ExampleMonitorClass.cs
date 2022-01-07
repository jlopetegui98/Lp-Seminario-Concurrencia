namespace MonitorExample;

public class MonExample{
    static object obj = new object();
    public static void Print(object msg){
        Monitor.Enter(obj); 
        try{
            Monitor.Wait(obj);
        }
        finally{
            Monitor.Exit(obj);
        }
        System.Console.WriteLine("Se termino la " + msg);
    }
    static void PulsePrint(){
        Thread.Sleep(2000);
        Monitor.Enter(obj);
        try{
            System.Console.WriteLine("Usando Pulse...");
            Monitor.Pulse(obj);
        }
        finally{
            Monitor.Exit(obj);
        }
    }
    static void PulseAllPrint(){
        Thread.Sleep(5000);
        Monitor.Enter(obj);
        try{
            System.Console.WriteLine("Usando PulseAll");
            Monitor.PulseAll(obj);
        }
        finally{
            Monitor.Exit(obj);
        }
    }
    public static void MainExample(){
        Thread hebra1 = new Thread(new ParameterizedThreadStart(Print));
        Thread hebra2 = new Thread(new ParameterizedThreadStart(Print));
        Thread hebra3 = new Thread(new ParameterizedThreadStart(Print));
        Thread hebraPulse = new Thread(PulsePrint);
        Thread hebraPulseAll = new Thread(PulseAllPrint);
        hebra1.Start("hebra1...");
        hebra2.Start("hebra2...");
        hebra3.Start("hebra3...");
        hebraPulse.Start();
        hebraPulseAll.Start();
        //Example output:
        // Usando Pulse...
        // Se termino la hebra2...
        // Usando PulseAll
        // Se termino la hebra3...
        // Se termino la hebra1...
    }
}