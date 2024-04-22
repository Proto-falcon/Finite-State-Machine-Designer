namespace Finite_State_Machine_Designer.Client.FSM
{
	public class StateTransition(FiniteState fromState, FiniteState toState)
	{
		private FiniteState _toState = toState;
		private FiniteState _fromState = fromState;

		public FiniteState FromState
		{
			get => _fromState;
			set => _fromState = value;
		}
		public FiniteState ToState
		{
			get => _toState;
			set => _toState = value;
		}

		/// <summary>
		/// It's the <see cref="CanvasCoordinate"/> that the transition originates from the edge
		/// of the state's <see cref="FiniteState.Radius"/>.
		/// <para>Note: If the <see cref="FromState"/> is <see langword="null"/>
		/// then it will return coordinates (0, 0)</para>
		/// </summary>
		public CanvasCoordinate FromCoord
		{
			get
			{
				if (!_isCurved)
				{
					float radius = 1;
					if (_fromState.Radius > 0)
						radius = _fromState.Radius;
					double angle = Angle;
					double y = (Math.Sin(angle) * radius) + _fromState.Coordinate.Y;
					double x = (Math.Cos(angle) * radius) + _fromState.Coordinate.X;
					return new (x, y);
				}
				else
				{
					double fromAngle = FromAngle;
					double x = _centerArc.X + (_radius * Math.Cos(fromAngle));
					double y = _centerArc.Y + (_radius + Math.Sin(fromAngle));
					return new (x, y);
				}
			}
			set => _fromState.Coordinate = value;
		}

		/// <summary>
		/// Uses one of the formula of a
		/// <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">circle segment</a>
		/// to get the angle.
		/// </summary>
		public double FromAngle
		{
			get
			{
				if (_isCurved)
				{
					CanvasCoordinate fromCoord = _fromState.Coordinate;
					double fromAngle = Math.Atan2(fromCoord.Y - _centerArc.Y, fromCoord.X - _centerArc.X);
					return fromAngle + (_fromState.Radius / _radius);
				}
				return Angle;
			}
		}

		/// <summary>
		/// It's the <see cref="CanvasCoordinate"/> that the transition goes to the edge
		/// of the state's <see cref="FiniteState.Radius"/>.
		/// <para>Note: If the <see cref="ToState"/> is <see langword="null"/>
		/// then it will return coordinates (0, 0)</para>
		/// </summary>
		public CanvasCoordinate ToCoord
		{
			get
			{
				if (!_isCurved)
				{
					float radius = 1;
					if (_toState.Radius > 0)
						radius = _toState.Radius;
					double y = -(Math.Sin(Angle) * radius) + _toState.Coordinate.Y;
					double x = -(Math.Cos(Angle) * radius) + _toState.Coordinate.X;
					return new(x, y);
				}
				else
				{
					double toAngle = ToAngle;
					double x = _centerArc.X + (_radius * Math.Cos(toAngle));
					double y = _centerArc.Y + (_radius + Math.Sin(toAngle));
					return new(x, y);
				}
			}
			set => _toState.Coordinate = value;
		}

		/// <summary>
		/// Uses one of the formula of a
		/// <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">circle segment</a>
		/// to get the angle.
		/// </summary>
		public double ToAngle
		{
			get
			{
				if (_isCurved)
				{
					CanvasCoordinate toCoord = _toState.Coordinate;
					double toAngle = Math.Atan2(toCoord.Y - _centerArc.Y, toCoord.X - _centerArc.X);
					return toAngle - (_toState.Radius / _radius);
				}
				return Angle;
			}
		}

		/// <summary>
		/// Anti-clockwise angle between the coordinates of <see cref="FromState"/> and the <see cref="ToState"/>.
		/// </summary>
		public double Angle
		{
			get
			{
				CanvasCoordinate dCoord = _toState.Coordinate - _fromState.Coordinate;
				return Math.Atan2(dCoord.Y, dCoord.X);
			}
		}

		private CanvasCoordinate _centerArc;

		public CanvasCoordinate CenterArc
		{
			get
			{
				if (_isCurved)
					return _centerArc;
				else
					return new ();
			}
			set
			{
				if (_isCurved)
					_centerArc = value;
			}
		}

		public CanvasCoordinate Anchor
		{
			get;
			set;
		}

		public double ParrellelAxis
		{
			get => _parrallelAxis;
			set => _parrallelAxis = value;
		}

		private double _parrallelAxis = 0;

		public double PerpendicularAxis
		{
			get => _perpendicularAxis;
			set => _perpendicularAxis = value;
		}

		private double _perpendicularAxis = 0;

		private double _radius;

		public double Radius
		{
			get => _radius;
			set => _radius = value;
		}

		private bool _isCurved = false;

		public bool IsCurved
		{
			get => _isCurved;
			set
			{
				_isCurved = value;
			}
		}

		private string _text = string.Empty;

		public string Text
		{
			get => _text;
			set => _text = value;
		}

		public override int GetHashCode() => HashCode.Combine(_fromState, _toState);

		public override string ToString() =>
			$"( {_fromState} -> {_toState}, Text: '{_text}', Angle: {Angle}, Center Point: {_centerArc}, Radius: {_radius} )";
	}
}
