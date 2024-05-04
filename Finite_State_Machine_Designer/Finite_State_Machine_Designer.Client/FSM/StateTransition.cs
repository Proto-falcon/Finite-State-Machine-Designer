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
				if (!IsCurved)
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
		/// It's the <see cref="CanvasCoordinate"/> that the transition goes to the edge
		/// of the state's <see cref="FiniteState.Radius"/>.
		/// <para>Note: If the <see cref="ToState"/> is <see langword="null"/>
		/// then it will return coordinates (0, 0)</para>
		/// </summary>
		public CanvasCoordinate ToCoord
		{
			get
			{
				if (!IsCurved)
				{
					float radius = 1;
					if (_toState.Radius > 0)
						radius = _toState.Radius;
					double y = (Math.Sin(Angle + Math.PI) * radius) + _toState.Coordinate.Y;
					double x = (Math.Cos(Angle + Math.PI) * radius) + _toState.Coordinate.X;
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
		/// to get the angle from positive x axis.
		/// </summary>
		public double FromAngle
		{
			get
			{
				if (IsCurved)
				{
					CanvasCoordinate fromCoord = _fromState.Coordinate;
					double fromAngle = Math.Atan2(fromCoord.Y - _centerArc.Y, fromCoord.X - _centerArc.X);
					/// 2 * Math.Asin(_fromState.Radius / (_radius * 2)) ≈ _fromState.Radius / _radius 
					/// ≈ angle of the segment by around 3 sig figures
					/// This is due to numbers less than 10^-3 stop making noticable differences on the canvas.
					return fromAngle + ((IsReversed ? -1 : 1) * (_fromState.Radius / _radius));
				}
				return Angle;
			}
		}

		/// <summary>
		/// Uses one of the formula of a
		/// <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">circle segment</a>
		/// to get the angle from positive x axis.
		/// </summary>
		public double ToAngle
		{
			get
			{
				if (IsCurved)
				{
					CanvasCoordinate toCoord = _toState.Coordinate;
					double toAngle = Math.Atan2(toCoord.Y - _centerArc.Y, toCoord.X - _centerArc.X);
					/// 2 * Math.Asin(_toState.Radius / (_radius * 2)) ≈ _toState.Radius / _radius 
					/// ≈ angle of the segment by around 3 sig figures
					/// This is due to numbers less than 10^-3 stop making noticable differences on the canvas.
					return toAngle - ((IsReversed ? -1 : 1) * (_toState.Radius / _radius));
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
				if (IsCurved)
					return _centerArc;
				else
					return default;
			}
			set => _centerArc = value;
		}

		public CanvasCoordinate Anchor
		{
			get
			{
				CanvasCoordinate dCoord = new(_toState.Coordinate.X - _fromState.Coordinate.X,
					_toState.Coordinate.Y - _fromState.Coordinate.Y);
				return new (_fromState.Coordinate.X + (dCoord.X * _parallelAxis) - (dCoord.Y * _perpendicularAxis),
					_fromState.Coordinate.Y + (dCoord.Y * _parallelAxis) + (dCoord.X * _perpendicularAxis));
			}
			set
			{
				CanvasCoordinate dCoord = new (_toState.Coordinate.X - _fromState.Coordinate.X,
					_toState.Coordinate.Y - _fromState.Coordinate.Y);
				double squareLength = (dCoord.X * dCoord.X) + (dCoord.Y * dCoord.Y);

				CanvasCoordinate dCoord2 = new (value.X - _fromState.Coordinate.X,
					value.Y - _fromState.Coordinate.Y);

				/// Using Dot Product
				_parallelAxis = ((dCoord.X*dCoord2.X) + (dCoord.Y*dCoord2.Y)) / squareLength;
				/// Using Determinant
				_perpendicularAxis = ((dCoord.X * dCoord2.Y) - (dCoord.Y * dCoord2.X)) / squareLength;

				_isReversed = _perpendicularAxis > 0;
				if (Math.Abs(_perpendicularAxis) < _minPerpendicularDistance)
				{
					_radius = 0;
					_perpendicularAxis = 0;
				}
			}
		}

		private double _parallelAxis;
		private double _perpendicularAxis;

		public double MinPerpendicularDistance
		{
			get => _minPerpendicularDistance;
			set => _minPerpendicularDistance = value;
		}

		private double _minPerpendicularDistance = 0.02;

		public bool IsReversed => _isReversed;

		private bool _isReversed;

		private double _radius;

		public double Radius
		{
			get => _radius;
			set => _radius = value;
		}

		public bool IsCurved => Math.Abs(_perpendicularAxis) >=  0.02;

		private string _text = string.Empty;

		public string Text
		{
			get => _text;
			set => _text = value;
		}

		public override int GetHashCode() => HashCode.Combine(_fromState, _toState);

		public override string ToString()
		{
			string text = $"( {_fromState} -> {_toState}, Text: '{_text}', ";

			if (!IsCurved)
				text += $"Angle: {Angle} )";
			else
				text += $"From Angle: {FromAngle}, To Angle: {ToAngle}, Center Point: {_centerArc}, Radius: {_radius}, Reversed: {IsReversed} )";

            return text;
		}
	}
}
