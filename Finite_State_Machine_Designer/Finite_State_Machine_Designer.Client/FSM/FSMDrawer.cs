using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
    public class FSMDrawer : IFSMDrawer
    {
        private readonly ILogger _logger;
        private IJSObjectReference? _jsModule;
        private readonly IFiniteStateMachine _fsm;
		private string _nonSelectedColour = "#ff0000";
		private string _selectedColour = "#0000ff";

		public FSMDrawer(ILogger<IFSMDrawer> logger, IFiniteStateMachine fsm)
        {
            _logger = logger;
			_fsm = fsm;
		}

		public IFiniteStateMachine FSM { get => _fsm; }

		public void SetStateColours(string colour = "#ff0000", string selectedColour = "#0000ff")
		{
			_nonSelectedColour = colour;
			_selectedColour = selectedColour;

			_logger.LogInformation("Current state colours are:\nColour={Colour}\nSelected={Selected}", _nonSelectedColour, _selectedColour);
		}

		public void SetJsModule(IJSObjectReference jsObjectRef)
		{
			_jsModule = jsObjectRef;
		}

		public async Task<CanvasCoordinate?> CreateState(int x, int y, float radius, string colour)
		{
			if (_jsModule != null)
			{
				bool isCreated = await _jsModule.InvokeAsync<bool>(
					"drawState", [x, y, radius, colour, Array.Empty<string>(), true, false]
				);

				if (!isCreated)
					return null;

				CanvasCoordinate coordinate = new (x, y);
				_fsm.AddState(coordinate, radius);
				_logger.LogInformation("Created state at canvas position: {Coordinate}", coordinate);
				return coordinate;
			}
			return null;
		}

		public void MoveState(MouseEventArgs mouseEventArgs)
		{
			if (_fsm.SelectedState != null
			&& mouseEventArgs.Buttons > 0
			&& mouseEventArgs.Buttons <= 3)
			{
				var coords = new CanvasCoordinate((int)mouseEventArgs.OffsetX, (int)mouseEventArgs.OffsetY);
				_fsm.SelectedState.Coordinate = coords;
			}
		}

		public async Task<bool> DrawMachineAsync(bool lineVisible = false)
		{
			if (_jsModule != null && await _jsModule.InvokeAsync<bool>("clearCanvas"))
			{
				bool editable;
				string currentColour;

				foreach (FiniteState state in _fsm.States)
				{
					editable = false;
					currentColour = _nonSelectedColour;

					if (state == _fsm.SelectedState)
					{
						currentColour = _selectedColour;
						editable = true;
					}

					string[] texts;

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
				return true;
			}
			return false;
		}
	}
}
