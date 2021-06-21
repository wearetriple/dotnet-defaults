using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebApp.Models
{
    public class HttpPathSettings
    {
        public IEnumerable<PathString> Paths { get; set; }
    }
}
