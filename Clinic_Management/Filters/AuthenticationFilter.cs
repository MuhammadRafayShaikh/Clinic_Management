using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

public class AuthenticationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.Keys.Contains("id") && string.IsNullOrEmpty(context.HttpContext.Request.Cookies["id"]))
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["alert"] = "Please Login First";
            }

            context.Result = new RedirectToActionResult("Login", "User", new { returnUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(context.HttpContext.Request.Path)) });
        }

        base.OnActionExecuting(context);
    }
}
