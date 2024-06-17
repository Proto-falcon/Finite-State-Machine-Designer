namespace Finite_State_Machine_Designer.Client.FSM
{
    public static class FSMUtilExtensions
    {
        /// <summary>
        /// <para>Removes any non-drawables not linked to transitions.</para>
        /// <para>Removes transitions with non-drawables states set
        /// to <see cref="Transition.ToState"/></para>
        /// <para>Adds states linked to transitions to list of states if they're not already on it.</para>
		/// Links <see cref="IFiniteStateMachine.Transitions"/> to <see cref="IFiniteStateMachine.States"/> and
		/// removes state duplicates.
		/// </summary>
		/// <returns>Finite state machine with no duplicate state objects</returns>
		public static void SanitiseFsm(this IFiniteStateMachine fsm)
        {
            /// Remove any transitions that have <see cref="Transition.ToState.IsDrawable"/> set to false.
            fsm.Transitions = fsm.Transitions.Where(transition => transition.ToState.IsDrawable).ToList();

            /// Dedupe the instances of states and also add state instances in transitions
            /// when a match isn't found in the list of states
            foreach (Transition transition in fsm.Transitions)
            {
                bool fromStateDeDuped = false;
                bool toStateDeDuped = false;
                foreach (FiniteState state in fsm.States)
                {
                    if (ReferenceEquals(state, transition.FromState))
                        fromStateDeDuped = true;
                    else if (state == transition.FromState && !ReferenceEquals(state, transition.FromState))
                    {
                        transition.FromState = state;
                        fromStateDeDuped = true;
                    }

                    if (!ReferenceEquals(state, transition.ToState))
                        toStateDeDuped = true;
                    else if (state == transition.ToState && !ReferenceEquals(state, transition.ToState))
                    {
                        transition.ToState = state;
                        toStateDeDuped = true;
                    }

                    if (fromStateDeDuped && toStateDeDuped)
                        break;
                }
                if (!fromStateDeDuped)
                    fsm.States.Add(transition.FromState);
                if (!toStateDeDuped)
                    fsm.States.Add(transition.ToState);
            }

            List<FiniteState> validStates = [];

            foreach (FiniteState state in fsm.States)
            {
                if (state.IsDrawable)
                {
                    validStates.Add(state);
                    continue;
                }
                if (fsm.Transitions.Any(transition => transition.FromState == state))
                    validStates.Add(state);
            }

            fsm.States = validStates;
        }
    }
}
