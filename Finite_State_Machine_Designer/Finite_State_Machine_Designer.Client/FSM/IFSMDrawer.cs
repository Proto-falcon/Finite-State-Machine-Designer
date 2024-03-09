using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Client.FSM
{
	public interface IFSMDrawer
	{
		public IFiniteStateMachine FSM { get; }

		public FiniteState? SelectedState { get; set; }

		public void SetStateColours(string colour = "#ff0000", string selectedColour = "#0000ff");

		public void SetJsModule(IJSObjectReference jsObjectRef);

		public Task<CanvasCoordinate?> CreateState(int x, int y, float radius, string colour);

		public void MoveState(MouseEventArgs mouseEventArgs, int lastX, int lastY);

		public Task<bool> DrawMachineAsync(bool lineVisible = false);
	}
}
