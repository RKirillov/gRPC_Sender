namespace gRPC_Sender.Interceptor
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Grpc.Core.Interceptors;  // Добавьте это пространство имен
    namespace gRPC_Sender.Service
    {
        public class LoggingInterceptor : Interceptor
        {
            private readonly ILogger<LoggingInterceptor> _logger;

            public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
            {
                _logger = logger;
            }

            // Обработка Server Streaming вызова (StreamEntities)
            public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
                TRequest request,
                IServerStreamWriter<TResponse> responseStream,
                ServerCallContext context,
                ServerStreamingServerMethod<TRequest, TResponse> continuation)
            {
                _logger.LogInformation("Received Streaming call: {Method} with request: {Request}", context.Method, request);

                // Здесь логируем, что начался стриминг
                await continuation(request, responseStream, context);
                /*                Вызов продолжения запроса: После того как запрос был обработан в интерсепторе(например, логирование), передается выполнение основному методу сервиса, который реализует логику обработки запроса.Для этого вызывается continuation — делегат, который выполняет фактическую логику обработки запроса.

                 request — это запрос от клиента (ReceiverRequest).
                responseStream — это поток ответов, в который сервер будет отправлять сообщения типа TResponse (в данном случае это Entity).
                context — это контекст запроса, который содержит информацию о вызове.*/
            }
        }
    }

}
