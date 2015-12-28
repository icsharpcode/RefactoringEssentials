using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodeConverterWebApp.Models;

namespace CodeConverterWebApp.Controllers
{
    public class ConverterController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Post([FromBody]ConvertRequest todo)
        {
            string code = todo.code;

            var response = new ConvertResponse()
            {
                conversionOk = false,
                convertedCode = "",
                errorMessage = "Not implemented"
            };

            return Ok(response);
        }
    }
}
