namespace EI.Shared;

public class FSM
{
    public int Epoch { get; private set; } = 0;
    public int CurrentStateId { get; private set; } = -1;

    private Dictionary<int, List<FSMState>> _states = new Dictionary<int, List<FSMState>>();
    public void Register(int stateId, FSMState state)
    {
        if (!_states.ContainsKey(stateId))
        {
            _states[stateId] = new List<FSMState>();
        }
        _states[stateId].Add(state);
    }

    public T? GetState<T>(int stateId) where T : FSMState
    {
        if (!_states.ContainsKey(stateId))
        {
            return null;
        }
        return _states[stateId].OfType<T>().FirstOrDefault();
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
        if (!_states.ContainsKey(stateId))
        {
            throw new InvalidOperationException($"Cannot enter state {stateId}: state not registered.");
        }

        if (_states.ContainsKey(CurrentStateId))
        {
            _states[CurrentStateId].ForEach(state => state.Exit());
        }

        if (!preserveEpoch)
        {
            Epoch++;
        }
        CurrentStateId = stateId;
        _states[CurrentStateId].ForEach(state => state.Enter());
    }



    public void Update()
    {
        if (!_states.ContainsKey(CurrentStateId))
            return;

        _states[CurrentStateId].ForEach(state => state.Update());
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

    public virtual void Enter()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {

    }
}

public abstract class FSMState<T> : FSMState
{
    public T Parent { get; private set; }

    public FSMState(T parent)
    {
        Parent = parent;
    }
}


public class IdleState : FSMState<FSM>
{
    public IdleState(FSM parent) : base(parent) { }

    public override void Update()
    {
        if (Parent.CurrentStateId != 0)
        {

        }
    }
}