using Microsoft.JSInterop;
using System.Drawing;

namespace Finite_State_Machine_Designer.Client.FSM
{
    public class FSMDrawer(ILogger<IFSMDrawer> logger, IFiniteStateMachine fsm) : IFSMDrawer
    {
        private readonly ILogger _logger = logger;
        private IJSObjectReference? _jsModule;
		private string _nonSelectedColour = "#ff0000";
		private string _selectedColour = "#0000ff";

		public FiniteState? SelectedState
		{
			get => _selectedState;
			set => _selectedState = value;
		}
		private FiniteState? _selectedState;

		public StateTransition? SelectedTransition
		{
			get => _selectedTransition;
			set => _selectedTransition = value;
		}
		private StateTransition? _selectedTransition;

		public IFiniteStateMachine FSM { get => fsm; }

		public void SetStateColours(string colour = "#ff0000", string selectedColour = "#0000ff")
		{
			_nonSelectedColour = colour;
			_selectedColour = selectedColour;

			_logger.LogInformation("Current state colours are:\nColour={Colour}\nSelected={Selected}",
				_nonSelectedColour,_selectedColour);
		}

		public void SetJsModule(IJSObjectReference jsObjectRef)
		{
			_jsModule = jsObjectRef;
		}

		public async Task<CanvasCoordinate?> CreateStateAsync(CanvasCoordinate coordinate, float radius)
		{
			if (_jsModule is not null)
			{
				var newState = new FiniteState(coordinate, radius);
				bool isCreated = await _jsModule.InvokeAsync<bool>("drawState", newState, _selectedColour, true);


				if (!isCreated)
				{
					_logger.LogInformation("Couldn't create state at canvas position: {Coordinate}", coordinate);
					return null;
				}

				_selectedState = newState;
				fsm.AddState(newState);
				_logger.LogInformation("Created state at canvas position: {Coordinate}", coordinate);
				_selectedTransition = null;
				return coordinate;
			}
			return null;
		}

		public void MoveState(FiniteState state, CanvasCoordinate newCoord, CanvasCoordinate lastCoord)
		{
			state.Coordinate += newCoord - lastCoord;
		}

		public async Task<StateTransition?> CreateTransitionAsync(
			CanvasCoordinate fromPos = default, CanvasCoordinate toPos = default,
			FiniteState? fromState = null, FiniteState? toState = null
			)
		{
			if (_jsModule is not null)
			{
				fromState ??= new(fromPos, 0) { IsDrawable = false };
				toState ??= new(toPos, 0) { IsDrawable = false };

				StateTransition newTransition = new(fromState, toState)
				{
					MinPerpendicularDistance = 0.05
				};

				if (newTransition.FromState == newTransition.ToState)
				{
					CanvasCoordinate stateCoord = newTransition.FromState.Coordinate;
					double stateRadius = newTransition.FromState.Radius;
					CanvasCoordinate dCoord = fromPos - stateCoord;
					double angle = Math.Atan2(dCoord.Y, dCoord.X);

					double circleX = stateCoord.X + (1.5 * stateRadius * Math.Cos(angle));
					double circleY = stateCoord.Y + (1.5 * stateRadius * Math.Sin(angle));

					newTransition.CenterArc = new CanvasCoordinate(circleX, circleY);

					newTransition.Radius = 0.75 * stateRadius;
					newTransition.SetSelfAngles(angle);
				}

				CanvasCoordinate fromCoord = newTransition.FromCoord;
				CanvasCoordinate toCoord = newTransition.ToCoord;

				bool isCreated = await _jsModule
					.InvokeAsync<bool>("drawTransition", newTransition, _selectedColour, true);

				if (!isCreated)
				{
					_logger.LogInformation("Couldn't create transition between points: {From} -> {To}",
						fromCoord, toCoord);
					return null;
				}
				fsm.AddTransition(newTransition);
				_selectedTransition = newTransition;
				_logger.LogInformation("Created a transition: {Transition}", newTransition);
				_selectedState = null;
				return newTransition;
			}

			return null;
		}

		/// <summary>
		/// Finds the determinant of 3x3 matrix
		/// </summary>
		/// <param name="a">1 row, column 1 cell</param>
		/// <param name="b">1 row, column 2 cell</param>
		/// <param name="c">1 row, column 3 cell</param>
		/// <param name="d">2 row, column 1 cell</param>
		/// <param name="e">2 row, column 2 cell</param>
		/// <param name="f">2 row, column 3 cell</param>
		/// <param name="g">3 row, column 1 cell</param>
		/// <param name="h">3 row, column 2 cell</param>
		/// <param name="i">3 row, column 3 cell</param>
		/// <returns>Determinant of 3x3 matrix</returns>
		private static double Determinant(
			double a, double b, double c,
			double d, double e, double f,
			double g, double h, double i) => a*e*i + b*f*g + c*d*h - a*f*h - b*d*i - c*e*g;

		/// <summary>
		/// Curves the transitions.
		/// </summary>
		/// <param name="coord">mouse Coordinate</param>
		/// <param name="transition">The transition to be curved</param>
		/// to find the coordinate of transition touch the state for efficiency.
		/// <see langword="true"/> to calculate the actual angle.</param>
		public void CurveTransition(CanvasCoordinate coord, StateTransition transition)
		{
			transition.Anchor = coord;

			if (!transition.IsCurved)
				return;

			CanvasCoordinate fromCoord = transition.FromState.Coordinate;
			CanvasCoordinate toCoord = transition.ToState.Coordinate;

			var (circleX, circleY, circleRadius) = CircleCentreRadiiFrom3Points(fromCoord, toCoord, transition.Anchor);

			transition.CenterArc = new CanvasCoordinate(circleX, circleY);
			transition.Radius = circleRadius;
		}

