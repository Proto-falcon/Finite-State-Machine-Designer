namespace Finite_State_Machine_Designer.Client.FSM
{
	public struct CanvasCoordinate(int x, int y)
	{
		public int X
		{ readonly get => x;
			set => x = value;
		}

		public int Y
		{ readonly get => y;
			set => y = value;
		}

		public static CanvasCoordinate operator +(CanvasCoordinate a, CanvasCoordinate b) =>
			new (a.X + b.X, a.Y + b.Y);
		public static CanvasCoordinate operator -(CanvasCoordinate a, CanvasCoordinate b) =>
			new (a.X - b.X, a.Y - b.Y);

		public override readonly string ToString() => $"(x: {x}, y: {y})";
	}
}
