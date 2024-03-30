namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteStateMachine : IFiniteStateMachine
	{
		public List<FiniteState> States => _states;
		private readonly List<FiniteState> _states = [];
		private readonly List<StateTransition> _transitions = [];

		public List<FiniteState> FinalSates => _states.Where(x => x.IsFinalState).ToList();

		public List<StateTransition> Transitions => _transitions;

		public void AddState(FiniteState state)
		{
			_states.Add(state);
		}

		public FiniteState? FindState(CanvasCoordinate coordinate)
		{
			// Equation of Circle -> (x-a)^2 + (y-b)^2 <= r^2
			// where (a,b) are x and y coordinates of the centre of the circle respectively
			// r is radius of circle
			foreach (var state in _states)
			{
				var coord = state.Coordinate;
				var leftSide = Math.Pow(coordinate.X - coord.X, 2) + Math.Pow(coordinate.Y - coord.Y, 2);
				var rightSide = Math.Pow(state.Radius, 2);
				if (leftSide <= rightSide)
					return state;
			}
			return null;
		}

		public bool RemoveState(FiniteState stateToBeRemoved)
		{
			foreach (var state in _states)
				if (stateToBeRemoved == state)
					return _states.Remove(state);
			return false;
		}

		public void AddTransition(StateTransition stateTransition)
		{
			_transitions.Add(stateTransition);
		}

		public bool RemoveTransition(StateTransition transition)
		{
			return _transitions.Remove(transition);
		}

		public StateTransition? FindTransition(CanvasCoordinate coordinate)
		{
			throw new NotImplementedException();
		}
	}
}
