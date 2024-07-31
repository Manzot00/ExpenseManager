using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExpenseManagement.Filters
{
    // Controlla se l'utente è autorizzato a visualizzare la pagina tramite l'accessToken e il ruolo
    public class CustomeAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var accessToken = context.HttpContext.Session.GetString("AccessToken");
            var Role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(accessToken))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
            }
            else if (context.RouteData.Values["controller"].ToString() == "Admin" &&
                     context.RouteData.Values["action"].ToString() == "Index" &&
                     Role == "User")
            {
                context.Result = new RedirectToActionResult("Index", "Dashboard", null);
            }
            base.OnActionExecuting(context);
        }
    }
}