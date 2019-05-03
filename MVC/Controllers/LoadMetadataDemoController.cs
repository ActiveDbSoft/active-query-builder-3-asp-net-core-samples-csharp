using System;
using System.Data;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using ASP.NET_Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class LoadMetadataDemoController : Controller
    {
        private readonly IDbConnection _conn;

        private string instanceId = "BootstrapTheming";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public LoadMetadataDemoController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;

            _conn = DataBaseHelper.CreateSqLiteConnection(Path.Combine(_env.WebRootPath, _config["SqLiteDataBase"]));
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            if (qb == null)
            {
                qb = _aqbs.Create(instanceId);
                qb.SyntaxProvider = new GenericSyntaxProvider();
            }

            return View(qb);
        }

        //////////////////////////////////////////////////////////////////////////
        /// 1st way:
        /// This method demonstrates the direct access to the internal metadata 
        /// objects collection (MetadataContainer).
        //////////////////////////////////////////////////////////////////////////
        public void Way1()
        {
            var queryBuilder1 = _aqbs.Get(instanceId);

            ResetQueryBuilderMetadata(queryBuilder1);
            queryBuilder1.SyntaxProvider = new GenericSyntaxProvider();
            // prevent QueryBuilder to request metadata
            queryBuilder1.MetadataLoadingOptions.OfflineMode = true;

            queryBuilder1.MetadataProvider = null;

            MetadataContainer metadataContainer = queryBuilder1.MetadataContainer;
            metadataContainer.BeginUpdate();

            try
            {
                metadataContainer.Clear();

                MetadataNamespace schemaDbo = metadataContainer.AddSchema("dbo");

                // prepare metadata for table "Orders"
                MetadataObject orders = schemaDbo.AddTable("Orders");
                // fields
                orders.AddField("OrderId");
                orders.AddField("CustomerId");

                // prepare metadata for table "Order Details"
                MetadataObject orderDetails = schemaDbo.AddTable("Order Details");
                // fields
                orderDetails.AddField("OrderId");
                orderDetails.AddField("ProductId");
                // foreign keys
                MetadataForeignKey foreignKey = orderDetails.AddForeignKey("OrderDetailsToOrders");

                using (MetadataQualifiedName referencedName = new MetadataQualifiedName())
                {
                    referencedName.Add("Orders");
                    referencedName.Add("dbo");

                    foreignKey.ReferencedObjectName = referencedName;
                }

                foreignKey.Fields.Add("OrderId");
                foreignKey.ReferencedFields.Add("OrderId");
            }
            finally
            {
                metadataContainer.EndUpdate();
            }

            queryBuilder1.MetadataStructure.Refresh();
        }

        //////////////////////////////////////////////////////////////////////////
        /// 2rd way:
        /// This method demonstrates on-demand manual filling of metadata structure using 
        /// corresponding MetadataContainer.ItemMetadataLoading event
        //////////////////////////////////////////////////////////////////////////
        public void Way2()
        {
            var queryBuilder1 = _aqbs.Get(instanceId);
            ResetQueryBuilderMetadata(queryBuilder1);
            // allow QueryBuilder to request metadata
            queryBuilder1.MetadataLoadingOptions.OfflineMode = false;

            queryBuilder1.MetadataProvider = null;
            queryBuilder1.MetadataContainer.ItemMetadataLoading += way2ItemMetadataLoading;
            queryBuilder1.MetadataStructure.Refresh();
        }

        private void way2ItemMetadataLoading(object sender, MetadataItem item, MetadataType types)
        {
            switch (item.Type)
            {
                case MetadataType.Root:
                    if ((types & MetadataType.Schema) > 0) item.AddSchema("dbo");
                    break;

                case MetadataType.Schema:
                    if ((item.Name == "dbo") && (types & MetadataType.Table) > 0)
                    {
                        item.AddTable("Orders");
                        item.AddTable("Order Details");
                    }
                    break;

                case MetadataType.Table:
                    if (item.Name == "Orders")
                    {
                        if ((types & MetadataType.Field) > 0)
                        {
                            item.AddField("OrderId");
                            item.AddField("CustomerId");
                        }
                    }
                    else if (item.Name == "Order Details")
                    {
                        if ((types & MetadataType.Field) > 0)
                        {
                            item.AddField("OrderId");
                            item.AddField("ProductId");
                        }

                        if ((types & MetadataType.ForeignKey) > 0)
                        {
                            MetadataForeignKey foreignKey = item.AddForeignKey("OrderDetailsToOrder");
                            foreignKey.Fields.Add("OrderId");
                            foreignKey.ReferencedFields.Add("OrderId");
                            using (MetadataQualifiedName name = new MetadataQualifiedName())
                            {
                                name.Add("Orders");
                                name.Add("dbo");

                                foreignKey.ReferencedObjectName = name;
                            }
                        }
                    }
                    break;
            }

            item.Items.SetLoaded(types, true);
        }

        //////////////////////////////////////////////////////////////////////////
        /// 3rd way:
        ///
        /// This method demonstrates loading of metadata through .NET data providers 
        /// unsupported by our QueryBuilder component. If such data provider is able 
        /// to execute SQL queries, you can use our EventMetadataProvider with handling 
        /// it's ExecSQL event. In this event the EventMetadataProvider will provide 
        /// you SQL queries it use for the metadata retrieval. You have to execute 
        /// a query and return resulting data reader object.
        ///
        /// Note: In this sample we are using SQLiteSyntaxProvider. You have to use specific syntax providers in your application, 
        /// e.g. MySQLSyntaxProver, OracleSyntaxProvider, etc.
        //////////////////////////////////////////////////////////////////////////
        public void Way3()
        {
            var queryBuilder1 = _aqbs.Get(instanceId);

            try
            {
                _conn.Close();
                _conn.Open();

                // allow QueryBuilder to request metadata
                queryBuilder1.MetadataLoadingOptions.OfflineMode = false;

                ResetQueryBuilderMetadata(queryBuilder1);
                queryBuilder1.SyntaxProvider = new SQLiteSyntaxProvider();

                //queryBuilder1.MetadataProvider = new EventMetadataProvider();
                //((EventMetadataProvider) queryBuilder1.MetadataProvider).ExecSQL += way3EventMetadataProvider_ExecSQL;
                queryBuilder1.MetadataStructure.Refresh();

            }
            catch (Exception ex)
            {
                queryBuilder1.Message.Error(ex.Message);
            }
        }

        private void way3EventMetadataProvider_ExecSQL(BaseMetadataProvider metadataProvider, string sql, bool schemaOnly, out IDataReader dataReader)
        {
            dataReader = null;

            if (_conn != null)
            {
                IDbCommand command = _conn.CreateCommand();
                command.CommandText = sql;
                dataReader = command.ExecuteReader();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        /// 4th way:
        /// This method demonstrates manual filling of metadata structure from 
        /// stored DataSet.
        //////////////////////////////////////////////////////////////////////////
        public void Way4()
        {
            var queryBuilder1 = _aqbs.Get(instanceId);
            ResetQueryBuilderMetadata(queryBuilder1);
            
            queryBuilder1.MetadataLoadingOptions.OfflineMode = true; // prevent QueryBuilder to request metadata from connection

            DataSet dataSet = new DataSet();

            // Load sample dataset created in the Visual Studio with Dataset Designer
            // and exported to XML using WriteXmlSchema() method.
            var xml = Path.Combine(_env.WebRootPath, _config["StoredDataSetSchema"]);
            dataSet.ReadXmlSchema(xml);

            queryBuilder1.MetadataContainer.BeginUpdate();

            try
            {
                queryBuilder1.ClearMetadata();

                // add tables
                foreach (DataTable table in dataSet.Tables)
                {
                    // add new metadata table
                    MetadataObject metadataTable = queryBuilder1.MetadataContainer.AddTable(table.TableName);

                    // add metadata fields (columns)
                    foreach (DataColumn column in table.Columns)
                    {
                        // create new field
                        MetadataField metadataField = metadataTable.AddField(column.ColumnName);
                        // setup field
                        metadataField.FieldType = TypeToDbType(column.DataType);
                        metadataField.Nullable = column.AllowDBNull;
                        metadataField.ReadOnly = column.ReadOnly;

                        if (column.MaxLength != -1)
                        {
                            metadataField.Size = column.MaxLength;
                        }

                        // detect the field is primary key
                        foreach (DataColumn pkColumn in table.PrimaryKey)
                        {
                            if (column == pkColumn)
                            {
                                metadataField.PrimaryKey = true;
                            }
                        }
                    }

                    // add relations
                    foreach (DataRelation relation in table.ParentRelations)
                    {
                        // create new relation on the parent table
                        MetadataForeignKey metadataRelation = metadataTable.AddForeignKey(relation.RelationName);

                        // set referenced table
                        using (MetadataQualifiedName referencedName = new MetadataQualifiedName())
                        {
                            referencedName.Add(relation.ParentTable.TableName);

                            metadataRelation.ReferencedObjectName = referencedName;
                        }

                        // set referenced fields
                        foreach (DataColumn parentColumn in relation.ParentColumns)
                        {
                            metadataRelation.ReferencedFields.Add(parentColumn.ColumnName);
                        }

                        // set fields
                        foreach (DataColumn childColumn in relation.ChildColumns)
                        {
                            metadataRelation.Fields.Add(childColumn.ColumnName);
                        }
                    }
                }
            }
            finally
            {
                queryBuilder1.MetadataContainer.EndUpdate();
            }

            queryBuilder1.MetadataStructure.Refresh();
        }

        private static DbType TypeToDbType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(Int16)) return DbType.Int16;
            if (type == typeof(Int32)) return DbType.Int32;
            if (type == typeof(Int64)) return DbType.Int64;
            if (type == typeof(UInt16)) return DbType.UInt16;
            if (type == typeof(UInt32)) return DbType.UInt32;
            if (type == typeof(UInt64)) return DbType.UInt64;
            if (type == typeof(Boolean)) return DbType.Boolean;
            if (type == typeof(Single)) return DbType.Single;
            if (type == typeof(Double)) return DbType.Double;
            if (type == typeof(Decimal)) return DbType.Decimal;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(TimeSpan)) return DbType.Time;
            if (type == typeof(Byte)) return DbType.Byte;
            if (type == typeof(SByte)) return DbType.SByte;
            if (type == typeof(Char)) return DbType.String;
            if (type == typeof(Byte[])) return DbType.Binary;
            if (type == typeof(Guid)) return DbType.Guid;
            return DbType.Object;
        }

        private void ResetQueryBuilderMetadata(QueryBuilder queryBuilder1)
        {
            queryBuilder1.MetadataProvider = null;
            queryBuilder1.ClearMetadata();
            queryBuilder1.MetadataContainer.ItemMetadataLoading -= way2ItemMetadataLoading;
        }
    }
}