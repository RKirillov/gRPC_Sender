using gRPC_Sender.Entity;
using static Grpc.Core.Metadata;

namespace gRPC_Sender.Repository
{
    public interface IDataReader
    {
        IEnumerable<AdkuEntity> ReadEntities();
        Task<IEnumerable<AdkuEntity>> ReadEntitiesAsync(CancellationToken cancellationToken);
    }

}
