namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFiniteStateMachine
	{
		public List<FiniteState> States { get; }

		public List<FiniteState> FinalSates { get; }
		
		public List<StateTransition> Transitions { get; }

		public void AddTransition(StateTransition transition);

		public bool RemoveTransition(StateTransition transition);

		public void AddState(FiniteState state);

		public FiniteState? FindState(CanvasCoordinate coordinate);
		
		public StateTransition? FindTransition(CanvasCoordinate coordinate);

		public bool RemoveState(FiniteState stateToBeRemoved);
	}
}
