using Microsoft.AspNetCore.Mvc;

namespace MApi.Models
{
    public class ResponseBase<T>
    {
        public bool Success { set; get; }
        public string Error { set; get; }
        public T Result { set; get; }

        public static JsonResult Failed(string errMsg)
        {
            return new JsonResult(new { Success = false, Error = errMsg });
        }
        public static JsonResult Ok(T result)
        {
            return new JsonResult(new { Result = result , Success = true, Error = "" });
        }
    }
}
