using KasaAPI;
using System.Collections.Concurrent;

namespace KasaRESTAPI
{
    public class DiscoveryService : BackgroundService
    {
        private readonly ConcurrentDictionary<string, KasaDevice> devices;
        public DiscoveryService(ConcurrentDictionary<string, KasaDevice> devices)
        {
            this.devices = devices;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                KasaDevice.Discovery(devices);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            return;
        }
    }
}
