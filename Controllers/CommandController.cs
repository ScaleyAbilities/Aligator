using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        // GET test route
        [HttpGet]
        public ActionResult<string> Get() 
        {
            return "hello world";
        }

        // POST api/command
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotImplementedException();
        }
    }
}
