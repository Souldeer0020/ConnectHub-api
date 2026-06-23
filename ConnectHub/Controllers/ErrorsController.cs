using ConnectHub.Application.DTO_s.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConnectHub.Controllers
{
    [Route("error/{statusCode}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
       
        public ActionResult Error(int statusCode)
        {
            return new ObjectResult(new ApiResponse(statusCode))
            {
                StatusCode = statusCode
            };
        }
    }
}
