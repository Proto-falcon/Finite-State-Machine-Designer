namespace Finite_State_Machine_Designer.Client.FSM
{
	public class StateTransition(CanvasCoordinate from, CanvasCoordinate to)
	{
		public CanvasCoordinate From
		{
			get => from;
			set => from = value;
		}

		public CanvasCoordinate To
		{
			get => to;
			set => to = value;
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

		public override int GetHashCode() => HashCode.Combine(from, to);

		public override string ToString()
		{
			string textTransition = $"( {from} -> {to}";
			if (!string.IsNullOrWhiteSpace(_text))
				textTransition += $", Text: '{_text}'";
			if (_centerArc is not null)
				textTransition += $", Center Arc: {_centerArc}";
			if (_radius > 0)
				textTransition += $", Radius: {_radius}";

			return textTransition + " )";
		}
	}
}
