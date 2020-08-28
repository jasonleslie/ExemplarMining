using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exemplar_Mining.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        /*
         * Returns a generic message when any uncaught exception occurs.
         */
        public IActionResult Error() => new ObjectResult(new { code = 500, message = "Something went wrong." });
    }

}