namespace Finite_State_Machine_Designer.Client.FSM
{
    public static class FSMUtilExtensions
    {
        /// <summary>
		/// Links <see cref="IFiniteStateMachine.Transitions"/> to <see cref="IFiniteStateMachine.States"/> and
		/// removes state duplicates.
		/// </summary>
		/// <returns>Finite state machine with no duplicate state objects</returns>
		public static void LinkStatesToTransitions(this IFiniteStateMachine fsm)
        {
            foreach (var transition in fsm.Transitions)
                foreach (var state in fsm.States)
                {
                    if (state == transition.FromState)
                        transition.FromState = state;
                    if (state == transition.ToState)
                        transition.ToState = state;
                }
        }
    }
}
