namespace Finite_State_Machine_Designer.Client.FSM
{
	public class CanvasCoordinate
	{
		public int X
		{
			get => _x;
			set => _x = value;
		}
		private int _x;

		public int Y
		{
			get => _y;
			set => _y = value;
		}
		private int _y;

		public CanvasCoordinate(int x, int y)
		{
			_x = x;
			_y = y;
		}

		public override bool Equals(object? obj)
		{
			if (obj is CanvasCoordinate coordinate)
			{
				return X == coordinate.X && Y == coordinate.Y;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_x, _y);
		}

		public override string ToString()
		{
			return $"(x: {_x}, y: {_y})";
		}
	}
}
