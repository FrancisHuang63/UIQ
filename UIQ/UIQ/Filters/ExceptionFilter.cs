using Microsoft.AspNetCore.Mvc.Filters;
using UIQ.Services.Interfaces;

namespace UIQ.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var errorMessage = $"{GetType().Name} exception. Message: {context.Exception.Message}";
            var logFileService = context.HttpContext.RequestServices.GetService(typeof(ILogFileService)) as ILogFileService;
            logFileService.WriteUiErrorLogFileAsync(errorMessage);
            return Task.CompletedTask;
        }
    }
}