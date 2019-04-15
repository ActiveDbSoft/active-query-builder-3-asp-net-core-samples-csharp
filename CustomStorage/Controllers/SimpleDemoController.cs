using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreCustomStorage.Controllers
{
    public class SimpleDemoController : Controller
    {
        private readonly IQueryBuilderService _aqbs;

        // Use IActiveQueryBuilderServiceBase to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public SimpleDemoController(IQueryBuilderService aqbs)
        {
            _aqbs = aqbs;
        }

        public ActionResult Index()
        {
            // We've redefined the QueryBuilderStore.Provider object to be of QueryBuilderSqliteStoreProvider class in the Global.asax.cs file.
            // The implementation of Get method in this provider gets _OR_creates_new_ QueryBuilder object.
            // The initialization of the QueryBuilder object is also internally made by the QueryBuilderSqliteStoreProvider.
            var qb = _aqbs.Get();
            return View(qb);
        }
    }
}