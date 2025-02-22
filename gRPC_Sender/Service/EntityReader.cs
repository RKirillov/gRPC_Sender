namespace gRPC_Sender.Service
{
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using System.Threading;
    using global::gRPC_Sender.Repository;
    using global::gRPC_Sender.Entity;

    public class EntityReader
    {
        private readonly IDataReader _dataReader;
        private readonly Channel<AdkuEntity> _channel;
        //private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public EntityReader(IDataReader dataReader, int capacity = 1000)
        {
            _dataReader = dataReader;
            _channel = Channel.CreateBounded<AdkuEntity>(capacity);
        }

        public async Task StartReadingAsync(CancellationToken cancellationToken)
        {
            /*            if (await _semaphore.WaitAsync(0, cancellationToken))
                        {*/
            //try
            //{
            var entities = _dataReader.ReadEntities();
            foreach (var entity in entities)
            {
                if (!_channel.Writer.TryWrite(entity))
                {
                    await _channel.Writer.WaitToWriteAsync(cancellationToken);
                    _channel.Writer.TryWrite(entity);
                }
            }
            //}
            /*                finally
                            {
                                _semaphore.Release();
                            }*/
            //  }
            //  else
            // {
            // Логика, если метод уже выполняется
            // Например, логирование или игнорирование вызова
            // }

            // _channel.Writer.Complete();
        }

        public ChannelReader<AdkuEntity> GetReader() => _channel.Reader;
    }
}
