using System;
using System.Web;
using System.Web.Mvc;
using TicketDesk.Domain;
using System.Linq;
using TicketDesk.Localization;
using System.Threading.Tasks;

namespace TicketDesk.Web.Client.Controllers
{
    [RoutePrefix("")]
    [Route("{action=index}")]
    public class HomeController : Controller
    {
        private TdDomainContext Context { get; set; }
        public HomeController(TdDomainContext context)
        {
            Context = context;
        }

        [Route("language")]
        public ActionResult SetLanguage(string name)
        {
            CultureHelper.SetCurrentCulture(name);

            var cookie = new HttpCookie("_culture", name);
            cookie.Expires = DateTime.Today.AddYears(1);
            Response.SetCookie(cookie);

            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            else
                return RedirectToAction("Index", "Home");
        }

        [Route("")]
        [Route("index")]
        public ActionResult Index()
        {
            if (ApplicationConfig.HomeEnabled)
            {
                var projects = Context.Projects.Select(s=>s.ProjectId).ToList();
                var isMulti = (projects.Count > 1);
                ViewBag.IsMultiProject = isMulti;
                return View();
            }
            else
            {
                return RedirectToActionPermanent("Index", "TicketCenter");
            }
        }        

        [Route("about")]
        public ActionResult About()
        {
            return View();
        }

        [Route("access-denied", Name = "AccessDenied")]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}