using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PharmacistFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetString("role") == "1")
        {
            return;
        }
        if (!session.Keys.Contains("staff_role") || session.GetString("staff_role") != "Pharmacist")
        {
            context.Result = new RedirectToActionResult("Login", "User", null);
        }

        base.OnActionExecuting(context);
    }
}
