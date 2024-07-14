using Finite_State_Machine_Designer.Client.FSM;
using Finite_State_Machine_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Components.Pages
{
    public partial class MyFSMs
    {
        /// <summary>
        /// Gets the user and their basic Finite State Machines' information.
        /// </summary>
        private async Task InitialiseUserAsync()
        {
            await using ApplicationDbContext dbContext =
                await DbFactory.CreateDbContextAsync();
            string? userName = userService.GetUser().Identity?.Name;
            if (userName is null)
            {
                Navigation.NavigateTo("Account/Login");
                return;
            }
            ApplicationUser? fetchedUser = dbContext.Users
                .Where(user => user.NormalizedUserName == userName.ToUpper())
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefault();
            if (fetchedUser is not null)
            {
                _user = fetchedUser;
                _user.StateMachines = await dbContext.Entry(_user)
                    .Collection(user => user.StateMachines)
                    .Query()
                    .OrderByDescending(fsm => fsm.TimeUpdated)
                    .Take(_availableFsmsLimit)
                    .AsNoTrackingWithIdentityResolution()
                    .ToListAsync();
                if (_user.StateMachines.Count > 0)
                    _lastRecentModifiedTime = _user.StateMachines
                        .Last().TimeUpdated;
                if (_user.StateMachines.Count < _availableFsmsLimit)
                    _loadMoreFsms = false;
            }
            else
                Navigation.NavigateTo("Account/Login");
            _finishLoading = true;
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
        private async Task SaveCurrentFSMAsync()
        {
            if (CheckJsModule(JsModule))
            {
                SaveCurrent = false;
                _currentlySaving = true;
                _currentSaved = false;
                _errorMsg = "";
                StateHasChanged();
                if (_currentDrawnFsm is not null && _user is not null)
                {
                    if (string.IsNullOrWhiteSpace(_currentDrawnFsm.Name))
                    {
                        _errorMsg = "Please enter a name.";
                    }
                    else
                        try
                        {
                            await using (ApplicationDbContext dbContext =
                                await DbFactory.CreateDbContextAsync())
                            {
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
                                        await DBCommnds.UpdateFsmAsync(
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
                                        await DBCommnds.AddFSMAsync(
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
                            _currentSaved = true;
                            await JsModule.InvokeVoidAsync(
                                "fSMCanvasUtils.saveFSM", _currentDrawnFsm);
                            _logger.LogInformation(
                                "Successfully saved the current "
                                + "FSM '{FSM}' from user '{user}'",
                                _currentDrawnFsm.Name, _user.Id);
                        }
                        catch (Exception ex)
                        {
                            _errorMsg = "Couldn't save!! Something went wrong.";
                            _logger.LogError(
                                "Couldn't save the current FSM "
                                + "'{FsmName}' from user '{user}'",
                            _currentDrawnFsm.Name, _user.Id);
                            _logger.LogError("{ERROR}", ex.ToString());
                        }
                }
                _currentlySaving = false;
                StateHasChanged();
            }
        }
    }
}
