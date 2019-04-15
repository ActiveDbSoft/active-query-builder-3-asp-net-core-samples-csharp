using System.Configuration;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class ToggleUseAltNamesController : Controller
    {
        private string instanceId = "ToggleUseAltNames";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public ToggleUseAltNamesController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            if (qb == null)
                qb = CreateQueryBuilder();

            return View(qb);
        }

        public ActionResult Toggle()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            qb.SQLFormattingOptions.UseAltNames = !qb.SQLFormattingOptions.UseAltNames;

            // Reload metadata structure to refill it with real or alternate names.
            // Note: reloading the structure does not reload the metadata container. 
            qb.MetadataStructure.Refresh();

            return new EmptyResult();
        }

        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new DB2SyntaxProvider();

            queryBuilder.SQLFormattingOptions.UseAltNames = false;
            
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["Db2XmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            //Set default query
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        private string GetDefaultSql()
        {
            return @"Select ""Employees"".""Employee ID"", ""Employees"".""First Name"", ""Employees"".""Last Name"", "+
                @"""Employee Photos"".""Photo Image"", ""Employee Resumes"".Resume From ""Employee Photos"" Inner Join "+
                @"""Employees"" On ""Employee Photos"".""Employee ID"" = ""Employees"".""Employee ID"" Inner Join " +
                @"""Employee Resumes"" On ""Employee Resumes"".""Employee ID"" = ""Employees"".""Employee ID""";
        }
    }
}