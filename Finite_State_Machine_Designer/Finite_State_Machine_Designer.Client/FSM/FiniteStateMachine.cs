using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteStateMachine : IFiniteStateMachine
    {
		public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

		public int Width { get; set; }

		public int Height { get; set; }

		/// <summary>
		/// The list includes "Invisible states" for incoming transitions
		/// </summary>
        public List<FiniteState> States
		{
			get => _states;
			set
			{
				if (value is not null)
					_states = value;
				else
					_states = [];
			}
		}
		private List<FiniteState> _states = [];

		public List<Transition> Transitions
        {
            get => _transitions;
            set
            {
                if (value is not null)
                    _transitions = value;
                else
                    _transitions = [];
            }
        }
        private List<Transition> _transitions = [];

		private int _transitionSearchRadius;
		public int TransitionSearchRadius
		{
			get => _transitionSearchRadius;
			set => _transitionSearchRadius = value;
		}

		[NotMapped]
		public List<FiniteState> InitialStates => _transitions.Where(x => !x.FromState.IsDrawable && x.ToState.IsDrawable)
			.Select(x => x.ToState).ToList();

        [NotMapped]
        public List<FiniteState> FinalStates => _states.Where(x => x.IsFinalState).ToList();

		public FiniteState? FindState(CanvasCoordinate coordinate)
		{
			// Equation of Circle -> (x-a)^2 + (y-b)^2 <= r^2
			// where (a,b) are x and y coordinates of the centre of the circle respectively
			// r is radius of circle
			foreach (var state in _states)
			{
                if (!state.IsDrawable)
					continue;
                CanvasCoordinate coord = state.Coordinate;
				double leftSide = Math.Pow(coordinate.X - coord.X, 2) + Math.Pow(coordinate.Y - coord.Y, 2);
				double rightSide = Math.Pow(state.Radius, 2);
				if (leftSide <= rightSide)
					return state;
			}
			return null;
		}

		public Transition? FindTransition(CanvasCoordinate coordinate)
		{
			CanvasCoordinate dCoord;
			foreach (Transition transition in _transitions)
			{
				if (!transition.IsCurved)
				{
					CanvasCoordinate dCoordTransition = transition.ToCoord - transition.FromCoord;
					dCoord = coordinate - transition.FromCoord;
					/// Using dot product to find out what part of the line has been clicked
					double squareDistance = (dCoordTransition.X * dCoordTransition.X) + (dCoordTransition.Y * dCoordTransition.Y);
					double scaledLength = ((dCoord.X * dCoordTransition.X) + (dCoord.Y * dCoordTransition.Y)) / squareDistance;
					if (scaledLength < 0 || scaledLength > 1)
						continue;
					/// Using determinant to find out how far away is the mouse perpendicular to the line
					double perpendicularDistance = ((dCoord.X * dCoordTransition.Y) - (dCoord.Y * dCoordTransition.X))
						/ Math.Sqrt(squareDistance);
					if (Math.Abs(perpendicularDistance) <= _transitionSearchRadius)
						return transition;
				}
				else
				{
					dCoord = new(coordinate.X - transition.CenterArc.X, coordinate.Y - transition.CenterArc.Y);
					/// Calculate the distance from CentreArc is within the limits
					double radiiDiff = Math.Sqrt(Math.Pow(dCoord.X, 2) + Math.Pow(dCoord.Y, 2)) - transition.Radius;
					if (Math.Abs(radiiDiff) > _transitionSearchRadius)
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

		public List<Transition> FindTransitions(FiniteState? state, Predicate<Transition>? filter = null)
		{
			if (filter is not null)
				return _transitions
					.Where(x => (x.FromState == state || x.ToState == state) && filter(x))
					.ToList();
            return _transitions
                .Where(x => (x.FromState == state || x.ToState == state))
                .ToList();
        }
	}
}
