namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFiniteStateMachine
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public int Width { get; set; }
        public int Height { get; set; }
        public List<FiniteState> States { get; set; }

		public List<FiniteState> FinalStates { get; }

		public List<FiniteState> InitialStates { get; }
		
		public List<Transition> Transitions { get; set; }

		public int TransitionSearchRadius { get; set; }

		public void AddTransition(Transition transition);

		public bool RemoveTransition(Transition transition);

		public void AddState(FiniteState state);

		public FiniteState? FindState(CanvasCoordinate coordinate);
		
		public Transition? FindTransition(CanvasCoordinate coordinate);

		public List<Transition> FindTransitions(FiniteState? state, Predicate<Transition>? filter = null);

		public bool RemoveState(FiniteState stateToBeRemoved);
	}
}
