namespace gRPC_Sender.Repository
{
    using System.Data.SqlClient;
    using Dapper;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoBogus;
    using Bogus;
    using gRPC_Sender.Entity;

    public class DataReader : IDataReader
    {
        private readonly string _connectionString;
        private static readonly Random _random = new Random();
        private int valueCounter = 0;
        public DataReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<AdkuEntity>> ReadEntitiesAsync(CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var entities = await connection.QueryAsync<AdkuEntity>("SELECT TagName, Value FROM Entities");

            return entities;
        }

        public IEnumerable<AdkuEntity> ReadEntities()
        {

            // Генерация случайного количества сущностей от 50 до 500
            int count = _random.Next(50, 50);
            // Создание экземпляра AutoFaker для AdkuEntity
            var faker = new AutoFaker<AdkuEntity>();

            // Создание экземпляра AutoFaker для AdkuEntity
            // Генерация списка сущностей
            var entities = faker.Generate(count).Select(entity =>
            {
                entity.Value = valueCounter++;
                return entity;
            }).ToList();

            // Генерация списка сущностей
            return entities;
        }
    }

}
