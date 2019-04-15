using System;
using System.Collections.Generic;
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
    public class QueryModificationDemoController : Controller
    {
        private string instanceId = "QueryModification";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IActiveQueryBuilderServiceBase to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryModificationDemoController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        private const string CustomersName = "Northwind.dbo.Customers";
        private const string OrdersName = "Northwind.dbo.Orders";
        private const string CustomersAlias = "c";
        private const string OrdersAlias = "o";
        private const string CustomersCompanyName = "CompanyName";
        private const string CusomerId = "CustomerId";
        private const string OrderDate = "OrderDate";

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
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();
            
            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            return queryBuilder;
        }

        private bool IsTablePresentInQuery(UnionSubQuery unionSubQuery, DataSource table)
        {
            // collect the list of datasources used in FROM
            var dataSources = unionSubQuery.GetChildrenRecursive<DataSource>(false);

            // check given table in list of all datasources
            return dataSources.IndexOf(table) != -1;
        }

        private bool IsQueryColumnListItemPresentInQuery(UnionSubQuery unionSubQuery, QueryColumnListItem item)
        {
            return unionSubQuery.QueryColumnList.IndexOf(item) != -1 && !String.IsNullOrEmpty(item.ExpressionString);
        }

        private void ClearConditionCells(UnionSubQuery unionSubQuery, QueryColumnListItem item)
        {
            for (int i = 0; i < unionSubQuery.QueryColumnList.GetMaxConditionCount(); i++)
            {
                item.ConditionStrings[i] = "";
            }
        }

        private DataSource AddTable(UnionSubQuery unionSubQuery, string name, string alias)
        {
            var queryBuilder1 = _aqbs.Get(instanceId);

            using (var parsedName = queryBuilder1.SQLContext.ParseQualifiedName(name))
            using (var parsedAlias = queryBuilder1.SQLContext.ParseIdentifier(alias))
            {
                return queryBuilder1.SQLQuery.AddObject(unionSubQuery, parsedName, parsedAlias);
            }
        }

        private DataSource FindTableInQueryByName(UnionSubQuery unionSubQuery, string name)
        {
            var queryBuilder1 = _aqbs.Get(instanceId);

            List<DataSourceObject> foundDatasources;
            using (var qualifiedName = queryBuilder1.SQLContext.ParseQualifiedName(name))
            {
                foundDatasources = new List<DataSourceObject>();
                unionSubQuery.FromClause.FindTablesByDbName(qualifiedName, foundDatasources);
            }

            // if found more than one tables with given name in the query, use the first one
            return foundDatasources.Count > 0 ? foundDatasources[0] : null;
        }

        private void AddWhereCondition(QueryColumnList columnList, QueryColumnListItem whereListItem, string condition)
        {
            bool whereFound = false;

            // GetMaxConditionCount: returns the number of non-empty criteria columns in the grid.
            for (int i = 0; i < columnList.GetMaxConditionCount(); i++)
            {
                // CollectCriteriaItemsWithWhereCondition:
                // This function returns the list of conditions that were found in
                // the i-th criteria column, applied to specific clause (WHERE or HAVING).
                // Thus, this function collects all conditions joined with AND
                // within one OR group (one grid column).
                List<QueryColumnListItem> foundColumnItems = new List<QueryColumnListItem>();
                CollectCriteriaItemsWithWhereCondition(columnList, i, foundColumnItems);

                // if found some conditions in i-th column, append condition to i-th column
                if (foundColumnItems.Count > 0)
                {
                    whereListItem.ConditionStrings[i] = condition;
                    whereFound = true;
                }
            }

            // if there are no cells with "where" conditions, add condition to new column
            if (!whereFound)
            {
                whereListItem.ConditionStrings[columnList.GetMaxConditionCount()] = condition;
            }
        }

        public void ApplyChanges(Model m)
        {
            var queryBuilder1 = _aqbs.Get(instanceId);

            DataSource customers = null;
            DataSource orders = null;
            QueryColumnListItem _companyName = null;
            QueryColumnListItem _orderDate = null;

            //prepare parsed names
            SQLQualifiedName joinFieldName = queryBuilder1.SQLContext.ParseQualifiedName(CusomerId);
            SQLQualifiedName companyNameFieldName = queryBuilder1.SQLContext.ParseQualifiedName(CustomersCompanyName);
            SQLQualifiedName orderDateFieldName = queryBuilder1.SQLContext.ParseQualifiedName(OrderDate);

            // get the active SELECT

            var usq = queryBuilder1.ActiveUnionSubQuery;

            #region actualize stored references (if query is modified in GUI)
            #region actualize datasource references
            // if user removed previously added datasources then clear their references
            if (customers != null && !IsTablePresentInQuery(usq, customers))
            {
                // user removed this table in GUI
                customers = null;
            }

            if (orders != null && !IsTablePresentInQuery(usq, orders))
            {
                // user removed this table in GUI
                orders = null;
            }
            #endregion

            // clear CompanyName conditions
            if (_companyName != null)
            {
                // if user removed entire row OR cleared expression cell in GUI, clear the stored reference
                if (!IsQueryColumnListItemPresentInQuery(usq, _companyName))
                    _companyName = null;
            }

            // clear all condition cells for CompanyName row
            if (_companyName != null)
            {
                ClearConditionCells(usq, _companyName);
            }

            // clear OrderDate conditions
            if (_orderDate != null)
            {
                // if user removed entire row OR cleared expression cell in GUI, clear the stored reference
                if (!IsQueryColumnListItemPresentInQuery(usq, _orderDate))
                    _orderDate = null;
            }

            // clear all condition cells for OrderDate row
            if (_orderDate != null)
            {
                ClearConditionCells(usq, _orderDate);
            }
            #endregion

            #region process Customers table
            if (m.Customers)
            {
                // if we have no previously added Customers table, try to find one already added by the user
                if (customers == null)
                {
                    customers = FindTableInQueryByName(usq, CustomersName);
                }

                // there is no Customers table in query, add it
                if (customers == null)
                {
                    customers = AddTable(usq, CustomersName, CustomersAlias);
                }

                #region process CompanyName condition
                if (m.CompanyName && !String.IsNullOrEmpty(m.CompanyNameText))
                {
                    // if we have no previously added grid row for this condition, add it
                    if (_companyName == null || _companyName.IsDisposing)
                    {
                        _companyName = usq.QueryColumnList.AddField(customers, companyNameFieldName.QualifiedName);
                        // do not append it to the select list, use this row for conditions only
                        _companyName.Selected = false;
                    }

                    // write condition from edit box to all needed grid cells
                    AddWhereCondition(usq.QueryColumnList, _companyName, m.CompanyNameText);
                }
                else
                {
                    // remove previously added grid row
                    if (_companyName != null)
                    {
                        _companyName.Dispose();
                    }

                    _companyName = null;
                }
                #endregion
            }
            else
            {
                // remove previously added datasource
                if (customers != null)
                {
                    customers.Dispose();
                }

                customers = null;
            }
            #endregion

            #region process Orders table
            if (m.Orders)
            {
                // if we have no previosly added Orders table, try to find one already added by the user
                if (orders == null)
                {
                    orders = FindTableInQueryByName(usq, OrdersName);
                }

                // there are no Orders table in query, add one
                if (orders == null)
                {
                    orders = AddTable(usq, OrdersName, OrdersAlias);
                }

                #region link between Orders and Customers
                // we added Orders table,
                // check if we have Customers table too,
                // and if there are no joins between them, create such join
                string joinFieldNameStr = joinFieldName.QualifiedName;
                if (customers != null &&
                    usq.FromClause.FindLink(orders, joinFieldNameStr, customers, joinFieldNameStr) == null &&
                    usq.FromClause.FindLink(customers, joinFieldNameStr, orders, joinFieldNameStr) == null)
                {
                    queryBuilder1.SQLQuery.AddLink(customers, joinFieldName, orders, joinFieldName);
                }
                #endregion

                #region process OrderDate condition
                if (m.OrderDate && !String.IsNullOrEmpty(m.OrderDateText))
                {
                    // if we have no previously added grid row for this condition, add it
                    if (_orderDate == null)
                    {
                        _orderDate = usq.QueryColumnList.AddField(orders, orderDateFieldName.QualifiedName);
                        // do not append it to the select list, use this row for conditions only
                        _orderDate.Selected = false;
                    }

                    // write condition from edit box to all needed grid cells
                    AddWhereCondition(usq.QueryColumnList, _orderDate, m.OrderDateText);
                }
                else
                {
                    // remove prviously added grid row
                    if (_orderDate != null)
                    {
                        _orderDate.Dispose();
                    }

                    _orderDate = null;
                }
                #endregion
            }
            else
            {
                if (orders != null)
                {
                    orders.Dispose();
                    orders = null;
                }
            }
            #endregion
        }

        private void CollectCriteriaItemsWithWhereCondition(QueryColumnList criteriaList, int columnIndex, IList<QueryColumnListItem> result)
        {
            result.Clear();

            foreach (var item in criteriaList)
            {
                if (item.ConditionType == ConditionType.Where &&
                    item.ConditionCount > columnIndex &&
                    item.GetASTCondition(columnIndex) != null)
                {
                    result.Add(item);
                }
            }
        }
    }

    public class Model
    {
        public bool Customers { get; set; }
        public bool CompanyName { get; set; }
        public string CompanyNameText { get; set; }
        public bool Orders { get; set; }
        public bool OrderDate { get; set; }
        public string OrderDateText { get; set; }
    }
}