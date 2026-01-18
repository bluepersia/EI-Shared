using EI.Shared;
namespace EI.Shared.Tests;

public class FSMTest
{
    public class TestFSM : FSM<FSMTest>
    {
        public TestFSM(FSMTest owner) : base(owner) { }
    }

    public class SpyState : FSMState<TestFSM>
    {
        public SpyState(TestFSM fsm) : base(fsm) { }

        public int EnterCalls { get; private set; } = 0;
        public int ExitCalls { get; private set; } = 0;
        public int UpdateCalls { get; private set; } = 0;

        protected override void OnEnter() => EnterCalls++;
        protected override void OnExit() => ExitCalls++;

        protected override void OnUpdate() => UpdateCalls++;
    }
    public class IdleModel : SpyState
    {
        public IdleModel(TestFSM fsm) : base(fsm) { }
    }

    public class IdleView : SpyState
    {
        public IdleView(TestFSM fsm) : base(fsm) { }
    }

    public class MoveModel : SpyState
    {
        public MoveModel(TestFSM fsm) : base(fsm) { }
    }


    public static int IDLE = 0;
    public static int MOVE = 1;

    private void AssertZero(SpyState state)
    {
        Assert.Equal(0, state.EnterCalls);
        Assert.Equal(0, state.ExitCalls);
        Assert.Equal(0, state.UpdateCalls);
    }

    [Fact]
    public void FullFlow()
    {
        TestFSM fsm = new TestFSM(this);

        IdleModel idleModel = new IdleModel(fsm);
        IdleView idleView = new IdleView(fsm);
        MoveModel moveModel = new MoveModel(fsm);

        fsm.Register(IDLE, idleModel);
        fsm.Register(IDLE, idleView);
        fsm.Register(MOVE, moveModel);

        Assert.False(fsm.IsStarted);
        Assert.Equal(-1, fsm.CurrentStateId);
        Assert.Equal(0, fsm.Epoch);
        AssertZero(idleModel);
        AssertZero(idleView);
        AssertZero(moveModel);


        fsm.SetState(IDLE);

        Assert.True(fsm.IsStarted);

        fsm.Update();

        Assert.Equal(IDLE, fsm.CurrentStateId);
        Assert.Equal(1, fsm.Epoch);

        Assert.Equal(1, idleModel.EnterCalls);
        Assert.Equal(0, idleModel.ExitCalls);
        Assert.Equal(1, idleModel.UpdateCalls);
        Assert.Equal(1, idleView.EnterCalls);
        Assert.Equal(0, idleView.ExitCalls);
        Assert.Equal(1, idleView.UpdateCalls);
        AssertZero(moveModel);

        fsm.SetState(MOVE);

        Assert.True(fsm.IsStarted);

        fsm.Update();

        Assert.Equal(MOVE, fsm.CurrentStateId);
        Assert.Equal(2, fsm.Epoch);

        Assert.Equal(1, idleModel.EnterCalls);
        Assert.Equal(1, idleModel.ExitCalls);
        Assert.Equal(1, idleModel.UpdateCalls);
        Assert.Equal(1, idleView.EnterCalls);
        Assert.Equal(1, idleView.ExitCalls);
        Assert.Equal(1, idleView.UpdateCalls);


        Assert.Equal(1, moveModel.EnterCalls);
        Assert.Equal(0, moveModel.ExitCalls);
        Assert.Equal(1, moveModel.UpdateCalls);

        fsm.SetState(MOVE);

        Assert.Equal(1, moveModel.EnterCalls);
        Assert.Equal(0, moveModel.ExitCalls);

        fsm.EnterState(MOVE);

        Assert.Equal(2, moveModel.EnterCalls);
        Assert.Equal(1, moveModel.ExitCalls);



    }

    [Fact]
    public void DuplicateRegister()
    {
        TestFSM fsm = new TestFSM(this);

        IdleModel idleModel = new IdleModel(fsm);
        IdleModel idleModel2 = new IdleModel(fsm);

        fsm.Register(IDLE, idleModel);

        Assert.Throws<InvalidOperationException>(() =>
        {
            fsm.Register(MOVE, idleModel2);
        });

        Assert.Empty(fsm.GetStates(MOVE));

    }

    [Fact]
    public void NotRegisteredState()
    {
        TestFSM fsm = new TestFSM(this);

        Assert.Throws<KeyNotFoundException>(() =>
          {
              fsm.EnterState(IDLE);
          });
    }
}