namespace EI.Shared;

public class FSM
{
    public int Epoch { get; private set; } = 0;

    public int CurrentStateId { get; private set; } = -1;
    public bool IsStarted => CurrentStateId != -1;


    private Dictionary<int, List<FSMState>> _registry = new Dictionary<int, List<FSMState>>();


    ///<summary>
    /// Registers provided state object with the provided state Id.
    /// A state object can only be registered once per type.
    /// State object must belong to this FSM.
    /// </summary>
    public void Register(int stateId, FSMState state)
    {
        state.ValidateOwnership(this);

        _registry.TryAdd(stateId, new List<FSMState>());

        if (_registry.Values.Any(list => list.Any(s => s.GetType() == state.GetType())))
        {
            throw new InvalidOperationException($"{state.GetType().ToString()} is already registered.");
        }

        _registry[stateId].Add(state);
    }



    /// <summary>
    /// Enters the provided registered state, if not already in this state.
    /// </summary>
    public void SetState(int newStateId, bool preserveEpoch = false)
    {
        if (newStateId == CurrentStateId)
        {
            return;
        }

        EnterState(newStateId, preserveEpoch);
    }

    /// <summary>
    /// Forces an entry into the provided registered state.
    /// If the FSM is already in the provided state, it is re-entered.
    /// </summary>
    public void EnterState(int stateId, bool preserveEpoch = false)
    {
        if (!_registry.TryGetValue(stateId, out var nextStates))
        {
            throw new KeyNotFoundException($"State ${stateId} is not registered, yet you tried to enter it.");
        }

        if (_registry.TryGetValue(CurrentStateId, out var currentStates))
        {
            currentStates.ForEach(state => state.Exit());
        }

        CurrentStateId = stateId;

        if (!preserveEpoch)
            Epoch++;

        nextStates.ForEach(state => state.Enter());
    }

    public void Update()
    {
        if (_registry.TryGetValue(CurrentStateId, out var states))
        {
            states.ForEach(state => state.Update());
        }
    }
}


public abstract class FSMState
{
    public virtual void ValidateOwnership(FSM fsm)
    {
    }
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

    public FSMState(TFSM fsm) : base()
    {
        FSM = fsm;
    }
    public sealed override void ValidateOwnership(FSM fsm)
    {
        if (FSM != fsm)
            throw new InvalidOperationException("Cross-FSM encountered");
    }
}

public abstract class FSMState<TFSM, TParent> : FSMState<TFSM> where TFSM : FSM
{
    protected TParent Parent { get; private set; }

    public FSMState(TFSM fsm, TParent parent) : base(fsm)
    {
        Parent = parent;
    }
}
