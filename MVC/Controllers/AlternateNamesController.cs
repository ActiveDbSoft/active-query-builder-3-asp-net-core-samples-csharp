using System;
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
    public class AlternateNamesController : Controller
    {
        private string instanceId = "AlternateNames";

        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IQueryBuilderService _aqbs;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public AlternateNamesController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
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

        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);

            queryBuilder.SyntaxProvider = new DB2SyntaxProvider();

            // Turn displaying of alternate names on in the text of result SQL query
            queryBuilder.SQLFormattingOptions.UseAltNames = true;
            
            queryBuilder.SQLQuery.SQLUpdated += OnSQLUpdated;

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
            return @"Select ""Employees"".""Employee ID"", ""Employees"".""First Name"", ""Employees"".""Last Name"", ""Employee Photos"".""Photo Image"", ""Employee Resumes"".Resume From ""Employee Photos"" Inner Join
			""Employees"" On ""Employee Photos"".""Employee ID"" = ""Employees"".""Employee ID"" Inner Join
			""Employee Resumes"" On ""Employee Resumes"".""Employee ID"" = ""Employees"".""Employee ID""";
        }

        public void OnSQLUpdated(object sender, EventArgs e)
        {
            var qb = _aqbs.Get(instanceId);

            var opts = new SQLFormattingOptions();

            opts.Assign(qb.SQLFormattingOptions);
            opts.KeywordFormat = KeywordFormat.UpperCase;

            // get SQL query with real object names
            opts.UseAltNames = false;
            var plainSql = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts);

            // get SQL query with alternate names
            opts.UseAltNames = true;
            var sqlWithAltNames = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts);

            // prepare additional data to be sent to the client
            qb.ExchangeData = new
            {
                SQL = plainSql,
                AlternateSQL = sqlWithAltNames
            };
        }
    }
}