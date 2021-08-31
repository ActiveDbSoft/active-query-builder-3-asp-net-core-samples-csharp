using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core;

namespace QueryBuilderApi.Services
{
    public class QueryBuilderMetadataStorage
    {
        private readonly SQLContext _sqlContext = new SQLContext
        {
            SyntaxProvider = new MySQLSyntaxProvider(),
        };

        public QueryBuilderMetadataStorage()
        {
            LoadMetadata(_sqlContext.MetadataContainer);
        }

        public void SetMetadata(ActiveQueryBuilder.Web.Server.QueryBuilder queryBuilder)
        {
            queryBuilder.MetadataContainer.Assign(_sqlContext.MetadataContainer);
        }

        private void LoadMetadata(MetadataContainer container)
        {
            using (var stream = typeof(QueryBuilderMetadataStorage).Assembly.GetManifestResourceStream("QueryBuilderApi.AdventureWorks.xml"))
                container.ImportFromXML(stream);
        }
    }
}
