using System.Text.Json.Serialization;

namespace Finite_State_Machine_Designer.Models.FSM
{
	public class FiniteState
	{
		public FiniteState() => Id = Ulid.NewUlid().ToGuid();

		public FiniteState(CanvasCoordinate coord, float rad) : this()
		{
			_coordinate = coord;
			_radius = rad;
		}

		public Guid Id { get; set; }

        private CanvasCoordinate _coordinate;

		private float _radius;

        public CanvasCoordinate Coordinate
		{
			get => _coordinate;
			set => _coordinate = value;
		}

		public bool IsDrawable { get; set; } = true;

		public float Radius
		{
			get => _radius;
			set => _radius = value;
		}

		private bool _isFinalState = false;

		public bool IsFinalState
		{
			get => _isFinalState;
			set => _isFinalState = value;
		}

		private string _text = string.Empty;

		public string Text
		{
			get => _text;
			set => _text = value;
		}

		public static bool operator ==(FiniteState? state, FiniteState? other) =>
			state?.GetHashCode() == other?.GetHashCode();

		public static bool operator !=(FiniteState? state, FiniteState? other) =>
			state?.GetHashCode() != other?.GetHashCode();

		public override bool Equals(object? obj)
		{
			if (obj is FiniteState state)
				return GetHashCode() == state.GetHashCode();
			return false;
		}

		public override int GetHashCode() => HashCode.Combine(Id, _coordinate, _radius, _isFinalState, IsDrawable, _text);

		public override string ToString()
		{
			string text = $"co-ordinate: {_coordinate}, Radius: {_radius}, Text: '{_text}', FinalState: {_isFinalState})";
			if (Id != Guid.Empty)
				text = $"(id: {Id}, {text}";
			else
				text = $"({text}";
			return text;
		}
    }
}
