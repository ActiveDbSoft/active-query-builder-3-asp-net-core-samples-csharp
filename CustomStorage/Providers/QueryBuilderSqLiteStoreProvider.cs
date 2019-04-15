using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using AspNetCoreCustomStorage.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreCustomStorage.Providers
{
    /// <summary>
    /// QueryBuilder storage provider which saves the state in Sqlite database
    /// </summary>
    public class QueryBuilderSqLiteStoreProvider : IQueryBuilderProvider
    {
        public bool SaveState { get; private set; }

        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;

        public QueryBuilderSqLiteStoreProvider(IHostingEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;

            SaveState = true;

            var sql = "create table if not exists QueryBuilders(id text primary key, layout TEXT)";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// Creates an instance of the QueryBuilder object and loads its state identified by the given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public QueryBuilder Get(string id)
        {
            var qb = new QueryBuilder(id) { SyntaxProvider = new SQLiteSyntaxProvider() };

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            qb.BehaviorOptions.AllowSleepMode = false;

            // Bind Active Query Builder to a live database connection.
            qb.MetadataProvider = new SQLiteMetadataProvider
            {
                // Assign an instance of DBConnection object to the Connection property.
                Connection = DataBaseHelper.CreateSqLiteConnection(GetDatabasePath())
            };

            var layout = GetLayout(id);

            try
            {
                if (layout != null)
                    qb.LayoutSQL = layout;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return qb;
        }

        private string GetDatabasePath()
        {
            var path = _config["DatabasePath"];
            return Path.Combine(_env.WebRootPath, path);
        }

        /// <summary>
        /// Saves the state of QueryBuilder object identified by its Tag property.
        /// </summary>
        /// <param name="qb">The QueryBuilder object.</param>
        public void Put(QueryBuilder qb)
        {
            if (GetLayout(qb.Tag) == null)
                Insert(qb);
            else
                Update(qb);
        }

        /// <summary>
        /// Clears the state of QueryBuilder object identified by the given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var sql = string.Format("delete from QueryBuilders where id = {0}", id);
            ExecuteNonQuery(sql);
        }

        private void Insert(QueryBuilder qb)
        {
            var sql = string.Format("insert into QueryBuilders values ('{0}', '{1}')", qb.Tag, qb.LayoutSQL);
            ExecuteNonQuery(sql);
        }
        private void Update(QueryBuilder qb)
        {
            var sql = string.Format("update QueryBuilders set layout = '{1}' where id = '{0}'", qb.Tag, qb.LayoutSQL);
            ExecuteNonQuery(sql);
        }

        private void ExecuteNonQuery(string sql)
        {
            var _connection = DataBaseHelper.CreateSqLiteConnection(GetDatabasePath());

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(_connection, sql))
                    cmd.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

        private string GetLayout(string id)
        {
            var sql = string.Format("select layout from QueryBuilders where id = '{0}'", id);
            var _connection = DataBaseHelper.CreateSqLiteConnection(GetDatabasePath());

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(_connection, sql))
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return reader["layout"].ToString();

                return null;
            }
            finally
            {
                _connection.Close();
            }
        }

        private IDbCommand CreateCommand(IDbConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }
    }
}
