using AutoMapper;
using Grpc.Core;
using gRPC_Sender.Entity;
using gRPC_Sender.Mapper;
using gRPC_Sender.Service;
using GrpcServices;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Channels;
using Channel = System.Threading.Channels.Channel;
using SenderService = gRPC_Sender.Service.gRPC_Sender.Service.SenderService;

namespace gRPC_Sender.Tests
{
    public class SenderServiceTests
    {
        private readonly Mock<ILogger<SenderService>> _loggerMock;
        private readonly IMapper _mapper;
        private readonly Mock<EntityReader> _entityReaderMock;
        private readonly SenderService _senderService;

        public SenderServiceTests()
        {
            _loggerMock = new Mock<ILogger<SenderService>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>());
            _mapper = config.CreateMapper();

            // Создаём мок для EntityReader, имитируем ChannelReader
            var testEntities = new List<AdkuEntity>
                {
                    new AdkuEntity { TagName = "Tag1", Value = 100, RegisterType = gRPC_Sender.Entity.RegisterType.Ad, DateTime = DateTime.Now, DateTimeUTC = DateTime.UtcNow },
                    new AdkuEntity { TagName = "Tag2", Value = 200, RegisterType = gRPC_Sender.Entity.RegisterType.Ti, DateTime = DateTime.Now, DateTimeUTC = DateTime.UtcNow }
                };

            var channel = Channel.CreateUnbounded<AdkuEntity>();
            foreach (var entity in testEntities)
            {
                channel.Writer.TryWrite(entity);
            }
            channel.Writer.Complete();
            // Передаем null, так как IDataReader не используется в тесте
            //MockBehavior.Strict означает, что любой неожиданный вызов метода или свойства на моке вызовет исключение.
            //_entityReaderMock = new Mock<EntityReader>(MockBehavior.Strict, new object[] { null! });

            var entityReaderMock = new Mock<IEntityReader>();
            entityReaderMock.Setup(x => x.GetReader()).Returns(channel.Reader);

            _senderService = new SenderService(_loggerMock.Object, entityReaderMock.Object, _mapper);
        }
        [Fact]
        public async Task StreamEntities_SendsMappedEntities()
        {
            // Arrange
            //GrpcServices.Entity есть в автомэппере. GrpcServices.Entity – это автоматически сгенерированный класс, который представляет gRPC-сообщение (protobuf message). 
            //Grpc.Core.Testing - для моков ServerCallContext и IServerStreamWriter
            var mockStreamWriter = new Mock<IServerStreamWriter<GrpcServices.Entity>>();
            var mockContext = new Mock<ServerCallContext>();

            // Act
            await _senderService.StreamEntities(new ReceiverRequest(), mockStreamWriter.Object, mockContext.Object);

            // Assert
            mockStreamWriter.Verify(w => w.WriteAsync(It.IsAny<GrpcServices.Entity>()), Times.Exactly(2));
        }
        [Fact]
        public async Task StreamEntities_HandlesRpcException()
        {
            // Arrange
            var mockStreamWriter = new Mock<IServerStreamWriter<GrpcServices.Entity>>();
            var mockContext = new Mock<ServerCallContext>();

            // Подготовим тестовые данные
            var testEntities = new List<AdkuEntity>
                {
                    new AdkuEntity { TagName = "Tag1", Value = 100, RegisterType = Entity.RegisterType.Ad, DateTime = DateTime.Now, DateTimeUTC = DateTime.UtcNow }
                };

            var channel = Channel.CreateUnbounded<AdkuEntity>();
            foreach (var entity in testEntities)
            {
                channel.Writer.TryWrite(entity);
            }
            channel.Writer.Complete();

            _entityReaderMock.Setup(x => x.GetReader()).Returns(channel.Reader);

            // Симулируем ошибку при записи в поток
            mockStreamWriter
                .Setup(w => w.WriteAsync(It.IsAny<GrpcServices.Entity>()))
                .ThrowsAsync(new RpcException(new Status(StatusCode.Internal, "Ошибка при отправке сущности")));

            // Act
            await _senderService.StreamEntities(new ReceiverRequest(), mockStreamWriter.Object, mockContext.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<RpcException>(), "Ошибка при отправке сущности"),
                Times.AtLeastOnce()); // Проверяем, что логгер был вызван при ошибке
        }

    }
}
