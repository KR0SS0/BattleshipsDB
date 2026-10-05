using Microsoft.JSInterop;

namespace Battleship.Web;

// C#'s door to wwwroot/js/sound.js. Sound is a nice extra, so if anything
// goes wrong on the JavaScript side the game simply stays silent.
public sealed class SoundService(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    public async Task PlayAsync(GameSound sound)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("play", sound.ToString().ToLowerInvariant());
        }
        catch (JSException)
        {
        }
    }

    // Loads the JavaScript module once, the first time it's needed
    private async Task<IJSObjectReference> GetModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/sound.js");

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
