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
		/// of the state's <see cref="Radius"/>.
		/// <para>Note: If the <see cref="FromState"/> is <see langword="null"/>
		/// then it will return coordinates (0, 0)</para>
		/// </summary>
		public CanvasCoordinate FromCoord
		{
			get
			{
				if (_fromState is not null)
				{
					float radius = 1;
					if (_fromState.Radius > 0)
						radius = _fromState.Radius;
					int y = (int)(Math.Sin(Angle) * radius) + _fromState.Coordinate.Y;
					int x = (int)(Math.Cos(Angle) * radius) + _fromState.Coordinate.X;
					return new (x, y);
				}
				return default;
			}
			set
			{
				if (_fromState is not null)
					_fromState.Coordinate = value;
			}
		}

		/// <summary>
		/// It's the <see cref="CanvasCoordinate"/> that the transition goes to the edge
		/// of the state's <see cref="Radius"/>.
		/// <para>Note: If the <see cref="ToState"/> is <see langword="null"/>
		/// then it will return coordinates (0, 0)</para>
		/// </summary>
		public CanvasCoordinate ToCoord
		{
			get
			{
				if (_toState is not null)
				{
					float radius = 1;
					if (_toState.Radius > 0)
						radius = _toState.Radius;
					int y = -(int)(Math.Sin(Angle) * radius) + _toState.Coordinate.Y;
					int x = -(int)(Math.Cos(Angle) * radius) + _toState.Coordinate.X;
					return new(x, y);
				}
				return default;
			}
			set
			{
				if (_toState is not null)
					_toState.Coordinate = value;
			}
		}

		/// <summary>
		/// Anti-clockwise angle between the <see cref="FromState"/> and the <see cref="ToState"/>.
		/// <para>Note: If the <see cref="FiniteState.Radius"/> is negligible on
		/// or <see cref="FiniteState.IsDrawable"/> is <see langword="false"/>
		/// either state.</para>
		/// Then it's between <see cref="FromCoord"/> and or <see cref="ToCoord"/>
		/// </summary>
		public double Angle
		{
			get
			{
				if (_fromState is not null && _toState is not null)
				{
					CanvasCoordinate dCoord = _toState.Coordinate - _fromState.Coordinate;
					return Math.Atan2(dCoord.Y, dCoord.X);
				}
				return 0;
			}
		}

		private CanvasCoordinate? _centerArc = null;

		public CanvasCoordinate? CenterArc
		{
			get => _centerArc;
			set => _centerArc = value;
		}

		private int _radius;

		public int Radius
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
				if (_isCurved)
					_centerArc = null;
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
			$"( {_fromState} -> {_toState}, Text: '{_text}', Angle: {Angle}, Center Arc: {_centerArc}, Radius: {_radius} )";
	}
}
