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

		public bool IsFinalState { get; set; } = false;
		public string Text { get; set; } = string.Empty;

		public void SetCoordinate(int newX, int newY)
		{
			coordinate.X = newX;
			coordinate.Y = newY;
		}

		public override string ToString()
		{
			return $"(co-ordinate: {coordinate}, Radius: {radius})";
		}
	}
}
