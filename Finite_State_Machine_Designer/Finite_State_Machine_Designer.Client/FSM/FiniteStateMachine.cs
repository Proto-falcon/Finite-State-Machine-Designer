﻿namespace Finite_State_Machine_Designer.Client.FSM
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

		public List<FiniteState> InitialStates => _transitions.Where(x => !x.FromState.IsDrawable && x.ToState.IsDrawable)
			.Select(x => x.ToState).ToList();

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
			double squareDistance;
			CanvasCoordinate dCoord;
			foreach (StateTransition transition in _transitions)
			{
				if (!transition.IsCurved)
				{
					// mx + c = -x/m + d
					// xm^2 + cm = -x + dm
					// x(m^2 + 1) = dm - cm
					// x = m(d - c)/(m^2 + 1)
					// where c = 0, x = md/(m^2 + 1)
					CanvasCoordinate dCoordTransition = transition.ToCoord - transition.FromCoord;
					double gradient = dCoordTransition.Y / dCoordTransition.X;
					double perdendicularGradient = -1 / gradient;
					dCoord = coordinate - transition.FromCoord;
					double yIntercept = dCoord.Y - (perdendicularGradient * dCoord.X);

					double x = (gradient * yIntercept) / (Math.Pow(gradient, 2) + 1);
					double y = gradient * x;
					
					squareDistance = Math.Pow(x - dCoord.X, 2) + Math.Pow(y - dCoord.Y, 2);

					if (squareDistance <= squaredSearchRadius)
						return transition;
				}
				else
				{
					dCoord = new(coordinate.X - transition.CenterArc.X, coordinate.Y - transition.CenterArc.Y);

					/// Calculate the distance from CentreArc is within the limits
					squareDistance = Math.Sqrt(Math.Pow(dCoord.X, 2) + Math.Pow(dCoord.Y, 2)) - transition.Radius;
					if (Math.Abs(squareDistance) > _transitionSearchRadius)
						continue;
					
					/// Calculate the angle from centreArc to dCoord to only click the segment of the circle
					/// between FromAngle and ToAngle.

					double angle = Math.Atan2(dCoord.Y, dCoord.X);

					double startAngle = transition.FromAngle;
					double endAngle = transition.ToAngle;

					if (transition.IsReversed)
						(startAngle, endAngle) = (endAngle, startAngle);

					if (endAngle < startAngle)
						endAngle += 2 * Math.PI;
					if (angle < startAngle)
						angle += 2 * Math.PI;
					else if (angle > endAngle)
						angle -= 2 * Math.PI;

					if (angle > startAngle && angle < endAngle)
						return transition;
				}

			}
			return null;
		}

		public List<StateTransition> FindTransitions(FiniteState state, Predicate<StateTransition>? filter = null)
		{
			if (filter is null)
				filter = x => true;

			return _transitions
				.Where(x => (x.FromState == state || x.ToState == state) && filter(x))
				.ToList();
		}
	}
}
