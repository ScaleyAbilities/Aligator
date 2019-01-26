using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Asp.Controllers
{
    [Route("api/command")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        // POST api/command
        [HttpPost]
        public ActionResult<string> Post([FromBody] JObject json)
        {
            try
            {
                RabbitHelper.PushCommand(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
            return "Pushed to queue";
        }
    }
}
