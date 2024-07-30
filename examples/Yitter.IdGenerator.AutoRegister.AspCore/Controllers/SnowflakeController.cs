using Microsoft.AspNetCore.Mvc;

namespace Yitter.IdGenerator.AutoRegister.AspCore.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SnowflakeController : ControllerBase
{
    private readonly ILogger<SnowflakeController> _logger;

    public SnowflakeController(ILogger<SnowflakeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public long Get()
    {
        _logger.LogInformation("Get SnowflakeId : {Id}", IdGeneratorUtil.NextId());
        return IdGeneratorUtil.NextId();
    }
}