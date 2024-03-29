namespace Finite_State_Machine_Designer.Client.FSM
{
	public class StateTransition(CanvasCoordinate? from, CanvasCoordinate to)
	{
		public CanvasCoordinate? From
		{
			get => from;
			set => from = value;
		}

		public CanvasCoordinate To
		{
			get => to;
			set => to = value;
		}

		public bool IsCurved { get; set; } = false;
		public string Text { get; set; } = string.Empty;
	}
}
