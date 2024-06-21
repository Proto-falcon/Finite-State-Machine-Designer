using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Finite_State_Machine_Designer.Data
{
    public static class DBCommnds
    {
        /// <summary>
        /// Adds the FSM to a database with a link to user's id via raw SQL queries.
        /// <para>This method doens't have a transaction within it.</para>
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/> where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs when they're are no GUIDs</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task AddFSM(DbContext dbContext, FiniteStateMachine fsm, string userId,
            bool newGuids = true)
        {
            if (newGuids || !Guid.TryParse(fsm.Id, out _))
                fsm.Id = Guid.NewGuid().ToString();

            await dbContext.Database.ExecuteSqlAsync($@"INSERT INTO dbo.StateMachines
            (Id, ApplicationUserId, Name, Description, Width, Height, TransitionSearchRadius)
            VALUES ({fsm.Id}, {userId}, {fsm.Name}, {fsm.Description}, {fsm.Width}, {fsm.Height},
            {fsm.TransitionSearchRadius})");

            await AddStates(dbContext, fsm, newGuids);
            await AddTransitions(dbContext, fsm, newGuids);
        }

        /// <summary>
        /// Updates the whole FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task UpdateFsm(DbContext dbContext, FiniteStateMachine fsm, string userId)
        {
            await dbContext.Database.ExecuteSqlAsync(@$"DELETE FROM dbo.StateMachines
            WHERE Id = {fsm.Id}");

            await AddFSM(dbContext, fsm, userId, false);
        }

        /// <summary>
        /// Adds a list of states from FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/> where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs when they're are no GUIDs</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task AddStates(DbContext dbContext,
            FiniteStateMachine fsm, bool newGuids = true)
        {
            List<SqlParameter> parameters = [];
            string insertStatesCommandPrefix = $@"INSERT INTO dbo.States
            (Id, FiniteStateMachineId, IsDrawable, IsFinalState,
            Coordinate_X, Coordinate_Y, Radius, Text)
            VALUES";
            string insertStatesCommand = insertStatesCommandPrefix;
            for (int i = 0; i < fsm.States.Count; i++)
            {
                FiniteState state = fsm.States[i];
                if (newGuids || !Guid.TryParse(state.Id, out _))
                    state.Id = Guid.NewGuid().ToString();
                parameters.Add(new($"Id{i}", state.Id));
                parameters.Add(new($"FsmId{i}", fsm.Id));
                parameters.Add(new($"Drawable{i}", state.IsDrawable));
                parameters.Add(new($"Final{i}", state.IsFinalState));
                parameters.Add(new($"CoordX{i}", state.Coordinate.X));
                parameters.Add(new($"CoordY{i}", state.Coordinate.Y));
                parameters.Add(new($"Radius{i}", state.Radius));
                parameters.Add(new($"Text{i}", state.Text));

                insertStatesCommand += " " + $@"(@Id{i}, @FsmId{i}, @Drawable{i},
                @Final{i}, @CoordX{i}, @CoordY{i}, @Radius{i}, @Text{i}),";
            }
            await dbContext.Database.ExecuteSqlRawAsync(insertStatesCommand[..^1], parameters);
        }

        /// <summary>
        /// Adds a list of transitions from FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/> where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs when they're are no GUIDs</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task AddTransitions(DbContext dbContext,
            FiniteStateMachine fsm, bool newGuids = true)
        {
            List<SqlParameter> parameters = [];
            string insertTransitionsCommand = $@"INSERT INTO dbo.Transitions
            (Id, FiniteStateMachineId, FromStateId, ToStateId, Text, ParallelAxis,
	        MinPerpendicularDistance, PerpendicularAxis, SelfAngle, Radius,
            CenterArc_X, CenterArc_Y, IsReversed) VALUES";

            for (int i = 0; i < fsm.Transitions.Count; i++)
            {
                Transition transition = fsm.Transitions[i];
                if (newGuids || !Guid.TryParse(transition.Id, out _))
                    transition.Id = Guid.NewGuid().ToString();
                transition.FromStateId = transition.FromState.Id;
                transition.ToStateId = transition.ToState.Id;

                parameters.Add(new($"Id{i}", transition.Id));
                parameters.Add(new($"FsmId{i}", fsm.Id));
                parameters.Add(new($"FromStateId{i}", transition.FromStateId));
                parameters.Add(new($"ToStateId{i}", transition.ToStateId));
                parameters.Add(new($"Text{i}", transition.Text));
                parameters.Add(new($"ParallelAxis{i}", transition.ParallelAxis));
                parameters.Add(new($"MinPerpDist{i}", transition.MinPerpendicularDistance));
                parameters.Add(new($"PerAxis{i}", transition.PerpendicularAxis));
                parameters.Add(new($"SelfAngle{i}", transition.SelfAngle));
                parameters.Add(new($"Radius{i}", transition.Radius));
                parameters.Add(new($"CentreX{i}", transition.CenterArc.X));
                parameters.Add(new($"CentreY{i}", transition.CenterArc.Y));
                parameters.Add(new($"Reversed{i}", transition.IsReversed));

                if (i > 0)
                    insertTransitionsCommand += Environment.NewLine;
                insertTransitionsCommand += 
                    $@"(@Id{i}, @FsmId{i}, @FromStateId{i}, @ToStateId{i}, @Text{i},
                @ParallelAxis{i}, @MinPerpDist{i}, @PerAxis{i}, @SelfAngle{i},
                @Radius{i}, @CentreX{i}, @CentreY{i}, @Reversed{i}),";
            }
            await dbContext.Database.ExecuteSqlRawAsync(insertTransitionsCommand[..^1], parameters);
        }

        public async static Task GetFullFsm(DbContext dbContext, FiniteStateMachine fsm)
        {
            fsm.States = await dbContext.Database.SqlQuery<FiniteState>($@"SELECT * FROM dbo.States
            WHERE FiniteStateMachineId = {fsm.Id}").ToListAsync();

            fsm.Transitions = await dbContext.Database.SqlQuery<Transition>($@"SELECT * FROM dbo.Transitions
            WHERE FiniteStateMachineId = {fsm.Id}").ToListAsync();
        }
    }
}
