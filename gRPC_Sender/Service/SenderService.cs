using Grpc.Core;

namespace gRPC_Sender.Service
{
    // SenderService.cs
    using GrpcServices;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using AutoMapper;
    using global::gRPC_Sender.Redis;
    using global::gRPC_Sender.Entity;

    namespace gRPC_Sender.Service
    {
        public class SenderService : GrpcServices.SenderService.SenderServiceBase
        {
            private readonly ILogger<SenderService> _logger;
            private readonly IEntityReader _entityReader;
            private readonly ICacheService _cacheService;
            private readonly IMapper _mapper;

            public SenderService(ILogger<SenderService> logger, IEntityReader entityReader, IMapper mapper,ICacheService cacheService)
            {
                _logger = logger;
                _entityReader = entityReader;
                _cacheService=cacheService;
                _mapper = mapper;
            }
/*            Server Streaming методы(как StreamEntities) обрабатываются через ServerStreamingServerHandler, что и нужно для вашей ситуации, поскольку сервер отправляет несколько сообщений в ответ на один запрос клиента.*/
            public override async Task StreamEntities(ReceiverRequest request, IServerStreamWriter<Entity> responseStream, ServerCallContext context)
            {
                try
                {
                    await foreach (var entity in _entityReader.GetReader().ReadAllAsync(context.CancellationToken))
                    {
                        try
                        {
                            var grpcEntity = _mapper.Map<Entity>(entity);
                            await responseStream.WriteAsync(grpcEntity);
                            string key = $"Adku:{entity.TagName}:{entity.DateTimeUTC:yyyyMMddHHmmss}";
                            await _cacheService.SetAsync<AdkuEntity>(key, entity);
                            //var z = await _cacheService.GetAsync<AdkuEntity>("Redis");
                        }
                        catch (RpcException ex)
                        {
                            //складывать в резистентный кеш Redis.
                            _logger.LogError(ex, "Ошибка при отправке сущности");
                            // Логика повторной отправки сущности
                        }
                    }
                }
                catch (RpcException ex)
                {
                    _logger.LogError(ex, "Ошибка при чтении сущностей");
                    // Логика повторной отправки сущностей
                }
            }
        }
    }
}
