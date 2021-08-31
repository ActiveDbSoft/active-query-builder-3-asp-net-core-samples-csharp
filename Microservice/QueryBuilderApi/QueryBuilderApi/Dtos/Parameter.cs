using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QueryBuilderApi.Dtos
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string CompareOperator { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }

        public Parameter() { }

        public Parameter(string name, string symbol, string compareOperator, DbType dataType)
        {
            Name = name;
            Symbol = symbol;
            CompareOperator = compareOperator;
            DataType = dataType.ToString();
        }
    }
}
