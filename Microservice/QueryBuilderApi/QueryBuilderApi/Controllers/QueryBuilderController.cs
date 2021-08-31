using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QueryBuilderApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace QueryBuilderApi.Controllers
{
    [Route("/")]
    public class QueryBuilderController : ControllerBase
    {
        private readonly IQueryBuilderService _queryBuilderService;

        public QueryBuilderController(IQueryBuilderService queryBuilderService)
        {
            _queryBuilderService = queryBuilderService;
        }

        [Route("create"), HttpPost]
        public IActionResult Create([FromBody] InstanceIdModel model)
        {
            _queryBuilderService.GetOrCreate(model.InstanceId);
            return new JsonResult(new { succeeded = true });
        }

        [Route("getQuery"), HttpPost]
        public IActionResult GetQuery([FromBody] GetQueryModel model)
        {
            var query = _queryBuilderService.GetQuery(model.InstanceId, model.Start, model.End, model.Sorting);
            return new JsonResult(new { succeeded = true, query });
        }
        
        [Route("getQueryCount"), HttpPost]
        public IActionResult GetQueryCount([FromBody] InstanceIdModel model)
        {
            var query = _queryBuilderService.GetQueryCount(model.InstanceId);
            return new JsonResult(new { succeeded = true, query });
        }

        public class InstanceIdModel
        {
            public string InstanceId { get; set; }
        }

        public class GetQueryModel : InstanceIdModel
        {
            public int? Start { get; set; }
            public int? End { get; set; }

            public List<SortingModel> Sorting { get; set; }
        }
    }
}
