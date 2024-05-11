using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFSMDrawer
	{
		public IFiniteStateMachine FSM { get; set; }

		public FiniteState? SelectedState { get; set; }

		public StateTransition? SelectedTransition { get; set; }

		public CanvasCoordinate LastMouseDownCoord { get; set; }

		public void SetStateColours(string colour = "#ff0000", string selectedColour = "#0000ff");

		public void SetJsModule(IJSObjectReference jsObjectRef);

		public Task<CanvasCoordinate?> CreateStateAsync(CanvasCoordinate coordinate, float radius);

		public void MoveState(FiniteState state, CanvasCoordinate newCoord, CanvasCoordinate lastCoord, bool snapState = false);

		public Task<StateTransition?> CreateTransitionAsync(
			CanvasCoordinate fromPos = default, CanvasCoordinate toPos = default,
			FiniteState? fromState = null, FiniteState? toState = null
			);

		public void CurveTransition(CanvasCoordinate coord, StateTransition transition);

		public void UpdateCurvedTransitions(FiniteState state);

		public void UpdateSelfTransition(StateTransition transition, CanvasCoordinate coord);

		public void SnapState(FiniteState state);

		public void SnapState(FiniteState state, FiniteState otherState);

		public Task<bool> DrawMachineAsync(bool lineVisible = false);
	}
}
