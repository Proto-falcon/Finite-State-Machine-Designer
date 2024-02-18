namespace Finite_State_Machine_Designer.Client.FSM
{
    public class FiniteStateMachine
    {
        public List<FiniteState> States { get => _states; }
        private readonly List<FiniteState> _states = [];

        public FiniteState? SelectedState
        {
            get => _selectedState;
            set => _selectedState = value;
        }
        private FiniteState? _selectedState;

        public void AddState(FiniteState state)
        {
            _states.Add(state);
            _selectedState = state;
        }

        public void AddState(CanvasCoordinate coordinate, float radius)
        {
            var state = new FiniteState(coordinate, radius);
            AddState(state);
        }

        public FiniteState? FindState(CanvasCoordinate coordinate)
        {
            // Equation of Circle -> (x-a)^2 + (y-b)^2 <= r^2
            // where (a,b) are x and y coordinates of the centre of the circle respectively
            // r is radius of circle
			foreach (var state in _states)
            {
                var coord = state.Coordinate;
                var leftSide = Math.Pow(coordinate.X - coord.X, 2) + Math.Pow(coordinate.Y - coord.Y, 2);
                var rightSide = Math.Pow(state.Radius, 2);
				if (leftSide <= rightSide)
                {
                    return state;
                }
            }
            return null;
        }

        public bool RemoveState(FiniteState stateToBeRemoved)
        {
            foreach (var state in _states)
            {
                if (stateToBeRemoved == state)
                {
                    _states.Remove(state);
                    return true;
                }
            }
            return false;
        }
    }
}
