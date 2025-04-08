using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


public class AdminIndexFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.Keys.Contains("id") || session.GetString("role") == "0" || session.GetString("role") == "3")
        {
            
            context.Result = new RedirectToActionResult("Login", "User", null);
        }

        base.OnActionExecuting(context);
    }
}
