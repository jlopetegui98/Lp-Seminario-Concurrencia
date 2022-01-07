namespace MyBarrier;

public class MyBarrier{
    //contador del numero de fases que se han ejecutado
    private int _currentPhase;
    //contador para saber el numero de participantes que no han completado la fase
    private int _partRemaining;
    //total de participantes en la barrera
    private int _participants;
    //metodo que se ejecuta despues de cada fase
    private Action<MyBarrier>? _postPhaseMethod = null;
    //objeto para sincronizar usando el monitor
    private static object _locker = new object();
    //constructor de la clase
    public MyBarrier(int participants)
    {
        if(participants < 0 || participants > 32677)
            throw new ArgumentOutOfRangeException();
        this._participants = participants;
        this._currentPhase = 0;
        this._partRemaining = participants; 
    }
    //sobrecarga del constructor que recibe el postPhaseAction 
    public MyBarrier(int participants, Action<MyBarrier> postPhaseMethod)
    {
        if(participants < 0 || participants > 32677)
            throw new ArgumentOutOfRangeException();
        this._participants = participants;
        this._currentPhase = 0;
        this._partRemaining = participants;
        this._postPhaseMethod = postPhaseMethod; 
    } 
    //Propiedades
    public int CurrentPhaseNumber => this._currentPhase;
    public int ParticipantCount => this._participants;
    public int ParticipantsRemaining => this._partRemaining;

    public void AddParticipant(){
        AddParticipants(1);
    }
    //implementacion del metodo AddParticipant
    public void AddParticipants(int n){
        lock(_locker){
            this._participants += n;
            this._partRemaining += n;
            if(this._partRemaining <= 0)
            {
                try{EndPhase();}
                catch(Exception e) {
                    Monitor.PulseAll(_locker);
                    throw new Exception(e.Message);
                }
                Monitor.PulseAll(_locker);
            }
        }
    }
    public void RemoveParticipant(){
        AddParticipants(-1);
    }
    //Implementacion del metodo RemoveParticipant, se utiliza el metodo AddParticipant
    public void RemoveParticipants(int n){
        AddParticipants(-n);
    }
    //metodo que se ejecuta al finnalizar una fase
    private void EndPhase(){
        //si no es null el postPhaseAction se ejecuta de forma segura
        if (_postPhaseMethod != null)
            try{this._postPhaseMethod(this);}
            catch(Exception e){
                this._currentPhase += 1;
                this._partRemaining = this._participants;
                throw new Exception(e.Message);
            }
        //se actualiza el estado de la barrera
        this._currentPhase += 1;
        this._partRemaining = this._participants;
    }
    //implementacion del metodo SignalAndWait
    public void SignalAndWait(){
        lock(_locker){
            this._partRemaining -= 1;
            //si con este participante se completa la fase, se llama el metodo EndPhase
            if(this._partRemaining == 0)
            {
                try{EndPhase();}
                catch(Exception e) {
                    Monitor.PulseAll(_locker);
                    throw new Exception(e.Message);
                }
                Monitor.PulseAll(_locker);
            }
            //si no se termina la fase se espera la notificacion a traves del objeto _locker para continuar la ejecucion
            else
            {
                Monitor.Wait(_locker);
            }
        }
    }


}

public class BarrierExample
{
    public static void MainExample()
    {
        int count = 0;

        //Se crea una barrera con 3 participantes y un postPhaseAction que al concluir la fase 2 lanzara una exception
        MyBarrier barrier = new MyBarrier(3, (b) =>
        {
            Console.WriteLine("Post-Phase action: count={0}, phase={1}", count, b.CurrentPhaseNumber);
            if (b.CurrentPhaseNumber == 2) throw new Exception("D'oh!");
        });

        // Se agregan dos participantes a la barrera(5)
        barrier.AddParticipants(2);

        // Se elimina un participante(4)
        barrier.RemoveParticipant();

        // Metodo a ejecutar por cada participante
        Action action = () =>
        {
            Interlocked.Increment(ref count);//se incrementa en 1 el valor de count
            barrier.SignalAndWait(); // al llegar al post-phase, el valor de count debe ser 4 y la fase 0
            Interlocked.Increment(ref count);
            barrier.SignalAndWait(); // al llegar al post-phase, el valor de count debe ser 4 y la fase 1

            // En el tercer llamado se lanzara un exception al ejecutar el postPhaseAction
            Interlocked.Increment(ref count);
            try
            {
                barrier.SignalAndWait();
            }
            catch (Exception bppe)
            {
                Console.WriteLine("Caught BarrierPostPhaseException: {0}", bppe.Message);
            }

            // La cuarta fase debe correrse sin problema pese a la exception capturada
            Interlocked.Increment(ref count);
            barrier.SignalAndWait(); // al llegar al post-phase, el valor de count debe ser 16 y la fase 3
        };

        // Se llaman cuatro ejecuciones en paralelo del metodo action que sirven como participantes de la barrera
        Parallel.Invoke(action, action, action, action);
    }
}