using Microsoft.JSInterop;

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

				StateTransition newTransition = new (fromState, toState);
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
		/// Uses <a href="https://en.wikipedia.org/wiki/Laplace_expansion">Laplace Expansion</a>
		/// and <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">one of the formulae of segment</a>
		/// to find the curve of the transition.
		/// </summary>
		/// <param name="coord">mouse Coordinate</param>
		/// <param name="transition">The transition to be curved</param>
		/// <param name="useAngle"><see langword="false"/> to use the ratio between state radius and transiton radius
		/// to find the coordinate of transition touch the state for efficiency.
		/// <see langword="true"/> to calculate the actual angle.</param>
		public void CurveTransition(CanvasCoordinate coord, StateTransition transition, bool useAngle = false)
		{
			transition.Anchor = coord;

			CanvasCoordinate fromCoord = transition.FromState.Coordinate;
			CanvasCoordinate toCoord = transition.ToState.Coordinate;

			double a = Determinant(
				fromCoord.X, fromCoord.Y, 1,
				toCoord.X,   toCoord.Y,   1,
				coord.X,     coord.Y,     1);

			double mouseLengthSquare = (coord.X*coord.X) + (coord.Y*coord.Y);
			double fromLengthSquare = (fromCoord.X*fromCoord.X) + (fromCoord.Y*fromCoord.Y);
			double toLengthSquare = (toCoord.X*toCoord.X) + (toCoord.Y*toCoord.Y);

			double bx = Determinant(
				fromLengthSquare,  fromCoord.Y, 1,
				toLengthSquare,    toCoord.Y,   1,
				mouseLengthSquare, coord.Y,     1);

			double by = Determinant(
				fromLengthSquare,  fromCoord.X, 1,
				toLengthSquare,    toCoord.X,   1,
				mouseLengthSquare, coord.X,     1);

			double c = Determinant(
				fromLengthSquare,  fromCoord.X, fromCoord.Y,
				toLengthSquare,    toCoord.X,   toCoord.Y,
				mouseLengthSquare, coord.X,     coord.Y);

			double circleX = bx / (2*a);
			double circleY = -(by / (2*a));
			double circleRadius = Math.Sqrt((circleX*circleX) + (circleY*circleY) + (c/a));
			
			transition.IsCurved = true;
			transition.CenterArc = new CanvasCoordinate(circleX, circleY);
			transition.Radius = circleRadius;
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
						editable && lineVisible
					);
				}
                return true;
			}
			return false;
		}
	}
}
