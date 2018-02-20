using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth.WebExample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet("anonymous")]
        [AllowAnonymous]
        public string Anonymous()
        {
            return "success";
        }

        [HttpGet("whoami")]
        [Authorize]
        public string WhoAmI()
        {
            return User.Identity.Name;
        }
    }
}