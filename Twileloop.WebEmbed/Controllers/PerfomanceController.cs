using Microsoft.AspNetCore.Mvc;

namespace Twileloop.WebEmbed
{
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PerfomanceController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "Perf")]
        public IActionResult Get()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => new
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
        }
    }
}
