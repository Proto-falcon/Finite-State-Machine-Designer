namespace Finite_State_Machine_Designer.Client.FSM
{
	public class CanvasCoordinate
	{
		public static int MaxCanvasX
		{
			set
			{
				if (value >= 0)
					_maxCanvasX = value;
				else
					_maxCanvasX = 100;
			}
		}
		private static int _maxCanvasX;

		public static int MaxCanvasY
		{
			set
			{
				if (value >= 0)
					_maxCanvasY = value;
				else
					_maxCanvasY = 100;
			}
		}
		private static int _maxCanvasY;

		public int X
		{
			get => _x;
			set
			{
				if (value >= 0 && value <= _maxCanvasX)
					_x = value;
				else if (value > _maxCanvasX)
					_x = _maxCanvasX;
				else
					_x = 0;
			}
		}
		private int _x;

		public int Y
		{
			get => _y;
			set
			{
				if (value >= 0 && value <= _maxCanvasY)
					_y = value;
				else if (value > _maxCanvasY)
					_y = _maxCanvasY;
				else
					_y = 0;
			}
		}
		private int _y;

		public CanvasCoordinate(int x, int y)
		{
			_x = x;
			_y = y;
		}

		public override bool Equals(object? obj)
		{
			if (obj is not CanvasCoordinate coordinate)
			{
				return false;
			}
			return (_x == coordinate._x) && (_y == coordinate._y);
		}

		public override int GetHashCode()
		{
			return (_x >> 1) ^ (_y << 1);
		}
	}
}
