using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using System.DirectoryServices.AccountManagement;
using Newtonsoft.Json;

namespace WebRunner.Controllers
{
    public class RunInput
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("user")]
        public string User{ get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    [ApiController]    
    public class WebRunnerController : ControllerBase
    {
        protected ILogger logger;
        private readonly IConfiguration _config;

        public WebRunnerController(IConfiguration config)
        {            
            _config = config;
        }

        [HttpPost]
        [Route("run")]
        public IActionResult Index([FromBody] RunInput input)
        {

            var user = _config["AllowedUsers"].Split(",").FirstOrDefault(item => item.Trim() == input.User);

            if (string.IsNullOrEmpty(user)) return Forbid();


            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, _config["Domain"]))
            {
                bool isValid = pc.ValidateCredentials(input.User, input.Password);

                if (!isValid) return Forbid();
            }

            logger = LogManager.GetLogger(input.Group);

            var commandGroup = _config.GetSection($"Commands:{input.Group}").GetChildren();

            if (commandGroup.Count() == 0)
            {
                logger.Error($"Group not found or empty: {input.Group}");
                return NotFound($"Group not found or empty: {input.Group}");
            }

            List<string> stdout = new List<string>();

            foreach (var command in commandGroup)
            {
                var process = new Process();
                process.StartInfo.FileName = command["command"];
                process.StartInfo.Arguments = command["arguments"];
                process.StartInfo.WorkingDirectory = @"c:\";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.OutputDataReceived += (sender, a) => stdout.Add(a.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }

            logger.Info(string.Join('\n', stdout));

            return Ok(stdout);
        }
    }
}
