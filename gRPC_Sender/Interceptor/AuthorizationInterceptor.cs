namespace gRPC_Sender.Interceptor
{
    using Grpc.Core;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Grpc.Core.Interceptors;  // Добавьте это пространство имен

    public class AuthorizationInterceptor : Interceptor
    {
        private readonly ILogger<AuthorizationInterceptor> _logger;
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationInterceptor(ILogger<AuthorizationInterceptor> logger, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _authorizationService = authorizationService;
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            // Извлечение токена из метаданных
            var token = context.RequestHeaders.GetValue("Authorization");

            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "No authorization token provided."));
            }

            // Убираем префикс "Bearer "
            token = token.Substring("Bearer ".Length);

            // Проверка авторизации с помощью токена
            var result = await _authorizationService.AuthorizeAsync(context.GetHttpContext().User, null, "GrpcAccessPolicy");

            if (!result.Succeeded)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "You are not authorized to call this service."));
            }

            // Продолжение обработки запроса
            await continuation(request, responseStream, context);
        }
    }

}
