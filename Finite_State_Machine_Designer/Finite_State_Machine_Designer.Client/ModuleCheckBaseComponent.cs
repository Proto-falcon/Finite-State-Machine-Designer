using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace Finite_State_Machine_Designer.Client
{
	public class ModuleCheckBaseComponent: ComponentBase
	{
		protected static bool CheckJsModule([NotNullWhen(true)] IJSObjectReference? module)
		{
			if (module == null)
				return false;
			return true;
		}

	}
}
