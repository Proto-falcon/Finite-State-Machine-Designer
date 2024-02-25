namespace Finite_State_Machine_Designer.Client.FSM
{
	public class FiniteState
	{
		public CanvasCoordinate Coordinate
		{
			get => _coordinate;
			set => _coordinate = value;
		}
		private CanvasCoordinate _coordinate;

		public float Radius
		{
			get => _radius;
			set
			{
				if (value >= 10)
					_radius = value;
				else
					_radius = 10f;
			}
		}
		private float _radius;
		public bool IsFinalState { get; set; } = false;
		public string Text { get; set; } = string.Empty;

		public FiniteState(CanvasCoordinate coordinate, float radius)
		{
			_coordinate = coordinate;
			_radius = radius;
		}

		public void SetCoordinate(int newX, int newY)
		{
			_coordinate.X = newX;
			_coordinate.Y = newY;
		}

		public override string ToString()
		{
			return $"(co-ordinate: {_coordinate}, Radius: {_radius})";
		}
	}
}
