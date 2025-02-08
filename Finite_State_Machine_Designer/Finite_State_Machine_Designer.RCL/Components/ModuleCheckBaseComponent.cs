using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace Finite_State_Machine_Designer.RCL.Components
{
    public class ModuleCheckBaseComponent : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public IJSRuntime JS { get; set; }

        private IJSObjectReference? _jsModule;
        protected IJSObjectReference? JsModule { get => _jsModule; }

        protected static bool CheckJsModule([NotNullWhen(true)] IJSObjectReference? module)
        {
            if (module == null)
                return false;
            return true;
        }

        protected async Task SetJsModule(params object?[]? jsPath)
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", jsPath);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            GC.SuppressFinalize(this);
            if (_jsModule is not null)
            {
                await _jsModule.DisposeAsync();
            }
        }
    }
}
