namespace Finite_State_Machine_Designer.Models.FSM
{
	public struct CanvasCoordinate(double x, double y)
	{
		public double X
		{ readonly get => x;
			set => x = value;
		}

		public double Y
		{ readonly get => y;
			set => y = value;
		}

		public static CanvasCoordinate operator +(CanvasCoordinate a, CanvasCoordinate b) =>
			new (a.X + b.X, a.Y + b.Y);
		public static CanvasCoordinate operator -(CanvasCoordinate a, CanvasCoordinate b) =>
			new (a.X - b.X, a.Y - b.Y);
		public static CanvasCoordinate operator *(CanvasCoordinate a, CanvasCoordinate b) =>
			new(a.X * b.X, a.Y * b.Y);


        public override readonly string ToString() => $"(x: {x}, y: {y})";
	}
}
