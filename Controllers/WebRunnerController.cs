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

namespace WebRunner.Controllers
{
    [ApiController]    
    public class WebRunnerController : ControllerBase
    {
        protected ILogger logger;
        private IConfiguration _config;

        public WebRunnerController(IConfiguration config)
        {            
            _config = config;
        }

        [HttpGet]
        [Route("run/{name}")]
        public IActionResult Index(string name)
        {
            logger = LogManager.GetLogger(name);

            var commands = _config.GetSection("Commands").GetChildren();

            var command = commands.Where(item => item["name"] == name).FirstOrDefault();

            if (command == null) return NotFound("Command not found");
            
            var process = new Process();
            process.StartInfo.FileName = command["command"];
            process.StartInfo.Arguments = command["arguments"];
            process.StartInfo.WorkingDirectory = @"c:\";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, a) => logger.Info(a.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return Ok("Done");
        }
    }
}
