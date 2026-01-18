namespace EI.Shared;

public class FSM
{
    public int Epoch { get; private set; } = 0;
    public int CurrentStateId { get; private set; } = -1;


    private readonly Dictionary<int, List<FSMState>> _states = new Dictionary<int, List<FSMState>>();
    public void Register(int stateId, FSMState state)
    {
        _states.TryAdd(stateId, new List<FSMState>());

        if (_states[stateId].Any(s => s.GetType() == state.GetType()))
        {
            throw new InvalidOperationException(
                $"State type {state.GetType().Name} already registered for state {stateId}");
        }

        _states[stateId].Add(state);
    }


    public void Start(int initialStateId)
    {
        if (CurrentStateId != -1)
        {
            throw new InvalidOperationException("FSM has already been started.");
        }


        if (!_states.TryGetValue(initialStateId, out var states))
        {
            throw new InvalidOperationException(
                $"Cannot start FSM: state {initialStateId} not registered.");
        }

        CurrentStateId = initialStateId;
        Epoch = 0;

        foreach (var state in states)
            state.Enter();
    }


    /// <summary>
    ///  Returns the first state of type T registered for the given stateId, or null if none exists.
    /// </summary>
    public T? GetState<T>(int stateId) where T : FSMState
    {
        if (!_states.TryGetValue(stateId, out var states))
        {
            return null;
        }
        return states.OfType<T>().FirstOrDefault();
    }
    public T GetRequiredState<T>(int stateId) where T : FSMState
    {
        if (!_states.TryGetValue(stateId, out var states))
        {
            throw new KeyNotFoundException(
                $"Cannot get state {stateId}: state not registered.");
        }

        T? state = states.OfType<T>().SingleOrDefault();

        if (state == null)
        {
            throw new InvalidOperationException(
                $"Expected exactly one state of type {typeof(T).Name} for state {stateId}, but none was found.");
        }

        return state;
    }


    public void SetState(int stateId, bool preserveEpoch = false)
    {
        if (CurrentStateId == stateId)
        {
            return;
        }

        EnterState(stateId, preserveEpoch);
    }

    public void EnterState(int stateId, bool preserveEpoch = false)
    {
        if (!_states.TryGetValue(stateId, out var nextStates))
        {
            throw new InvalidOperationException(
                $"Cannot enter state {stateId}: state not registered.");
        }

        if (_states.TryGetValue(CurrentStateId, out var currentStates))
        {
            foreach (var state in currentStates)
                state.Exit();
        }


        if (!preserveEpoch)
        {
            Epoch++;
        }
        CurrentStateId = stateId;

        foreach (var state in nextStates)
            state.Enter();
    }



    public void Update()
    {
        if (!_states.TryGetValue(CurrentStateId, out var states))
            return;

        foreach (var state in states)
            state.Update();
    }

}


public class FSM<T> : FSM
{
    public T Owner { get; private set; }
    public FSM(T owner)
    {
        Owner = owner;
    }
}

public abstract class FSMState
{

    public void Enter()
    {
        OnEnter();
    }

    public void Update()
    {
        OnUpdate();
    }

    public void Exit()
    {
        OnExit();
    }

    protected virtual void OnEnter()
    {

    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnExit()
    {

    }
}

public abstract class FSMState<TFSM> : FSMState where TFSM : FSM
{
    protected TFSM FSM { get; private set; }

    public FSMState(TFSM fsm)
    {
        FSM = fsm;
    }
}

public abstract class FSMState<TFSM, TParent> : FSMState<TFSM> where TFSM : FSM
{

    public TParent Parent { get; private set; }

    public FSMState(TFSM fsm, TParent parent) : base(fsm)
    {
        Parent = parent;
    }
}


