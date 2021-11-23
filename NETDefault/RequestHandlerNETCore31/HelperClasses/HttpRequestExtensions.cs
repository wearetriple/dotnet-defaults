using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RequestHandlerNETCore31.HelperClasses
{
    internal static class HttpRequestExtensions
    {
        public static async Task<string> ReadAsStringAsync(this HttpRequest request)
        {
            request.EnableBuffering();
            string result = null;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
            {
                result = await reader.ReadToEndAsync();
            }

            request.Body.Seek(0L, SeekOrigin.Begin);
            return result;
        }
    }
}
