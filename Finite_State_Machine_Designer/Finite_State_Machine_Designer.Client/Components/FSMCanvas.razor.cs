using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace Finite_State_Machine_Designer.Client.Components
{
	public partial class FSMCanvas
	{
		/// <summary>
		/// Creates a new state or make an existing state final
		/// </summary>
		/// <param name="existingState">A possible existing state</param>
		/// <returns>
		///  <see langword="true"/> for when the <paramref name="existingState"/> exists,
		///  otherwise <see langword="false"/>.
		/// </returns>
		private async Task<bool> DrawStateOrFinal(FiniteState? existingState)
		{
			CanvasCoordinate? createdCoords = null;
			if (existingState == null)
			{
				createdCoords = await _fsmDrawer.CreateStateAsync(
					_lastMousePos,
					_defalutStateRadius
				);
				if (!(createdCoords is not null))
				{
					_logger.LogError("State to be created at {Coordinate} failed to be created", createdCoords);
					return false;
				}
				return true;
			}
			else if (existingState == _fsmDrawer.SelectedState)
			{
				_fsmDrawer.SelectedState.IsFinalState = !_fsmDrawer.SelectedState.IsFinalState;
				await _fsmDrawer.DrawMachineAsync(true);
				return true;
			}
			return false;
		}

		private void StateKeyHandler(KeyboardEventArgs keyboardEventArgs, FiniteState state)
		{
			if (CheckJsModule(JsModule))
			{
				switch (keyboardEventArgs.Key.ToLower())
				{
					case "backspace":
						if (state.Text.Length > 0)
						{
							var text = state.Text;
							state.Text = text.Substring(0, text.Length - 1);
							_caretVisible = true;
						}
						break;
					case "delete":
						if (_fsmDrawer.FSM.RemoveState(state))
						{
							List<StateTransition> connectedTransitions = _fsmDrawer.FSM.FindTransitions(state);
							foreach (StateTransition transition in connectedTransitions)
								_fsmDrawer.FSM.Transitions.Remove(transition);
						}
						break;
					case "return":
					case "enter":
					case "↵":
						state.Text += '\n';
						_caretVisible = true;
						break;
					case "spacebar":
					case " ":
						state.Text += ' ';
						_caretVisible = true;
						break;
					default:
						if (UpdateTextStyle(keyboardEventArgs))
							break;

						if (keyboardEventArgs.Key.Length == 1)
							state.Text = AddText(state.Text, keyboardEventArgs.Key);
						break;
				}
			}
		}

		private void TransitionKeyHandler(KeyboardEventArgs keyboardEventArgs, StateTransition transition)
		{
			if (CheckJsModule(JsModule))
			{
				switch (keyboardEventArgs.Key.ToLower())
				{
					case "backspace":
						if (transition.Text.Length > 0)
						{
							var text = transition.Text;
							transition.Text = text.Substring(0, text.Length - 1);

							_caretVisible = true;
						}
						break;
					case "delete":
						_fsmDrawer.FSM.RemoveTransition(transition);
						break;
					case "return":
					case "enter":
					case "↵":
						transition.Text += '\n';
						_caretVisible = true;
						break;
					case "spacebar":
					case " ":
						transition.Text += ' ';
						_caretVisible = true;
						break;
					default:
						if (UpdateTextStyle(keyboardEventArgs))
							break;

						if (keyboardEventArgs.Key.Length == 1)
							transition.Text = AddText(transition.Text, keyboardEventArgs.Key);
						break;
				}
			}
		}

		private bool UpdateTextStyle(KeyboardEventArgs keyboardEventArgs)
		{
			bool updatedStyle = false;
			if (keyboardEventArgs.CtrlKey)
			{
				if (keyboardEventArgs.Key == ".")
				{
					_subScriptMode = !_subScriptMode;
					_superScriptMode = false;
					updatedStyle = true;
				}
				else if (keyboardEventArgs.Key == ",")
				{
					_superScriptMode = !_superScriptMode;
					_subScriptMode = false;
					updatedStyle = true;
				}
			}
			return updatedStyle;
		}

		/// <summary>
		/// Makes the <paramref name="state"/> resizable
		/// </summary>
		/// <param name="state">A finite state</param>
		private void ToggleResizeState(FiniteState? state)
		{
			if (state is null)
				return;

			var coords = state.Coordinate;
			float radius = (float)Math.Sqrt(
				Math.Pow(_lastMousePos.X - coords.X, 2)
				+ Math.Pow(_lastMousePos.Y - coords.Y, 2)
			);

			_logger.LogDebug("Radius difference: {Difference}", Math.Abs(radius - state.Radius));
			if (Math.Abs(radius - state.Radius) <= 10)
				_canResizeState = true;
			else
				_canResizeState = false;
		}

		private async void DrawMachineTimer(object? obj)
		{
			await _fsmDrawer.DrawMachineAsync(_caretVisible);
			if (JsModule is not null)
				await JsModule.InvokeVoidAsync("saveFSM", _fsmDrawer.FSM);
			_logger.LogDebug("State machine is redrawn at {Time} with caret {Visibilty}.",
			DateTimeOffset.Now, _caretVisible ? "visible" : "not visible");
			_caretVisible = !_caretVisible;
			_lastDrawTimerCall = DateTime.Now;
		}

		private string AddText(string text, string newText)
		{
			// For superscript numbers ⁰ and ⁷, numbers 1-3 are in U+00B9, U+00B2, U+00B3 respectively
			// However subscript numbers are within U+208x where 0 < x < 9
			// Instead of typing '_0' to get '₀', just press 'ctrl ,' for superscript mode and 'ctrl .' for subscript mode
			int utfSubSuperRange = 0x2080;
			if (_superScriptMode)
				utfSubSuperRange = 0x2070;
			if (_superScriptMode || _subScriptMode)
			{
				char[] characters = newText.ToCharArray();
				for (int i = 0; i < characters.Length; i++)
				{
					if (char.IsDigit(characters[i]))
					{
						int newUtfCode = 0;
						int keyNum = int.Parse(newText[i].ToString());
						if (_superScriptMode && (characters[i] == '2' || characters[i] == '3'))
							newUtfCode = 0x00B0 + keyNum;
						else if (_superScriptMode && characters[i] == '1')
							newUtfCode = 0x00B9;
						else
							newUtfCode = utfSubSuperRange + keyNum;
						characters[i] = char.ConvertFromUtf32(newUtfCode)[0];
					}
				}
				newText = string.Concat(characters);
			}

			newText = text + newText;
			for (int i = 0; i < _greekAlphabet.Length; i++)
			{
				Regex greekRegex = new(@$"\\{_greekAlphabet[i]}", RegexOptions.IgnoreCase);
				int offsetAfterRho = Convert.ToInt32(i > 16);
				newText = newText.Replace(@$"\{_greekAlphabet[i]}", char.ConvertFromUtf32(0x003b1 + i + offsetAfterRho));
				newText = greekRegex.Replace(newText, char.ConvertFromUtf32(0x0391 + i + offsetAfterRho));
			}

			/*
			* Unicode for empty set (∅): U+2205
			* Unicdoe for rightarrow (→): U+2192
			* Unicode for leftarrow (←): U+2190
			*/
			newText = newText.Replace(@"\emptyset", char.ConvertFromUtf32(0x2205));
			newText = newText.Replace(@"\rightarrow", char.ConvertFromUtf32(0x2192));
			newText = newText.Replace(@"\leftarrow", char.ConvertFromUtf32(0x2190));

			_caretVisible = true;
			return newText;
		}

		/// <summary>
		/// Loads the Finite State Machine from local storage.
		/// </summary>
		/// <returns>Finite state machine</returns>
		private async Task<IFiniteStateMachine?> LoadFSM()
		{
			IFiniteStateMachine? fsm = null;
			if (JsModule is not null)
			{
				fsm = await JsModule.InvokeAsync<FiniteStateMachine>("loadFSM");
				foreach (var transition in fsm.Transitions)
					foreach (var state in fsm.States)
					{
						if (state == transition.FromState)
							transition.FromState = state;
						if (state == transition.ToState)
							transition.ToState = state;
					}

			}
			return fsm;
		}
	}
}
