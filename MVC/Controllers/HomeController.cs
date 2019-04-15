using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQueryBuilderService _aqbs;
        private readonly IQueryTransformerService _qts;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public HomeController(IQueryBuilderService aqbs, IQueryTransformerService qts)
        {
            _aqbs = aqbs;
            _qts = qts;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        //CUT:PRO{{
        public void DisposeStates()
        {
            _aqbs.Remove();
            _qts.Remove();
        }
        //}}CUT:PRO
    }
}
