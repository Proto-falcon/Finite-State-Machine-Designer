using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFSMDrawer
	{
		public IFiniteStateMachine FSM { get; }

		public FiniteState? SelectedState { get; set; }

		public float MinStateRadius { get; set; }

		public StateTransition? SelectedTransition { get; set; }

		public void SetStateColours(string colour = "#ff0000", string selectedColour = "#0000ff");

		public void SetJsModule(IJSObjectReference jsObjectRef);

		public Task<CanvasCoordinate?> CreateStateAsync(int x, int y, float radius);

		public void MoveState(MouseEventArgs mouseEventArgs, int lastX, int lastY);

		public Task<StateTransition?> CreateTransitionAsync(
			CanvasCoordinate fromPos = default, CanvasCoordinate toPos = default,
			FiniteState? fromState = null, FiniteState? toState = null
			);

		public void MoveTransition(CanvasCoordinate coord);

		public Task<bool> DrawMachineAsync(bool lineVisible = false);
	}
}
