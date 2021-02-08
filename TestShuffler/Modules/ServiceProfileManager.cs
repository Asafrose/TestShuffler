using System.Threading.Tasks;
using MongoDB.Driver;

namespace TestShuffler
{
    public interface IServiceProfileManager
    {
        Task<ServiceProfile> GetServiceProfileAsync();
    }

    public class ServiceProfileManager : Module, IServiceProfileManager
    {
        private readonly IDatabase _database;

        private ServiceProfile _serviceProfile;

        public ServiceProfileManager(IDatabase database) =>
            _database = database;

        public override async ValueTask DisposeAsync()
        {
            await _database.DocumentsCollection.ReplaceOneAsync(
                _ => _.Id == nameof(ServiceProfile),
                _serviceProfile,
                new ReplaceOptions
                {
                    IsUpsert = true
                },
                CancellationToken);

            await base.DisposeAsync();
        }

        public async Task<ServiceProfile> GetServiceProfileAsync()
        {
            if (_serviceProfile != null)
            {
                return _serviceProfile;
            }

            _serviceProfile =
                (ServiceProfile)(await _database.
                    DocumentsCollection.
                    FindAsync(_ => _.Id == nameof(ServiceProfile), cancellationToken: CancellationToken)).
                    SingleOrDefault(CancellationToken) ??
                new ServiceProfile();

            return _serviceProfile;
        }
    }
}