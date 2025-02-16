using Microsoft.JSInterop;
using Finite_State_Machine_Designer.Models.FSM;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FSMDrawer(ILogger<IFSMDrawer> logger, IFiniteStateMachine fsm) : IFSMDrawer
	{
		private readonly ILogger _logger = logger;
		private IJSObjectReference? _jsModule;
		private string _nonSelectedColour = "#ff0000";
		private string _selectedColour = "#0000ff";
		private readonly int _snapPadding = 6;
		private readonly double _minPerpendicularDistance = 0.05;
		private string _backgroundColour = "#000000";

		public CanvasCoordinate LastMouseDownCoord
		{
			get => _lastMouseDownCoord;
			set
			{
				_lastMouseDownCoord = value;
				if (_selectedState is not null)
					_selectedStateCoordOffset = _selectedState.Coordinate - _lastMouseDownCoord;
				else if (_selectedTransition is not null && _selectedTransition.FromState == _selectedTransition.ToState)
				{
					CanvasCoordinate dCoord = _lastMouseDownCoord - _selectedTransition.FromState.Coordinate;
					_selfTransitionAngleOffset = _selectedTransition.SelfAngle - Math.Atan2(dCoord.Y, dCoord.X);
				}
			}
		}
		private CanvasCoordinate _lastMouseDownCoord;
		private CanvasCoordinate _selectedStateCoordOffset;
		private double _selfTransitionAngleOffset;

		public FiniteState? SelectedState
		{
			get => _selectedState;
			set => _selectedState = value;
		}
		private FiniteState? _selectedState;

		public Transition? SelectedTransition
		{
			get => _selectedTransition;
			set => _selectedTransition = value;
		}
		private Transition? _selectedTransition;

		public IFiniteStateMachine FSM
		{
			get => fsm;
			set
			{
				fsm = value;
			}
		}

		public void SetColours(string colour = "#ff0000", string selectedColour = "#0000ff", string backgroundColour = "#000000")
		{
			_nonSelectedColour = colour;
			_selectedColour = selectedColour;
			_backgroundColour = backgroundColour;
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
				bool isCreated = await _jsModule.InvokeAsync<bool>("fSMCanvasUtils.drawState", newState, _selectedColour, true);


				if (!isCreated)
				{
					_logger.LogInformation("Couldn't create state at canvas position: {Coordinate}", coordinate);
					return null;
				}

				_selectedState = newState;
				fsm.States.Add(newState);
				_logger.LogInformation("Created state at canvas position: {Coordinate}", coordinate);
				_selectedTransition = null;
				return coordinate;
			}
			return null;
		}

		public void MoveState(FiniteState state, CanvasCoordinate newCoord,
			CanvasCoordinate lastCoord, bool snapState = false)
		{
			CanvasCoordinate lastStateCoord = state.Coordinate;
			state.Coordinate = newCoord + _selectedStateCoordOffset;
			
			if (snapState)
				SnapState(state);

			List<Transition> transitions = fsm
				.FindTransitions(_selectedState, x => !x.FromState.IsDrawable);
			CanvasCoordinate dCoord = state.Coordinate - lastStateCoord;
			foreach (var transition in transitions)
			{
				// Moves the non-drawable states from incoming transitions
				transition.FromState.Coordinate += dCoord;
				SnapState(transition.FromState, transition.ToState);
			}
		}

		public async Task<Transition?> CreateTransitionAsync(
			CanvasCoordinate fromPos = default, CanvasCoordinate toPos = default,
			FiniteState? fromState = null, FiniteState? toState = null
			)
		{
			if (_jsModule is not null)
			{
				if (fromState is null)
				{
					fromState = new(fromPos, 0) { IsDrawable = false };
					fsm.States.Add(fromState);
				}
				if (toState is null)
				{
					toState = new(toPos, 0) { IsDrawable = false };
					fsm.States.Add(toState);
				}

				Transition newTransition = new(fromState, toState)
				{
					MinPerpendicularDistance = _minPerpendicularDistance
				};

				if (newTransition.FromState == newTransition.ToState)
				{
					CanvasCoordinate stateCoord = newTransition.FromState.Coordinate;
					CanvasCoordinate dCoord = fromPos - stateCoord;
					double angle = Math.Atan2(dCoord.Y, dCoord.X);
					newTransition.SelfAngle = angle;
				}

				CanvasCoordinate fromCoord = newTransition.FromCoord;
				CanvasCoordinate toCoord = newTransition.ToCoord;

				bool isCreated = await _jsModule
					.InvokeAsync<bool>("fSMCanvasUtils.drawTransition", newTransition, _selectedColour, true);

				if (!isCreated)
				{
					_logger.LogInformation("Couldn't create transition between points: {From} -> {To}",
						fromCoord, toCoord);
					return null;
				}
				fsm.Transitions.Add(newTransition);
				_selectedTransition = newTransition;
				_logger.LogInformation("Created a transition: {Transition}", newTransition);
				_selectedState = null;
				return newTransition;
			}

			return null;
		}

		/// <summary>
		/// Curves the transitions.
		/// </summary>
		/// <param name="coord">mouse Coordinate</param>
		/// <param name="transition">The transition to be curved</param>
		/// to find the coordinate of transition touch the state for efficiency.
		/// <see langword="true"/> to calculate the actual angle.</param>
		public void CurveTransition(CanvasCoordinate coord, Transition transition)
		{
			CanvasCoordinate fromCoord = transition.FromState.Coordinate;
			CanvasCoordinate toCoord = transition.ToState.Coordinate;

			CanvasCoordinate dCoord = new(toCoord.X - fromCoord.X,
					toCoord.Y - fromCoord.Y);
			double squareLength = (dCoord.X * dCoord.X) + (dCoord.Y * dCoord.Y);

			CanvasCoordinate dCoord2 = new(coord.X - fromCoord.X,
				coord.Y - fromCoord.Y);

			/// Using Dot Product
			transition.ParallelAxis = ((dCoord.X * dCoord2.X) + (dCoord.Y * dCoord2.Y)) / squareLength;
			/// Using Determinant
			transition.PerpendicularAxis = ((dCoord.X * dCoord2.Y) - (dCoord.Y * dCoord2.X)) / squareLength;

			transition.IsReversed = transition.PerpendicularAxis > 0;
			if (Math.Abs(transition.PerpendicularAxis) < _minPerpendicularDistance)
			{
				transition.Radius = 0;
				transition.PerpendicularAxis = 0;
			}

			if (!transition.IsCurved)
				return;

			var (circleX, circleY, circleRadius) = Helper.Matrix.CircleCentreRadiiFrom3Points(fromCoord, toCoord, transition.Anchor);

			transition.CenterArc = new CanvasCoordinate(circleX, circleY);
			transition.Radius = circleRadius;
		}

		/// <summary>
		/// Updates all the transitions curvature connected to the given state.
		/// </summary>
		/// <param name="state">A finite state</param>
		public void UpdateCurvedTransitions(FiniteState state)
		{
			List<Transition> transitions = fsm.FindTransitions(state, x => x.IsCurved);

			foreach (var transition in transitions)
			{
				if (transition.FromState != transition.ToState)
				{
					var (circleX, circleY, circleRadius) = Helper.Matrix.CircleCentreRadiiFrom3Points(
						transition.FromState.Coordinate,
						transition.ToState.Coordinate,
						transition.Anchor);

					transition.CenterArc = new CanvasCoordinate(circleX, circleY);
					transition.Radius = circleRadius;
				}
				else if (transition.FromState == transition.ToState)
					transition.Radius = 0.75 * transition.FromState.Radius;
			}
		}

		/// <summary>
		/// Updates the self link transition's angles the given coordinate.
		/// </summary>
		/// <param name="transition">A state transition that links it self</param>
		/// <param name="coord">
		/// Coordinate that changes the orientation of the self transitions around the state
		/// </param>
		public void UpdateSelfTransition(Transition transition, CanvasCoordinate coord)
		{
			if (transition.FromState != transition.ToState)
				return;

			CanvasCoordinate stateCoord = transition.FromState.Coordinate;
			CanvasCoordinate dCoord = coord - stateCoord;
			double angle = Math.Atan2(dCoord.Y, dCoord.X) + _selfTransitionAngleOffset;

			/// Round the divided number to get an integer multiple of π/2
			/// to be mulitplied to get the angle at right angles.
			var snap = Math.Round(angle / (Math.PI/2)) * (Math.PI/2);
			if (Math.Abs(angle - snap) < 0.1)
				angle = snap;

			transition.SelfAngle = angle;
		}

		/// <summary>
		/// Snaps a state to any other states x or y coordinates.
		/// </summary>
		/// <param name="state">A finite state.</param>
		/// <returns>
		/// <see langword="true"/> when state snapped to other states,
		/// otherwise <see langword="false"/>.
		/// </returns>
		public bool SnapState(FiniteState state)
		{
			bool isSnapped = false;
			bool xSnapped = false;
			bool ySnapped = false;

			double x = state.Coordinate.X;
			double y = state.Coordinate.Y;

			foreach (var otherState in fsm.States)
			{
				if (state == otherState)
					continue;

				if (Math.Abs(state.Coordinate.X - otherState.Coordinate.X) < _snapPadding)
				{
					x = otherState.Coordinate.X;
					xSnapped = true;
					isSnapped = true;
				}

				if (Math.Abs(state.Coordinate.Y - otherState.Coordinate.Y) < _snapPadding)
				{
					y = otherState.Coordinate.Y;
					ySnapped = true;
					isSnapped = true;
				}

				if (xSnapped && ySnapped)
					break;
			}

			state.Coordinate = new CanvasCoordinate(x, y);
			return isSnapped;
		}

		/// <summary>
		/// Snaps the first state to the others states x or y coordinates.
		/// </summary>
		/// <param name="state">First finite state</param>
		/// <param name="otherState">Second finite state</param>
		/// <returns>
		/// <see langword="true"/> when state snapped to other state,
		/// otherwise <see langword="false"/>.
		/// </returns>
		public bool SnapState(FiniteState state, FiniteState otherState)
		{
			bool isSnapped = false;
			if (state == otherState)
				return true;

			double x = state.Coordinate.X;
			double y = state.Coordinate.Y;

			if (Math.Abs(state.Coordinate.X - otherState.Coordinate.X) < _snapPadding)
			{
				x = otherState.Coordinate.X;
				isSnapped = true;
			}

			if (Math.Abs(state.Coordinate.Y - otherState.Coordinate.Y) < _snapPadding)
			{
				y = otherState.Coordinate.Y;
				isSnapped = true;
			}

			state.Coordinate = new CanvasCoordinate(x, y);
			return isSnapped;
		}

		public async Task<bool> DrawMachineAsync(bool lineVisible = false)
		{
			if (_jsModule is not null && await _jsModule.InvokeAsync<bool>("fSMCanvasUtils.clearCanvas"))
			{
				await _jsModule.InvokeVoidAsync("fSMCanvasUtils.drawBackgroundColour", _backgroundColour);
				bool editable;
				string currentColour;

				Parallel.ForEach(fsm.States, async state =>
				{
                    if (!state.IsDrawable)
                        return;
                    editable = false;
                    currentColour = _nonSelectedColour;

                    if (state == _selectedState)
                    {
                        currentColour = _selectedColour;
                        editable = true;
                    }

                    await _jsModule.InvokeAsync<bool>(
                        "fSMCanvasUtils.drawState",
                        state,
                        currentColour,
                        editable && lineVisible
                    );
                });

				Parallel.ForEach(fsm.Transitions, async transition =>
				{
                    editable = false;
                    currentColour = _nonSelectedColour;

                    if (transition == _selectedTransition)
                    {
                        editable = true;
                        currentColour = _selectedColour;
                    }

                    await _jsModule.InvokeAsync<bool>(
                        "fSMCanvasUtils.drawTransition",
                        transition,
                        currentColour,
                        editable && lineVisible && transition.ToState.IsDrawable
                    );
                });
				return true;
			}
			return false;
		}
	}
}
