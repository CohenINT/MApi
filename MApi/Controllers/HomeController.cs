using MApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MApi.Controllers
{

    [Route("apiv1/[controller]/[action]")]
    public class HomeController : Controller
    {
        public IServiceProvider services { get; set; }
        public AppConfiguration config { set; get; }
        public HomeController(IServiceProvider svc)
        {
            this.services = svc;
            this.config = svc.GetRequiredService<AppConfiguration>();

        }


        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<dynamic>),200)]
        public async Task<JsonResult> GetSomeData()
        {
            var aa = this.config.ApplicationConfiguration.Worker.Active;
            if (aa)
            {
                return ResponseBase<dynamic>.Ok(aa);
            }
            else
                return ResponseBase<dynamic>.Failed("bad request");
        }

    }
}
