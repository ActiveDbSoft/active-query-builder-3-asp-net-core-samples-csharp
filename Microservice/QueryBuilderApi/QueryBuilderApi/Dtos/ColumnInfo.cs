using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QueryBuilderApi.Dtos
{
    public class ColumnInfo
    {
        public string Name { get; }
        public DbType Type { get; }

        public ColumnInfo() { }

        public ColumnInfo(string name, DbType type)
        {
            Name = name;
            Type = type;
        }
    }
}
