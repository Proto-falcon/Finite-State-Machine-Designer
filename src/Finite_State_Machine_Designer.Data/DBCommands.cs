using Finite_State_Machine_Designer.Data.Identity;
using Finite_State_Machine_Designer.Models.FSM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Finite_State_Machine_Designer.Data
{
    public static class DBCommands
    {
        /// <summary>
        /// Deletes older FSMs.
        /// </summary>
        /// <param name="dbContext">Database Context to query with.</param>
        /// <param name="user">User with Finite State Machines</param>
        /// <param name="skipNum">Number of FSMs to skip.</param>
        public async static Task DeleteOldModifiedFsms(ApplicationDbContext dbContext, ApplicationUser user, int skipNum)
        {
            FiniteStateMachine? oldFsm = await dbContext.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(user => user.StateMachines)
                .OrderByDescending(fsm => fsm.TimeUpdated)
                .Skip(skipNum)
                .FirstAsync();
            await dbContext.StateMachines
                .Where(fsm => fsm.TimeUpdated <= oldFsm.TimeUpdated)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// <para>Delete Fsms in the database via Ids.</para>
        /// <b>Note:</b> Deletes them wihtin a transaction so can't use it with
        /// other database contexts with an already existing transaction.
        /// </summary>
        /// <param name="dbContext">Database Context to query with.</param>
        /// <param name="fsmIds">Finite State Machine Ids</param>
        /// <returns>
        /// <see langword="true"/> for successfully deleting FSMs,
        /// otherwise <see langword="false"/> for unsuccessful.
        /// </returns>
        public async static Task<bool> DeleteFsms(DbContext dbContext,
            ILogger? logger = null,
            params Guid[] fsmIds)
        {
            if (fsmIds.Length <= 0)
                return true;
            bool isDeleted = false;
            string deleteQuery = "DELETE dbo.StateMachines WHERE Id IN ( ";

            List<SqlParameter> parameters = [];

            for (int i = 0; i < fsmIds.Length; i++)
            {
                Guid fsmId = fsmIds[i];
                parameters.Add(new SqlParameter($"id{i}", fsmId));
                deleteQuery += $"@id{i}, ";
            }
            deleteQuery = deleteQuery.TrimEnd(',', ' ') + " )";
            var transactStrategy = dbContext.Database.CreateExecutionStrategy();
            try
            {
                await transactStrategy.ExecuteAsync(async () =>
                {
                    await using var transact = await dbContext.Database.BeginTransactionAsync();
                    await dbContext.Database.ExecuteSqlRawAsync(deleteQuery, parameters);
                    await transact.CommitAsync();
                    isDeleted = true;
                });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Coudln't delete the Finite State Machines");
                isDeleted = false;
            }

            return isDeleted;
        }

        /// <summary>
        /// Gets page of FSMs from user
        /// </summary>
        /// <param name="dbContext">Database Context to query with.</param>
        /// <param name="user">User with Finite State Machines</param>
        /// <param name="maxTime">
        /// The time to get more least recent updated FSMs
        /// </param>
        /// <param name="includeMax">Include the fsm with max time</param>
        /// <param name="numOfFsms"></param>
        /// <returns><see langword="true"/> for more available FSMs in next page,
        /// otherwise no more FSMs.</returns>
        public async static Task<bool> FetchPageFsmsAsync(DbContext dbContext,
            ApplicationUser user, DateTime maxTime, bool includeMax = false, int numOfFsms = int.MaxValue)
        {
            var query = dbContext.Entry(user)
                .Collection(appUser => appUser.StateMachines)
                .Query();
            if (includeMax)
                query = query.Where(fsm => fsm.TimeUpdated <= maxTime);
            else
                query = query.Where(fsm => fsm.TimeUpdated < maxTime);
            List<FiniteStateMachine> newFsms = await query
                .OrderByDescending(fsm => fsm.TimeUpdated)
                .Take(numOfFsms + 1)
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();
            user.StateMachines = [];
            if (newFsms.Count > numOfFsms)
                user.StateMachines = newFsms[..^1];
            else
                user.StateMachines = newFsms;
            if (newFsms.Count <= numOfFsms)
                return false;
            return true;
        }

        /// <summary>
        /// Loads all the states and transitions of FSM.
        /// </summary>
        /// <param name="dbContext">Database Context to query with.</param>
        /// <param name="fsm">Finite State Machine</param>
        /// <param name="cancelToken">A cancellation token</param>
        public async static Task<FsmResultState> GetFullFsmAsync(DbContext dbContext, 
            FiniteStateMachine fsm, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
                return FsmResultState.Interrupted;
            List<Transition> prevTransitions = fsm.Transitions;
            fsm.Transitions = [];
            List<FiniteState> prevStates = fsm.States;
            fsm.States = [];
            EntityEntry<FiniteStateMachine> fsmEntry = dbContext.Attach(fsm);
            fsm.States = await fsmEntry
                .Collection(stateMachine => stateMachine.States)
                .Query()
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync(cancelToken);
            if (cancelToken.IsCancellationRequested)
            {
                fsm.States = prevStates;
                fsm.Transitions = prevTransitions;
                return FsmResultState.Interrupted;
            }

            fsm.Transitions = await fsmEntry
                .Collection(stateMachine => stateMachine.Transitions)
                .Query()
                .AsNoTracking()
                .ToListAsync(cancelToken);

            List<Transition> validTransitions = [];

            foreach (Transition transition in fsm.Transitions)
            {
                if (fsm.States
                        .Find(state => state.Id == transition.FromStateId)
                    is FiniteState fromState)
                    transition.FromState = fromState;
                if (fsm.States
                        .Find(state => state.Id == transition.ToStateId)
                    is FiniteState toState)
                    transition.ToState = toState;

                if (transition.FromState is not null
                    && transition.ToState is not null)
                    validTransitions.Add(transition);
            }

            fsm.Transitions = validTransitions;

            return FsmResultState.Success;
        }

        /// <summary>
        /// Adds the FSM to a database with a link to user's id via raw SQL queries.
        /// <para>This method doens't have a transaction within it.</para>
        /// <para>NOTE: When <paramref name="newGuids"/> is <see langword="true"/>
        /// it also updates TimeCreated of FSM
        /// to <see cref="DateTime.UtcNow"/>.</para>
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/>
        /// where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/>
        /// to only generate new GUIDs when they're are no GUIDs
        /// <para>NOTE: When <see langword="true"/> it also updates
        /// TimeCreated of FSM
        /// to <see cref="DateTime.UtcNow"/>.</para>
        /// </param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task AddFSMAsync(DbContext dbContext,
            FiniteStateMachine fsm, Guid userId,
            bool newGuids = true)
        {
            if (newGuids | fsm.Id == Guid.Empty)
            {
                fsm.Id = Ulid.NewUlid().ToGuid();
                fsm.TimeCreated = DateTime.UtcNow;
            }
            fsm.TimeUpdated = DateTime.UtcNow;
            
            await dbContext.Database
                .ExecuteSqlAsync($@"INSERT INTO dbo.StateMachines
            (Id, ApplicationUserId, Name, Description, Width, Height,
            TransitionSearchRadius, timeCreated, timeUpdated)
            VALUES ({fsm.Id}, {userId}, {fsm.Name}, {fsm.Description},
            {fsm.Width}, {fsm.Height}, {fsm.TransitionSearchRadius},
            {fsm.TimeCreated}, {fsm.TimeUpdated})");

            await AddStatesAsync(dbContext, fsm, newGuids);
            await AddTransitionsAsync(dbContext, fsm, newGuids);
        }

        /// <summary>
        /// Updates the whole FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <exception cref="OperationCanceledException"/>
        public async static Task UpdateFsmAsync(DbContext dbContext,
            FiniteStateMachine fsm, Guid userId)
        {
            await dbContext.Database
                .ExecuteSqlAsync(@$"DELETE FROM dbo.StateMachines
            WHERE Id = {fsm.Id}");

            await AddFSMAsync(dbContext, fsm, userId, false);
        }

        /// <summary>
        /// Adds a list of states from FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/>
        /// where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs 
        /// when they're are no GUIDs</param>
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
                if (newGuids | state.Id == Guid.Empty)
                {
                    state.Id = Ulid.NewUlid().ToGuid();
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

            await dbContext.Database
                .ExecuteSqlRawAsync(insertStatesCommand[..^1], parameters);
        }

        /// <summary>
        /// Adds a list of transitions from FSM
        /// </summary>
        /// <param name="fsm">New Finite State Machine</param>
        /// <param name="dbContext">dbContext to insert FSM to database</param>
        /// <param name="newGuids">By default it's <see langword="true"/>
        /// where it generates
        /// new GUIDs for states even if they're already an existing GUID,
        /// otherwise <see langword="false"/> to only generate new GUIDs 
        /// when they're are no GUIDs</param>
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
                if (newGuids | transition.Id == Guid.Empty)
                    transition.Id = Ulid.NewUlid().ToGuid();
                transition.FromStateId = transition.FromState.Id;
                transition.ToStateId = transition.ToState.Id;

                parameters.Add(new($"Id{i}", transition.Id));
                parameters.Add(new($"FsmId{i}", fsm.Id));
                parameters.Add(new($"FromStateId{i}", transition.FromStateId));
                parameters.Add(new($"ToStateId{i}", transition.ToStateId));
                parameters.Add(new($"Text{i}", transition.Text));
                parameters.Add(new($"ParallelAxis{i}", transition.ParallelAxis));
                parameters.Add(new($"MinPerpDist{i}",
                    transition.MinPerpendicularDistance));
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
            
            await dbContext.Database
                .ExecuteSqlRawAsync(insertTransitionsCommand[..^1], parameters);
        }
    }
}
