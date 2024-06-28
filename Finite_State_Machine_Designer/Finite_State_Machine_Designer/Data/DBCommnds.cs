using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Finite_State_Machine_Designer.Data
{
    public static class DBCommnds
    {
        /// <summary>
        /// Gets List of FSMs depending on page number and size of page
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async static Task<List<FiniteStateMachine>> GetPageOfFsmsAsync(DbContext dbContext,
            string userId, int page, int pageSize)
            => await dbContext.Database.SqlQuery<FiniteStateMachine>($@"SELECT *
                    FROM dbo.StateMachines
                    WHERE ApplicationUserId = {userId}
                    ORDER BY TimeUpdated DESC
                    OFFSET {page * pageSize} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY").ToListAsync();

        /// <summary>
        /// Adds the FSM to a database with a link to user's id via raw SQL queries.
        /// <para>This method doens't have a transaction within it.</para>
        /// <para>NOTE: When <paramref name="newGuids"/> is <see langword="true"/> it also updates TimeCreated of FSM
        /// to <see cref="DateTime.UtcNow"/>.</para>
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/> where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs when they're are no GUIDs
        /// <para>NOTE: When <see langword="true"/> it also updates TimeCreated of FSM
        /// to <see cref="DateTime.UtcNow"/>.</para>
        /// </param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task AddFSMAsync(DbContext dbContext, FiniteStateMachine fsm, string userId,
            bool newGuids = true)
        {
            if (newGuids || !Guid.TryParse(fsm.Id, out _))
            {
                fsm.Id = Guid.NewGuid().ToString();
                fsm.TimeCreated = DateTime.UtcNow;
            }
            fsm.TimeUpdated = DateTime.UtcNow;
            
            try
            {
                await dbContext.Database.ExecuteSqlAsync($@"INSERT INTO dbo.StateMachines
                (Id, ApplicationUserId, Name, Description, Width, Height,
                TransitionSearchRadius, timeCreated, timeUpdated)
                VALUES ({fsm.Id}, {userId}, {fsm.Name}, {fsm.Description}, {fsm.Width}, {fsm.Height},
                {fsm.TransitionSearchRadius}, {fsm.TimeCreated}, {fsm.TimeUpdated})");

                await AddStatesAsync(dbContext, fsm, newGuids);
                await AddTransitionsAsync(dbContext, fsm, newGuids);
            }
            catch (OperationCanceledException)
            {
                if (newGuids)
                    fsm.Id = string.Empty;
                throw;
            }
        }

        /// <summary>
        /// Updates the whole FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task UpdateFsmAsync(DbContext dbContext, FiniteStateMachine fsm, string userId)
        {
            await dbContext.Database.ExecuteSqlAsync(@$"DELETE FROM dbo.StateMachines
            WHERE Id = {fsm.Id}");

            await AddFSMAsync(dbContext, fsm, userId, false);
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
        public async static Task AddStatesAsync(DbContext dbContext,
            FiniteStateMachine fsm, bool newGuids = true)
        {
            if (fsm.States.Count <= 0)
                return;
            List<SqlParameter> parameters = [];
            List<FiniteState> addedStates = [];
            string insertStatesCommandPrefix = $@"INSERT INTO dbo.States
            (Id, FiniteStateMachineId, IsDrawable, IsFinalState,
            Coordinate_X, Coordinate_Y, Radius, Text)
            VALUES";
            string insertStatesCommand = insertStatesCommandPrefix;
            for (int i = 0; i < fsm.States.Count; i++)
            {
                FiniteState state = fsm.States[i];
                if (newGuids || !Guid.TryParse(state.Id, out _))
                {
                    state.Id = Guid.NewGuid().ToString();
                    addedStates.Add(state);
                }
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
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(insertStatesCommand[..^1], parameters);
            }
            catch (OperationCanceledException)
            {
                foreach (var state in addedStates)
                    state.Id = string.Empty;
                throw;
            }
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
        public async static Task AddTransitionsAsync(DbContext dbContext,
            FiniteStateMachine fsm, bool newGuids = true)
        {
            if (fsm.Transitions.Count <= 0)
                return;
            List<SqlParameter> parameters = [];
            List<Transition> addedTransitions = [];
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
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(insertTransitionsCommand[..^1], parameters);
            }
            catch (OperationCanceledException)
            {
                foreach (Transition trannsition in addedTransitions)
                    trannsition.Id = string.Empty;
                throw;
            }
        }
    }
}
