namespace Finite_State_Machine_Designer.Client.FSM
{
	public class StateTransition(FiniteState fromState, FiniteState toState)
	{
		private readonly FiniteState _toState = toState;
		private readonly FiniteState _fromState = fromState;

		public CanvasCoordinate FromCoord
		{
			get
			{
				if (_fromState is not null)
					return _fromState.Coordinate;
				return default;
			}
			set
			{
				if (_fromState is not null)
					_fromState.Coordinate = value;
			}
		}

		public CanvasCoordinate ToCoord
		{
			get
			{
				if (_toState is not null)
					return _toState.Coordinate;
				return default;
			}
			set
			{
				if (_toState is not null)
					_toState.Coordinate = value;
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
			$"( {_fromState} -> {_toState}, Text: '{_text}', Center Arc: {_centerArc}, Radius: {_radius} )";
	}
}
