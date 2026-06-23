using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.DTO_s.Common
{
    public class ApiExceptionResponse : ApiResponse
    {
        public string? details { get; set; }
        public ApiExceptionResponse(int StatusCode,string? Message=null,string? Details=null):base(StatusCode,Message)
        {
            Details = details;
        }
    }
}
