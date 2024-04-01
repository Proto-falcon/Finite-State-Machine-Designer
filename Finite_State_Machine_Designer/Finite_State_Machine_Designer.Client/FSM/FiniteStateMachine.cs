using System.Drawing;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteStateMachine : IFiniteStateMachine
	{
		public List<FiniteState> States => _states;
		private readonly List<FiniteState> _states = [];
		private readonly List<StateTransition> _transitions = [];

		private int _transitionSearchRadius;
		public int TransitionSearchRadius
		{
			get => _transitionSearchRadius;
			set => _transitionSearchRadius = value;
		}

		public List<FiniteState> FinalStates => _states.Where(x => x.IsFinalState).ToList();

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
				CanvasCoordinate coord = state.Coordinate;
				double leftSide = Math.Pow(coordinate.X - coord.X, 2) + Math.Pow(coordinate.Y - coord.Y, 2);
				double rightSide = Math.Pow(state.Radius, 2);
				if (leftSide <= rightSide)
					return state;
			}
			return null;
		}

		public bool RemoveState(FiniteState stateToBeRemoved)
		{
			foreach (FiniteState state in _states)
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
			double squaredSearchRadius = _transitionSearchRadius * _transitionSearchRadius;
			foreach (StateTransition transition in _transitions)
			{
				// mx + c = -x/m + d
				// xm^2 + cm = -x + dm
				// x(m^2 + 1) = dm - cm
				// x = m(d - c)/(m^2 + 1)
				// where c = 0, x = md/(m^2 + 1)
				CanvasCoordinate dCoordTransition = transition.ToCoord - transition.FromCoord;
				double gradient = dCoordTransition.Y / dCoordTransition.X;
				double perdendicularGradient = -1 / gradient;
				CanvasCoordinate dCoord = coordinate - transition.FromCoord;
				double yIntercept = dCoord.Y - (perdendicularGradient * dCoord.X);

				double x = (gradient * yIntercept) / (Math.Pow(gradient, 2) + 1);
				double y = gradient * x;
				
				double distance = Math.Pow(x - dCoord.X, 2) + Math.Pow(y - dCoord.Y, 2);

				if (distance <= squaredSearchRadius)
					return transition;

			}
			return null;
		}
	}
}