		/// <summary>
		/// Updates all the transitions curvature connected to the given state.
		/// </summary>
		/// <param name="state">A finite state</param>
		public void UpdateCurvedTransitions(FiniteState state)
		{
			List<StateTransition> transitions = fsm.FindTransitions(state, x => x.IsCurved);

			foreach (var transition in transitions)
			{
				if (transition.FromState != transition.ToState)
				{
					var (circleX, circleY, circleRadius) = CircleCentreRadiiFrom3Points(
						transition.FromState.Coordinate,
						transition.ToState.Coordinate,
						transition.Anchor);

					transition.CenterArc = new CanvasCoordinate(circleX, circleY);
					transition.Radius = circleRadius;
				}
				else
					UpdateSelfTransition(transition);
			}
		}

		/// <summary>
		/// Updates the centre arc of the transition
		/// </summary>
		/// <param name="transition">State transition</param>
		public static void UpdateSelfTransition(StateTransition transition)
		{
			if (transition.FromState != transition.ToState)
				return;

			CanvasCoordinate stateCoord = transition.FromState.Coordinate;
			double stateRadius = transition.FromState.Radius;

			double circleX = stateCoord.X + (1.5 * stateRadius * Math.Cos(transition.Angle));
			double circleY = stateCoord.Y + (1.5 * stateRadius * Math.Sin(transition.Angle));

			transition.CenterArc = new CanvasCoordinate(circleX, circleY);

			transition.Radius = 0.75 * stateRadius;
		}

		/// <summary>
		/// Updates all the transitions that link back to one state using the given coordinate.
		/// </summary>
		/// <param name="transition">A state transition that links it self</param>
		/// <param name="coord">Coordinate that changes the orientation of the self transitions around the state</param>
		public void UpdateSelfTransition(StateTransition transition, CanvasCoordinate coord)
		{
			if (transition.FromState != transition.ToState)
				return;

			CanvasCoordinate stateCoord = transition.FromState.Coordinate;
			double stateRadius = transition.FromState.Radius;
			CanvasCoordinate dCoord = coord - stateCoord;
			double angle = Math.Atan2(dCoord.Y, dCoord.X);
			transition.SetSelfAngles(angle);

			double circleX = stateCoord.X + (1.5 * stateRadius * Math.Cos(transition.Angle));
			double circleY = stateCoord.Y + (1.5 * stateRadius * Math.Sin(transition.Angle));

			transition.CenterArc = new CanvasCoordinate(circleX, circleY);
			transition.Radius = 0.75 * stateRadius;
		}

		/// <summary>
		/// Generates a circles from 3 points using
		/// <a href="https://en.wikipedia.org/wiki/Laplace_expansion">Laplace Expansion</a>
		/// to find the curve of the transition.
		/// </summary>
		/// <param name="coord1">Coordinate 1</param>
		/// <param name="coord2">Coordinate 2</param>
		/// <param name="coord3">Coordinate 3</param>
		/// <returns>First and second numbers are x and y values respectively
		/// and the last number is the radius of the circle.
		/// </returns>
		private static Tuple<double, double, double> CircleCentreRadiiFrom3Points(
			CanvasCoordinate coord1, CanvasCoordinate coord2, CanvasCoordinate coord3)
		{
			double a = Determinant(
				coord1.X, coord1.Y, 1,
				coord2.X, coord2.Y, 1,
				coord3.X, coord3.Y, 1);

			double mouseLengthSquare = (coord3.X * coord3.X) + (coord3.Y * coord3.Y);
			double fromLengthSquare = (coord1.X * coord1.X) + (coord1.Y * coord1.Y);
			double toLengthSquare = (coord2.X * coord2.X) + (coord2.Y * coord2.Y);
			double bx = Determinant(
				fromLengthSquare, coord1.Y, 1,
				toLengthSquare, coord2.Y, 1,
				mouseLengthSquare, coord3.Y, 1);

			double by = Determinant(
				fromLengthSquare, coord1.X, 1,
				toLengthSquare, coord2.X, 1,
				mouseLengthSquare, coord3.X, 1);

			double c = Determinant(
				fromLengthSquare, coord1.X, coord1.Y,
				toLengthSquare, coord2.X, coord2.Y,
				mouseLengthSquare, coord3.X, coord3.Y);

			double circleX = bx / (2 * a);
			double circleY = -(by / (2 * a));
			double circleRadius = Math.Sqrt((circleX * circleX) + (circleY * circleY) + (c / a));

			return new Tuple<double, double, double>(circleX, circleY, circleRadius);
		}

		public async Task<bool> DrawMachineAsync(bool lineVisible = false)
		{
			if (_jsModule is not null && await _jsModule.InvokeAsync<bool>("clearCanvas"))
			{
				bool editable;
				string currentColour;

				foreach (FiniteState state in fsm.States)
				{
					if (!state.IsDrawable)
						continue;
					editable = false;
					currentColour = _nonSelectedColour;

					if (state == _selectedState)
					{
						currentColour = _selectedColour;
						editable = true;
					}

					await _jsModule.InvokeAsync<bool>(
						"drawState",
						state,
						currentColour,
						editable && lineVisible
					);
				}
				foreach (StateTransition transition in fsm.Transitions)
                {
					editable = false;
					currentColour = _nonSelectedColour;

					if (transition == _selectedTransition)
					{
						editable = true;
						currentColour = _selectedColour;
					}
					
					await _jsModule.InvokeAsync<bool>(
						"drawTransition",
						transition,
						currentColour,
						editable && lineVisible && transition.ToState.IsDrawable
					);
				}
                return true;
			}
			return false;
		}
	}
}
