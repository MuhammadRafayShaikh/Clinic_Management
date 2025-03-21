﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AdminFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.Keys.Contains("id"))
        {
            context.Result = new RedirectToActionResult("Login", "User", null);
        }
        else if (session.GetString("role") == "0" || session.Keys.Contains("staff_role"))
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }

        base.OnActionExecuting(context);
    }
}
