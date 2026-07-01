using System.Net;
using Libify.Infraestructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Libify.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Erro não tratado na requisição.");

            var statusCode = exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                FileNotFoundException => HttpStatusCode.NotFound,
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                AsaasApiException asaas => asaas.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError
                    ? asaas.StatusCode
                    : HttpStatusCode.BadGateway,
                _ => HttpStatusCode.InternalServerError
            };

            var mensagem = statusCode switch
            {
                HttpStatusCode.Conflict => "O registro foi alterado por outra operação. Recarregue os dados e tente novamente.",
                HttpStatusCode.BadGateway => exception is AsaasApiException a ? a.Message : "Erro na integração com o gateway de pagamento.",
                _ => "Ocorreu um erro inesperado."
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(new { error = mensagem });
        }
    }
}
