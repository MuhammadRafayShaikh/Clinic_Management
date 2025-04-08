using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthenticationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.Keys.Contains("id"))
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["alert"] = "Please Login First";
            }

            context.Result = new RedirectToActionResult("Login", "User", new { returnUrl = context.HttpContext.Request.Path });
        }

        base.OnActionExecuting(context);
    }
}
