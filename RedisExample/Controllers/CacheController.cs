using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;

        public CacheController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        [HttpGet("set")]
        public async Task<IActionResult> Set(string name)
        {
            await _distributedCache.SetStringAsync("name", name, options: new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(10)
            });
            return Ok();
        }
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _distributedCache.GetStringAsync("name"));

        }
    }
}
