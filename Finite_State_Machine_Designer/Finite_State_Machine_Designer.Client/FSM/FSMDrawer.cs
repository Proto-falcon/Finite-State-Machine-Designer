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

					if (string.IsNullOrEmpty(transition.Text))
						texts = [];
					else
						texts = transition.Text.Split('\n');

					// TODO: Implement curved transition
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
