using Laqeeta.Api.Auth;
using Laqeeta.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web;

/// <summary>Base class of every MVC (Razor) controller.</summary>
[AutoValidateAntiforgeryToken]
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class AppController : Controller
{
    protected long UserId => User.GetUserId();
    protected long StationId => User.GetRequiredStationId();
    protected ILoc L => HttpContext.RequestServices.GetRequiredService<ILoc>();

    protected void Success(string message) => TempData["Success"] = message;
    protected void Error(string message) => TempData["Error"] = message;

    protected static bool IsDomain(Exception ex) => ex is DomainException;

    /// <summary>User-facing text of a business exception in the current language.</summary>
    protected string Msg(Exception ex) => ex is DomainException d ? L.Error(d) : ex.Message;

    /// <summary>Runs a service call; returns null on success or the business error message.</summary>
    protected async Task<string?> RunAsync(Func<Task> action)
    {
        try { await action(); return null; }
        catch (DomainException ex) { return L.Error(ex); }
    }

    /// <summary>Runs a service call and stores a flash message (success or error).</summary>
    protected async Task<bool> FlashAsync(Func<Task> action, string successMessage)
    {
        var error = await RunAsync(action);
        if (error is null) Success(successMessage); else Error(error);
        return error is null;
    }
}
