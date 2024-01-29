using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SentinelExample.Services;

namespace SentinelExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        [HttpGet("setValue/{key}/{value}")]
        public async Task<IActionResult> setValue(string key, string value)
        {
            var redis = await RedisService.redisMasterDatabase();
            await redis.StringSetAsync(key, value);
            return Ok();
        }
        [HttpGet("getValue/{key}")]
        public async Task<IActionResult> getValue(string key)
        {
            var redis = await RedisService.redisMasterDatabase();
            var data = await redis.StringGetAsync(key);
            return Ok(data.ToString());
        }
    }
}
