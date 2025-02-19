using Finite_State_Machine_Designer.Models.FSM;
using Finite_State_Machine_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Finite_State_Machine_Designer.Data.Identity;
using Finite_State_Machine_Designer.Enums;

namespace Finite_State_Machine_Designer.Components.Pages
{
    public partial class MyFSMs
    {
        /// <summary>
        /// Gets <see cref="_user"/> FSMs from database.
        /// Doesn't correctly load states and transitions.
        /// </summary>
        /// <param name="maxTime">Max Time to filter the Fsms.</param>
        /// <param name="includeMax">Include the fsm with max time</param>
        /// <returns><see langword="true"/> for completing successfully, otherwise <see langword="false"/>.</returns>
        private async Task<bool> GetUserFsmsPage(DateTime maxTime, bool includeMax = false)
        {
            bool completed = false;
            if (_user is not null)
            {
                await using ApplicationDbContext dbContext
                    = await DbFactory.CreateDbContextAsync();
                try
                {
                    _loadMoreFsms = await DBCommands.FetchPageFsmsAsync(dbContext, _user, maxTime,
                        includeMax, _userConfig.Value.VisibleFsmsLimit);
                    completed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Couldn't get Fsms from user - '{User}'", _user.Id);
                    _logger.LogError("{Error}", ex.ToString());
                }
            }
            return completed;
        }

        /// <summary>
        /// Gets the user and their basic Finite State Machines' information.
        /// </summary>
        private async Task InitialiseUserAsync()
        {
            string? userName = "";
            ApplicationUser? fetchedUser = null;
            await using (ApplicationDbContext dbContext =
                await DbFactory.CreateDbContextAsync())
            {
                userName = userService.GetUser().Identity?.Name;
                if (userName is null)
                {
                    Navigation.NavigateTo("Account/Login");
                    return;
                }
                fetchedUser = dbContext.Users
                    .Where(user => user.NormalizedUserName == userName.ToUpper())
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefault();
            }
            if (fetchedUser is not null)
            {
                _user = fetchedUser;
                await GetUserFsmsPage(DateTime.MaxValue);
                if (_user.StateMachines.Count > 0)
                    _leastRecentModifiedTime = _user.StateMachines
                        .Last().TimeUpdated;
                _finishLoading = true;
            }
            else
            {
                _finishLoading = true;
                Navigation.NavigateTo("Account/Login");
            }
        }

        /// <summary>
        /// Generates an SVG of the Finite State Machine
        /// </summary>
        /// <param name="fsm">Finite State Machine</para>
        /// <param name="scale">Scale the FSM from original canvas dimensions</para>
        private async Task GenerateFsmSvgAsync(
            FiniteStateMachine fsm, double scale = 1)
        {
            if (CheckJsModule(JsModule))
            {
                _fsmSvgUrl = await JsModule.InvokeAsync<string>("fSMExport.fsmToSVG",
                    fsm, fsm.Width, fsm.Height, _colour,
                    _canvasBackgroundColour, 2, scale, true);
            }
        }

        /// <summary>
        /// Saves the current FSM in use to database.
        /// </summary>
        /// <returns>Save State of the current FSM</returns>
        private async Task<SaveState> SaveCurrentFSMAsync()
        {
            if (CheckJsModule(JsModule))
            {
                _saveFsm = false;
                _fsmSaveState = SaveState.Saving;
                _errorMsg = "";
                StateHasChanged();
                if (_currentDrawnFsm is not null && _user is not null)
                {
                    if (string.IsNullOrWhiteSpace(_currentDrawnFsm.Name))
                    {
                        _fsmSaveState = SaveState.Failed;
                        _errorMsg = "Please enter a name.";
                    }
                    else
                        try
                        {
                            await using (ApplicationDbContext dbContext =
                                await DbFactory.CreateDbContextAsync())
                            {
                                VerifyFsm(dbContext, _currentDrawnFsm);
                                FiniteStateMachine? existingFSM =
                                    await dbContext.Entry(_user)
                                    .Collection(user => user.StateMachines)
                                    .Query()
                                    .Where(fsm => fsm.Name == _currentDrawnFsm.Name)
                                    .FirstOrDefaultAsync();
                                if (existingFSM is not null)
                                {
                                    await using var transact =
                                        await dbContext.Database.BeginTransactionAsync();
                                    try
                                    {
                                        _currentDrawnFsm.Id = existingFSM.Id;
                                        await DBCommands.UpdateFsmAsync(
                                            dbContext,
                                            _currentDrawnFsm,
                                            _user.Id);
                                        await transact.CommitAsync();
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        await transact.RollbackAsync();
                                        throw;
                                    }
                                }
                                else
                                {
                                    await using var transact =
                                        await dbContext.Database.BeginTransactionAsync();
                                    try
                                    {
                                        await DBCommands.AddFSMAsync(
                                            dbContext,
                                            _currentDrawnFsm,
                                            _user.Id);
                                        await transact.CommitAsync();
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        await transact.RollbackAsync();
                                        throw;
                                    }
                                }
                            }
                            _fsmSaveState = SaveState.Saved;
                            await JsModule.InvokeVoidAsync(
                                "fSMCanvasUtils.saveFSM", _currentDrawnFsm);
                            _logger.LogInformation(
                                "Successfully saved the current "
                                + "FSM '{FSM}' from user '{user}'",
                                _currentDrawnFsm.Name, _user.Id);
                            _fsmSaveState = SaveState.Saved;
                        }
                        catch (Exception ex)
                        {
                            _fsmSaveState = SaveState.Failed;
                            string? errMsg = ex.Message.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            _errorMsg = $"Couldn't save!! '{errMsg}'";
                            _logger.LogError(
                                "Couldn't save the current FSM "
                                + "'{FsmName}' from user '{user}'",
                            _currentDrawnFsm.Name, _user.Id);
                            _logger.LogError("{ERROR}", ex.ToString());
                        }
                }
                StateHasChanged();
                return _fsmSaveState;
            }
            return SaveState.Failed;
        }

        private static void VerifyFsm(ApplicationDbContext dbContext, FiniteStateMachine fsm)
        {
            if (fsm.Name.Length > dbContext.FsmNameLimit)
                throw new InvalidOperationException($"Name must be below o {dbContext.FsmNameLimit+1} characters long");
            if (fsm.Description.Length > dbContext.FsmDescLimit)
                throw new InvalidOperationException($"Description must be below {dbContext.FsmDescLimit+1} characters long");
            foreach (var state in fsm.States)
                if (state.Text.Length > dbContext.FsmTextLimit)
                    throw new InvalidOperationException($"State text must be below {dbContext.FsmDescLimit+1} characters long");
            foreach (var transition in fsm.Transitions)
                if (transition.Text.Length > dbContext.FsmTextLimit)
                    throw new InvalidOperationException($"Transition text must be below {dbContext.FsmDescLimit+1} characters long");
        }
    }
}
