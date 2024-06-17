using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFSMDrawer
	{
		public IFiniteStateMachine FSM { get; set; }

		public FiniteState? SelectedState { get; set; }

		public Transition? SelectedTransition { get; set; }

		public CanvasCoordinate LastMouseDownCoord { get; set; }

		public void SetColours(string colour = "#ffffff", string selectedColour = "#0000ff", string backgroundColour = "#000000");

		public void SetJsModule(IJSObjectReference jsObjectRef);

		public Task<CanvasCoordinate?> CreateStateAsync(CanvasCoordinate coordinate, float radius);

		public void MoveState(FiniteState state, CanvasCoordinate newCoord, CanvasCoordinate lastCoord, bool snapState = false);

		public Task<Transition?> CreateTransitionAsync(
			CanvasCoordinate fromPos = default, CanvasCoordinate toPos = default,
			FiniteState? fromState = null, FiniteState? toState = null
			);

		public void CurveTransition(CanvasCoordinate coord, Transition transition);

		public void UpdateCurvedTransitions(FiniteState state);

		public void UpdateSelfTransition(Transition transition, CanvasCoordinate coord);

		public bool SnapState(FiniteState state);

		public bool SnapState(FiniteState state, FiniteState otherState);

		public Task<bool> DrawMachineAsync(bool lineVisible = false);
	}
}
