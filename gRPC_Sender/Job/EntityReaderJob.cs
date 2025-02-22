namespace gRPC_Sender.Job
{
    using gRPC_Sender.Service;
    using Quartz;
    using System.Threading.Tasks;

    //Реализуйте класс, который будет выполнять метод StartReadingAsync из EntityReader.
    public class EntityReaderJob : IJob
    {
        private readonly EntityReader _entityReader;

        public EntityReaderJob(EntityReader entityReader)
        {
            _entityReader = entityReader;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _entityReader.StartReadingAsync(context.CancellationToken);
        }
    }

}
