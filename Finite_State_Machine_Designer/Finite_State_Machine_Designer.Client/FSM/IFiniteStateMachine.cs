namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFiniteStateMachine
	{
		public List<FiniteState> States { get; }

		public List<FiniteState> FinalSates { get; }

		public void AddState(FiniteState state);

		public FiniteState? FindState(CanvasCoordinate coordinate);

		public bool RemoveState(FiniteState stateToBeRemoved);
	}
}
