using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class QueryAnalysisController : Controller
    {
        private string instanceId = "QueryAnalysis";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryAnalysisController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.GetOrCreate(instanceId, InitializeQueryBuilder);

            return View(qb);
        }

        private void InitializeQueryBuilder(QueryBuilder queryBuilder)
        {
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();
            
            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            queryBuilder.SQLQuery.SQLUpdated += OnSQLUpdated;

            queryBuilder.SQL = GetDefaultSql();
        }

        private string GetDefaultSql()
        {
            return @"Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity
                From Orders o Inner Join
                  Customers c On o.Customer_ID = c.ID Inner Join
                  Shippers s On s.ID = o.Shipper_ID
                Where o.ShipCity = 'A'";
        }

        public void OnSQLUpdated(object sender, EventArgs e)
        {
            var queryBuilder = _aqbs.Get(instanceId);

            var data = new ExchangeClass();

            data.Statistics = GetQueryStatistic(queryBuilder.QueryStatistics);
            data.SubQueries = DumpSubQueries(queryBuilder);
            data.QueryStructure = DumpQueryStructureInfo(queryBuilder.ActiveSubQuery);
            data.UnionSubQuery = new UnionSubQueryExchangeClass();

            data.UnionSubQuery.SelectedExpressions = DumpSelectedExpressionsInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery);
            data.UnionSubQuery.DataSources = DumpDataSourcesInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery);
            ;
            data.UnionSubQuery.Links = DumpLinksInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery);
            data.UnionSubQuery.Where = GetWhereInfo(queryBuilder.ActiveUnionSubQuery);
            
            queryBuilder.ExchangeData = data;
        }

        internal class UnionSubQueryExchangeClass
        {
            public string SelectedExpressions;
            public string DataSources;
            public string Links;
            public string Where;
        }

        private class ExchangeClass
        {
            public string Statistics;
            public string SubQueries;
            public string QueryStructure;
            public UnionSubQueryExchangeClass UnionSubQuery;
        }


        private string GetQueryStatistic(QueryStatistics qs)
        {
            string stats = "";

            stats = "<b>Used Objects (" + qs.UsedDatabaseObjects.Count + "):</b><br/>";
            for (int i = 0; i < qs.UsedDatabaseObjects.Count; i++)
            {
                stats += "<br />" + qs.UsedDatabaseObjects[i].ObjectName.QualifiedName;
            }

            stats += "<br /><br />" + "<b>Used Columns (" + qs.UsedDatabaseObjectFields.Count + "):</b><br />";
            for (int i = 0; i < qs.UsedDatabaseObjectFields.Count; i++)
            {
                stats += "<br />" + qs.UsedDatabaseObjectFields[i].FullName.QualifiedName;
            }

            stats += "<br /><br />" + "<b>Output Expressions (" + qs.OutputColumns.Count + "):</b><br />";
            for (int i = 0; i < qs.OutputColumns.Count; i++)
            {
                stats += "<br />" + qs.OutputColumns[i].Expression;
            }
            return stats;
        }

        private string DumpQueryStructureInfo(SubQuery subQuery)
        {
            var stringBuilder = new StringBuilder();
            DumpUnionGroupInfo(stringBuilder, "", subQuery);
            return stringBuilder.ToString();
        }

        private void DumpUnionGroupInfo(StringBuilder stringBuilder, string indent, UnionGroup unionGroup)
        {
            QueryBase[] children = GetUnionChildren(unionGroup);

            foreach (QueryBase child in children)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine("<br />");
                }

                if (child is UnionSubQuery)
                {
                    // UnionSubQuery is a leaf node of query structure.
                    // It represent a single SELECT statement in the tree of unions
                    DumpUnionSubQueryInfo(stringBuilder, indent, (UnionSubQuery)child);
                }
                else
                {
                    // UnionGroup is a tree node.
                    // It contains one or more leafs of other tree nodes.
                    // It represent a root of the subquery of the union tree or a
                    // parentheses in the union tree.
                    unionGroup = (UnionGroup)child;

                    stringBuilder.AppendLine(indent + unionGroup.UnionOperatorFull + "group: [");
                    DumpUnionGroupInfo(stringBuilder, indent + "&nbsp;&nbsp;&nbsp;&nbsp;", unionGroup);
                    stringBuilder.AppendLine(indent + "]<br />");
                }
            }
        }

        private void DumpUnionSubQueryInfo(StringBuilder stringBuilder, string indent, UnionSubQuery unionSubQuery)
        {
            string sql = unionSubQuery.GetResultSQL();

            stringBuilder.AppendLine(indent + unionSubQuery.UnionOperatorFull + ": " + sql + "<br />");
        }

        private QueryBase[] GetUnionChildren(UnionGroup unionGroup)
        {
            ArrayList result = new ArrayList();

            for (int i = 0; i < unionGroup.Count; i++)
            {
                result.Add(unionGroup[i]);
            }

            return (QueryBase[])result.ToArray(typeof(QueryBase));
        }

        public string DumpSelectedExpressionsInfoFromUnionSubQuery(UnionSubQuery unionSubQuery)
        {
            var stringBuilder = new StringBuilder();
            // get list of CriteriaItems
            QueryColumnList criteriaList = unionSubQuery.QueryColumnList;

            // dump all items
            for (int i = 0; i < criteriaList.Count; i++)
            {
                QueryColumnListItem criteriaItem = criteriaList[i];

                // only items have .Select property set to True goes to SELECT list
                if (!criteriaItem.Selected)
                    continue;

                // separator
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine("<br />");
                }

                DumpSelectedExpressionInfo(stringBuilder, criteriaItem);
            }

            return stringBuilder.ToString();
        }

        private void DumpSelectedExpressionInfo(StringBuilder stringBuilder, QueryColumnListItem selectedExpression)
        {
            // write full sql fragment of selected expression
            stringBuilder.AppendLine(selectedExpression.ExpressionString + "<br />");

            // write alias
            if (!String.IsNullOrEmpty(selectedExpression.AliasString))
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;alias: " + selectedExpression.AliasString + "<br />");
            }

            // write datasource reference (if any)
            if (selectedExpression.ExpressionDatasource != null)
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;datasource: " + selectedExpression.ExpressionDatasource.GetResultSQL() + "<br />");
            }

            // write metadata information (if any)
            if (selectedExpression.ExpressionField != null)
            {
                MetadataField field = selectedExpression.ExpressionField;
                stringBuilder.AppendLine("&nbsp;&nbsp;field name: " + field.Name + "<br />");

                string s = Enum.GetName(typeof(DbType), field.FieldType);
                stringBuilder.AppendLine("&nbsp;&nbsp;field type: " + s + "<br />");
            }
        }

        private void DumpDataSourcesInfo(StringBuilder stringBuilder, IList<DataSource> dataSources)
        {
            for (int i = 0; i < dataSources.Count; i++)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine("<br />");
                }

                DumpDataSourceInfo(stringBuilder, dataSources[i]);
            }
        }

        public string DumpDataSourcesInfoFromUnionSubQuery(UnionSubQuery unionSubQuery)
        {
            StringBuilder stringBuilder = new StringBuilder();
            DumpDataSourcesInfo(stringBuilder, GetDataSourceList(unionSubQuery));
            return stringBuilder.ToString();
        }

        private List<DataSource> GetDataSourceList(UnionSubQuery unionSubQuery)
        {
            List<DataSource> list = new List<DataSource>();

            unionSubQuery.FromClause.GetDatasourceByClass<DataSource>(list);

            return list;
        }

        private void DumpDataSourceInfo(StringBuilder stringBuilder, DataSource dataSource)
        {
            // write full sql fragment
            stringBuilder.AppendLine("<b>" + dataSource.GetResultSQL() + "</b><br />");

            // write alias
            stringBuilder.AppendLine("&nbsp;&nbsp;alias: " + dataSource.Alias + "<br />");

            // write referenced MetadataObject (if any)
            if (dataSource.MetadataObject != null)
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;ref: " + dataSource.MetadataObject.Name + "<br />");
            }

            // write subquery (if datasource is actually a derived table)
            if (dataSource is DataSourceQuery)
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;subquery sql: " + ((DataSourceQuery)dataSource).SubQuery.GetResultSQL() + "<br />");
            }

            // write fields
            string fields = String.Empty;

            for (int i = 0; i < dataSource.Metadata.Count; i++)
            {
                if (fields.Length > 0)
                {
                    fields += ", ";
                }

                fields += dataSource.Metadata[i].Name;
            }

            stringBuilder.AppendLine("&nbsp;&nbsp;fields (" + dataSource.Metadata.Count.ToString() + "): " + fields + "<br />");
        }

        private void DumpLinkInfo(StringBuilder stringBuilder, Link link)
        {
            // write full sql fragment of link expression
            stringBuilder.AppendLine(link.LinkExpression.GetSQL(link.SQLContext.SQLGenerationOptionsForServer) + "<br />");

            // write information about left side of link
            stringBuilder.AppendLine("&nbsp;&nbsp;left datasource: " + link.LeftDataSource.GetResultSQL() + "<br />");

            if (link.LeftType == LinkSideType.Inner)
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;left type: Inner" + "<br />");
            }
            else
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;left type: Outer" + "<br />");
            }

            // write information about right side of link
            stringBuilder.AppendLine("&nbsp;&nbsp;right datasource: " + link.RightDataSource.GetResultSQL() + "<br />");

            if (link.RightType == LinkSideType.Inner)
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;lerightft type: Inner" + "<br />");
            }
            else
            {
                stringBuilder.AppendLine("&nbsp;&nbsp;right type: Outer" + "<br />");
            }
        }

        private void DumpLinksInfo(StringBuilder stringBuilder, IList<Link> links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine("<br />");
                }

                DumpLinkInfo(stringBuilder, (Link)links[i]);
            }
        }

        private IList<Link> GetLinkList(UnionSubQuery unionSubQuery)
        {
            List<Link> links = new List<Link>();

            unionSubQuery.FromClause.GetLinksRecursive(links);

            return links;
        }

        public string DumpLinksInfoFromUnionSubQuery(UnionSubQuery unionSubQuery)
        {
            var stringBuilder = new StringBuilder();
            DumpLinksInfo(stringBuilder, GetLinkList(unionSubQuery));
            return stringBuilder.ToString();
        }

        public void DumpWhereInfo(StringBuilder stringBuilder, SQLExpressionItem where)
        {
            DumpExpression(stringBuilder, "", where);
        }

        private void DumpExpression(StringBuilder stringBuilder, string indent, SQLExpressionItem expression)
        {
            const string cIndentInc = "&nbsp;&nbsp;&nbsp;&nbsp;";

            string newIndent = indent + cIndentInc;

            if (expression == null) // NULL reference protection
            {
                stringBuilder.AppendLine(indent + "--nil--" + "<br />");
            }
            else if (expression is SQLExpressionBrackets)
            {
                // Expression is actually the brackets query structure node.
                // Create the "brackets" tree node and load content of
                // the brackets as children of the node.
                stringBuilder.AppendLine(indent + "()" + "<br />");
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionBrackets)expression).LExpression);
            }
            else if (expression is SQLExpressionOr)
            {
                // Expression is actually the "OR" query structure node.
                // Create the "OR" tree node and load all items of
                // the "OR" collection as children of the tree node.
                stringBuilder.AppendLine(indent + "OR" + "<br />");

                for (int i = 0; i < ((SQLExpressionOr)expression).Count; i++)
                {
                    DumpExpression(stringBuilder, newIndent, ((SQLExpressionOr)expression)[i]);
                }
            }
            else if (expression is SQLExpressionAnd)
            {
                // Expression is actually the "AND" query structure node.
                // Create the "AND" tree node and load all items of
                // the "AND" collection as children of the tree node.
                stringBuilder.AppendLine(indent + "AND" + "<br />");

                for (int i = 0; i < ((SQLExpressionAnd)expression).Count; i++)
                {
                    DumpExpression(stringBuilder, newIndent, ((SQLExpressionAnd)expression)[i]);
                }
            }
            else if (expression is SQLExpressionNot)
            {
                // Expression is actually the "NOT" query structure node.
                // Create the "NOT" tree node and load content of
                // the "NOT" operator as children of the tree node.
                stringBuilder.AppendLine(indent + "NOT" + "<br />");
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionNot)expression).LExpression);
            }
            else if (expression is SQLExpressionOperatorBinary)
            {
                // Expression is actually the "BINARY OPERATOR" query structure node.
                // Create a tree node containing the operator value and
                // two leaf nodes with the operator arguments.
                string s = ((SQLExpressionOperatorBinary)expression).OperatorObj.OperatorName;
                stringBuilder.AppendLine(indent + s + "<br />");
                // left argument of the binary operator
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionOperatorBinary)expression).LExpression);
                // right argument of the binary operator
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionOperatorBinary)expression).RExpression);
            }
            else
            {
                // other type of AST nodes - out as a text
                string s = expression.GetSQL(expression.SQLContext.SQLGenerationOptionsForServer);
                stringBuilder.AppendLine(indent + s + "<br />");
            }
        }

        private string GetWhereInfo(UnionSubQuery unionSubQuery)
        {
            StringBuilder stringBuilder = new StringBuilder();

            SQLSubQuerySelectExpression unionSubQueryAst = unionSubQuery.ResultQueryAST;

            try
            {
                if (unionSubQueryAst.Where != null)
                {
                    DumpWhereInfo(stringBuilder, unionSubQueryAst.Where);
                }
            }
            finally
            {
                unionSubQueryAst.Dispose();
            }

            return stringBuilder.ToString();
        }

        public string DumpSubQueries(QueryBuilder queryBuilder)
        {
            StringBuilder stringBuilder = new StringBuilder();
            DumpSubQueriesInfo(stringBuilder, queryBuilder);
            return stringBuilder.ToString();
        }

        private void DumpSubQueryInfo(StringBuilder stringBuilder, int index, SubQuery subQuery)
        {
            string sql = subQuery.GetResultSQL();

            stringBuilder.AppendLine(index + ": " + sql + "<br />");
        }

        public void DumpSubQueriesInfo(StringBuilder stringBuilder, QueryBuilder queryBuilder)
        {
            for (int i = 0; i < queryBuilder.SQLQuery.QueryRoot.SubQueries.Count; i++)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine("<br />");
                }

                DumpSubQueryInfo(stringBuilder, i, queryBuilder.SQLQuery.QueryRoot.SubQueries[i]);
            }
        }
    }
}