using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task<ActionResult<JObject>> Post([FromBody] JObject json)
        {
            var username = json["usr"]?.ToString();
            var command = json["cmd"]?.ToString();

            if (command == "DUMPLOG" && string.IsNullOrEmpty(username))
                username = "admin";

            if (string.IsNullOrEmpty(username))
                return BadRequest("No user specified");

            // Add a reference value so we can get response
            var reference = Guid.NewGuid().ToString("N");
            json.Add("ref", reference);

            var referenceCompletion = new TaskCompletionSource<JObject>();
            Program.PendingResponses.Add(reference, referenceCompletion);

            var instance = (username[1] % Program.Instances) + 1;

            try
            {
                RabbitHelper.PushCommand(json, instance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            var timeoutCancellation = new CancellationTokenSource();
            var timeoutTask = Task.Delay(60000).ContinueWith(t => {
                // Were we already canceled?
                if (timeoutCancellation.IsCancellationRequested)
                    return null;

                var timeout = new JObject();
                timeout.Add("status", "error");
                timeout.Add("data", "Timed out waiting for reply from transaction server");

                Program.PendingResponses.Remove(reference);

                return timeout;
            });

            var result = await (await Task.WhenAny(referenceCompletion.Task, timeoutTask));
            timeoutCancellation.Cancel();
            
            return result;
        }
    }
}
