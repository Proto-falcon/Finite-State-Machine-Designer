using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
    public class FSMDrawer(ILogger<IFSMDrawer> logger, IFiniteStateMachine fsm) : IFSMDrawer
    {
        private readonly ILogger _logger = logger;
        private IJSObjectReference? _jsModule;
		private string _nonSelectedColour = "#ff0000";
		private string _selectedColour = "#0000ff";
		private float _minStateRadius;

		public float MinStateRadius
		{
			get => _minStateRadius;
			set => _minStateRadius = value;
		}

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

		public async Task<CanvasCoordinate?> CreateStateAsync(int x, int y, float radius)
		{
			if (_jsModule is not null)
			{
				bool isCreated = await _jsModule.InvokeAsync<bool>(
					"drawState", x, y, radius, _selectedColour, Array.Empty<string>(), true, false
				);

				CanvasCoordinate coordinate = new (x, y);

				if (!isCreated)
				{
					_logger.LogInformation("Couldn't create state at canvas position: {Coordinate}", coordinate);
					return null;
				}

				var newState = new FiniteState(coordinate, radius);
				_selectedState = newState;
				fsm.AddState(newState);
				_logger.LogInformation("Created state at canvas position: {Coordinate}", coordinate);
				_selectedTransition = null;
				return coordinate;
			}
			return null;
		}

		public void MoveState(MouseEventArgs mouseEventArgs, int lastX, int lastY)
		{
			if (_selectedState is not null
			&& mouseEventArgs.Buttons > 0
			&& mouseEventArgs.Buttons <= 3)
			{
				int xDiff = (int)mouseEventArgs.OffsetX - lastX;
				int yDiff = (int)mouseEventArgs.OffsetY - lastY;
				var coord = _selectedState.Coordinate;
				coord.X += xDiff;
				coord.Y += yDiff;
				_selectedState.Coordinate = coord;
			}
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

				int dx = toCoord.X - fromCoord.X;
				int dy = toCoord.Y - fromCoord.Y;

				bool isCreated = await _jsModule.InvokeAsync<bool>("drawTransition",
					fromCoord.X, fromCoord.Y, toCoord.X, toCoord.Y,
					Math.Atan2(dy, dx), _selectedColour, Array.Empty<string>(),
					true, false
					);

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

		public void MoveTransition(CanvasCoordinate coord)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> DrawMachineAsync(bool lineVisible = false)
		{
			if (_jsModule is not null && await _jsModule.InvokeAsync<bool>("clearCanvas"))
			{
				bool editable;
				string currentColour;
				string[] texts;

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

					if (string.IsNullOrEmpty(state.Text))
						texts = [];
					else
						texts = state.Text.Split('\n');

					await _jsModule.InvokeAsync<bool>(
						"drawState",
						state.Coordinate.X,
						state.Coordinate.Y,
						state.Radius,
						currentColour,
						texts,
						editable && lineVisible,
						state.IsFinalState
					);
				}
				int dx;
				int dy;
				foreach (StateTransition transition in fsm.Transitions)
                {
					editable = false;
					currentColour = _nonSelectedColour;

					if (transition == _selectedTransition)
					{
						editable = true;
						currentColour = _selectedColour;
					}

					if (string.IsNullOrEmpty(transition.Text))
						texts = [];
					else
						texts = transition.Text.Split('\n');

					dx = transition.ToCoord.X - transition.FromCoord.X;
					dy = transition.ToCoord.Y - transition.FromCoord.Y;

					await _jsModule.InvokeAsync<bool>(
						"drawTransition",
						transition.FromCoord.X,
						transition.FromCoord.Y,
						transition.ToCoord.X,
						transition.ToCoord.Y,
						Math.Atan2(dy, dx),
						currentColour,
						texts,
						editable && lineVisible,
						transition.IsCurved
					);
				}
                return true;
			}
			return false;
		}
	}
}
