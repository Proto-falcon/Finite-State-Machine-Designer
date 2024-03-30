namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteState(CanvasCoordinate coordinate, float radius)
	{
		public CanvasCoordinate Coordinate
		{
			get => coordinate;
			set => coordinate = value;
		}

		public float Radius
		{
			get => radius;
			set
			{
				if (value >= 10)
					radius = value;
				else
					radius = 10f;
			}
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

		public void SetCoordinate(int newX, int newY)
		{
			coordinate.X = newX;
			coordinate.Y = newY;
		}

		public override string ToString()
		{
			return $"(co-ordinate: {coordinate}, Radius: {radius}, Text: '{_text}', FinalState: {_isFinalState})";
		}
	}
}
