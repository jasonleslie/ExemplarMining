using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exemplar_Mining.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly IActionDescriptorCollectionProvider _actionCollection;

        public APIController(IActionDescriptorCollectionProvider actionCollection)
        {
            _actionCollection = actionCollection;
        }


        /*
         * Returns a list of all possible actions that are availible on the API. 
         * Action - name of the Method e.g. GetAllActions()
         * Verb - HTTP verb e.g. GET
         * Parameters - List of all query parameters e.g. [ "employeeName" ]
         * Location - URL location of the action e.g. "https://myhost:5001/api/Reports"
         */
        [HttpGet]
        public IActionResult GetAllActions()
        {
            var baseurl = this.Request.Scheme + "://" + this.Request.Host;

            var routes = _actionCollection.ActionDescriptors.Items.Where(x => x.RouteValues["Controller"] != "API" && x.ActionConstraints != null).Select(x => new
            {
                Action = x.RouteValues["Action"],
                Verb = x.ActionConstraints.OfType<HttpMethodActionConstraint>().Select(x => x.HttpMethods.First()).First(),
                Parameters = ((x.Parameters != null) ? x.Parameters.Select(x => x.Name) : new List<String>()),
                Location = baseurl + "/" + x.AttributeRouteInfo.Template
            }).ToList();

            return Ok(routes);
        }
    }
}