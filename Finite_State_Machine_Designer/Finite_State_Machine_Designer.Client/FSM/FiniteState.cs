namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteState(CanvasCoordinate coordinate, float radius)
	{
		public CanvasCoordinate Coordinate
		{
			get => coordinate;
			set => coordinate = value;
		}

		public bool IsDrawable { get; set; } = true;

		public float Radius
		{
			get => radius;
			set => radius = value;
		}

		private bool _isFinalState = false;

		public bool IsFinalState
		{
			get => _isFinalState;
			set => _isFinalState = value;
		}

		private string _text = string.Empty;

		public string Text
		{
			get => _text;
			set => _text = value;
		}

		public override int GetHashCode() =>
			HashCode.Combine(coordinate, _isFinalState, _text);

		public override string ToString() => 
			$"(co-ordinate: {coordinate}, Radius: {radius}, Text: '{_text}', FinalState: {_isFinalState})";
	}
}
