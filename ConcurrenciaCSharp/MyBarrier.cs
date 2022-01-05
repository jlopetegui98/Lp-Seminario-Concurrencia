namespace MyBarrier;

public class MyBarrier{
    private int _currentPhase;
    private int _partRemaining;
    private int _participants;
    private Action<MyBarrier>? _postPhaseMethod = null;
    private static object _locker = new object();
    public MyBarrier(int participants)
    {
        if(participants < 0 || participants > 32677)
            throw new ArgumentOutOfRangeException();
        this._participants = participants;
        this._currentPhase = 0;
        this._partRemaining = participants; 
    } 
    public MyBarrier(int participants, Action<MyBarrier> postPhaseMethod)
    {
        if(participants < 0 || participants > 32677)
            throw new ArgumentOutOfRangeException();
        this._participants = participants;
        this._currentPhase = 0;
        this._partRemaining = participants;
        this._postPhaseMethod = postPhaseMethod; 
    } 
    public int CurrentPhaseNumber => this._currentPhase;
    public int ParticipantCount => this._participants;
    public int ParticipantsRemaining => this._partRemaining;

    public void AddParticipant(){
        AddParticipants(1);
    }
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
    public void RemoveParticipants(int n){
        AddParticipants(-n);
    }
    private void EndPhase(){
        if (_postPhaseMethod != null)
            try{this._postPhaseMethod(this);}
            catch(Exception e){
                this._currentPhase += 1;
                this._partRemaining = this._participants;
                throw new Exception(e.Message);
            }
        this._currentPhase += 1;
        this._partRemaining = this._participants;
    }
    public void SignalAndWait(){
        lock(_locker){
            this._partRemaining -= 1;
            if(this._partRemaining == 0)
            {
                try{EndPhase();}
                catch(Exception e) {
                    Monitor.PulseAll(_locker);
                    throw new Exception(e.Message);
                }
                Monitor.PulseAll(_locker);
            }
            else
            {
                Monitor.Wait(_locker);
            }
        }
    }


}

public class BarrierExample
{
    // Demonstrates:
    //      Barrier constructor with post-phase action
    //      Barrier.AddParticipants()
    //      Barrier.RemoveParticipant()
    //      Barrier.SignalAndWait()
    public static void MainExample()
    {
        int count = 0;

        // Create a barrier with three participants
        // Provide a post-phase action that will print out certain information
        // And the third time through, it will throw an exception
        MyBarrier barrier = new MyBarrier(3, (b) =>
        {
            Console.WriteLine("Post-Phase action: count={0}, phase={1}", count, b.CurrentPhaseNumber);
            if (b.CurrentPhaseNumber == 2) throw new Exception("D'oh!");
        });

        // Nope -- changed my mind.  Let's make it five participants.
        barrier.AddParticipants(2);

        // Nope -- let's settle on four participants.
        barrier.RemoveParticipant();

        // This is the logic run by all participants
        Action action = () =>
        {
            Interlocked.Increment(ref count);
            barrier.SignalAndWait(); // during the post-phase action, count should be 4 and phase should be 0
            Interlocked.Increment(ref count);
            barrier.SignalAndWait(); // during the post-phase action, count should be 8 and phase should be 1

            // The third time, SignalAndWait() will throw an exception and all participants will see it
            Interlocked.Increment(ref count);
            try
            {
                barrier.SignalAndWait();
            }
            catch (Exception bppe)
            {
                Console.WriteLine("Caught BarrierPostPhaseException: {0}", bppe.Message);
            }

            // The fourth time should be hunky-dory
            Interlocked.Increment(ref count);
            barrier.SignalAndWait(); // during the post-phase action, count should be 16 and phase should be 3
        };

        // Now launch 4 parallel actions to serve as 4 participants
        Parallel.Invoke(action, action, action, action);
    }
}