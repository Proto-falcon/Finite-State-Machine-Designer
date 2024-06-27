using Finite_State_Machine_Designer.Client.FSM;
using Finite_State_Machine_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.JSInterop;

namespace Finite_State_Machine_Designer.Components.Pages
{
    public partial class MyFSMs
    {
        private async ValueTask GetFullFsm(
            FiniteStateMachine fsm, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
                return;
            await using DbContext dbContext =
                await DbFactory.CreateDbContextAsync(cancelToken);
            EntityEntry<FiniteStateMachine> fsmEntry = dbContext.Attach(fsm);
            await fsmEntry
                .Collection(stateMachine => stateMachine.States)
                .LoadAsync(cancelToken);
            await fsmEntry
                .Collection(stateMachine => stateMachine.Transitions)
                .LoadAsync(cancelToken);

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
        }

        /// <summary>
        /// Generates an SVG of the Finite State Machine
        /// </summary>
        /// <param name="fsm">Finite State Machine</para>
        /// <param name="scale">Scale the FSM from original canvas dimensions</para>
        /// <returns>
        /// <see cref="MarkupString"/> that contains the URL
        /// of the SVG of Finite State Machine
        /// </returns>
        private async Task<string> GenerateFsmSvg(
            FiniteStateMachine fsm, double scale = 1)
        {
            string fsmSvg = string.Empty;
            if (CheckJsModule(JsModule))
            {
                fsmSvg = await JsModule.InvokeAsync<string>("fSMExport.fsmToSVG",
                    fsm, fsm.Width, fsm.Height, _colour,
                    _canvasBackgroundColour, 2, scale, true);
            }
            return fsmSvg;
        }

        /// <summary>
        /// Saves the current FSM in use to database.
        /// </summary>
        private async Task SaveCurrentFSM()
        {
            if (CheckJsModule(JsModule))
            {
                SaveCurrent = false;
                _currentlySaving = true;
                _currentSaved = false;
                _currentSavedFailed = false;
                StateHasChanged();
                if (_currentFSM is not null)
                {
                    if (string.IsNullOrWhiteSpace(_currentFSM.Name))
                    {
                        _errorSaveMsg = "Please enter a name.";
                        _currentSavedFailed = true;
                    }
                    else
                    {
                        _errorSaveMsg = "Couldn't save!! Something went wrong.";

                        try
                        {
                            await using (ApplicationDbContext dbContext =
                                await DbFactory.CreateDbContextAsync())
                            {
                                FiniteStateMachine? existingFSM =
                                    await dbContext.Entry(_user)
                                    .Collection(user => user.StateMachines)
                                    .Query()
                                    .Where(fsm => fsm.Name == _currentFSM.Name)
                                    .FirstOrDefaultAsync();
                                if (existingFSM is not null)
                                {
                                    await using var transact =
                                        await dbContext.Database.BeginTransactionAsync();
                                    try
                                    {
                                        _currentFSM.Id = existingFSM.Id;
                                        await DBCommnds.UpdateFsm(
                                            dbContext,
                                            _currentFSM,
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
                                        await DBCommnds.AddFSM(
                                            dbContext,
                                            _currentFSM,
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
                                "fSMCanvasUtils.saveFSM", _currentFSM);
                            _logger.LogInformation(
                                "Successfully saved the current "
                                + "FSM '{FSM}' from user '{user}'",
                                _currentFSM.Name, _user.Id);
                        }
                        catch (Exception ex)
                        {
                            _currentSavedFailed = true;
                            _logger.LogError(
                                "Couldn't save the current FSM "
                                + "'{FsmName}' from user '{user}'",
                            _currentFSM.Name, _user.Id);
                            _logger.LogError("{ERROR}", ex.ToString());
                        }
                    }
                }
                _currentlySaving = false;
                StateHasChanged();
            }
        }
    }
}
