using Microsoft.EntityFrameworkCore;
using MiniChatApp2.Data;
using System.Security.Claims;
using System.Text;

namespace MiniChatApp2.Middlewares
{
    public class Middleware : IMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MiniChatApp2Context _dbcontext;
        private readonly ILogger<Middleware> _logger;
        public Middleware(RequestDelegate next)
        {
            _next = next;
        }
        public Middleware(ILogger<Middleware> logger, MiniChatApp2Context dbcontext)
        {
            _logger = logger;
            _dbcontext = dbcontext;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
                var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                Console.WriteLine(context.User);

                string ip = context.Connection.RemoteIpAddress?.ToString();
                string RequestBody = await getRequestBodyAsync(context.Request);
                string TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string UserName = userName;


                string log = $"IP: {ip}, Username: {UserName}, Timestamp: {TimeStamp}, Request Body: {RequestBody}";

                _logger.LogInformation(log);

                _dbcontext.Logs.Add(new Model.Logs
                {
                    ip = ip,
                    RequestBody = RequestBody,
                    TimeStamp = TimeStamp,
                    Username = UserName,
                });

                await _dbcontext.SaveChangesAsync();

                await next(context);
            }

            public async Task<string> getRequestBodyAsync(HttpRequest req)
            {
                req.EnableBuffering();

                using var reader = new StreamReader(req.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                string requestBody = await reader.ReadToEndAsync();

                req.Body.Position = 0;

                return requestBody;
            }
        }
    }

