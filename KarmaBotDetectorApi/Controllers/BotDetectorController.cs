using Microsoft.AspNetCore.Mvc;
using RedditBotDetector.Library;

namespace KarmaBotDetectorApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class BotDetectorController : ControllerBase {
        [HttpGet("{userName}")]
        public RepostReport Get([FromRoute] string userName) {
            return RepostChecker.CheckUser(userName);
        }
    }
}