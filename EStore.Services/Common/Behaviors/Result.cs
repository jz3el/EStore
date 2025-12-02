using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EStore.Services.Common.Behaviors
{
    public abstract class Result
    {
        public List<Errors>? Errors { get; set; }
        public bool isError => Errors != null && Errors.Count > 0;
    }

    public class Result<T> : Result
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? StatusCode { get; set; }
    }
}
