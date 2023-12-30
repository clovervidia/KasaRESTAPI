using KasaAPI;
using System.Collections.Concurrent;

namespace KasaRESTAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            ConcurrentDictionary<string, KasaDevice> devices = new();
            builder.Services.AddHostedService(discoveryService => new DiscoveryService(devices));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            ConcurrentDictionary<string, (EnergyMonitor energyMonitor, DateTime lastPoll)> energyMonitorCache = new();

            app.MapGet("/api/devices", () => devices.Select(d =>
            {
                var sysInfo = d.Value.GetSystemInfo().System.GetSysInfo;
                return new Device(sysInfo.MAC, sysInfo.DevName);
            }).OrderBy(d => d.MacAddress));

            app.MapGet("/api/{macAddress}/energymonitor", (string macAddress) =>
            {
                if (!devices.TryGetValue(macAddress, out var device))
                {
                    return Results.NotFound(new ErrorResponse($"Couldn't find a device with MAC address: '{macAddress}'"));
                }

                if (!device.EnergyMonitoring)
                {
                    return Results.BadRequest(new ErrorResponse($"The device '{macAddress}' does not support energy monitoring."));
                }

                if (energyMonitorCache.TryGetValue(macAddress, out var cache) && (DateTime.Now - cache.lastPoll <= TimeSpan.FromSeconds(2)))
                {
                    return Results.Ok(cache.energyMonitor);
                }

                var energyMonitorResponse = device.GetRealTimeStats();
                EnergyMonitor energyMonitorStats = new(energyMonitorResponse.Emeter.GetRealTime.VoltageMV / 1000.0,
                                                         energyMonitorResponse.Emeter.GetRealTime.CurrentMA / 1000.0,
                                                         energyMonitorResponse.Emeter.GetRealTime.PowerMW / 1000.0,
                                                         energyMonitorResponse.Emeter.GetRealTime.TotalWH / 1000.0);
                energyMonitorCache[macAddress] = (energyMonitorStats, DateTime.Now);
                return Results.Ok(energyMonitorStats);
            });

            app.MapGet("/api/{macAddress}/outlet", (string macAddress) =>
            {
                if (!devices.TryGetValue(macAddress, out var device))
                {
                    return Results.NotFound(new ErrorResponse($"Couldn't find a device with MAC address: '{macAddress}'"));
                }
                return Results.Ok(new OutletState(device.OutletState));
            });

            app.MapGet("/api/{macAddress}/outlet/{newState}", (string macAddress, string newState) =>
            {
                if (!devices.TryGetValue(macAddress, out var device))
                {
                    return Results.NotFound(new ErrorResponse($"Couldn't find a device with MAC address: '{macAddress}'"));
                }

                switch (newState.ToLower())
                {
                    case "on":
                        device.OutletState = true;
                        return Results.Ok(new OutletState(device.OutletState));
                    case "off":
                        device.OutletState = false;
                        return Results.Ok(new OutletState(device.OutletState));
                    case "toggle":
                        device.ToggleOutlet();
                        return Results.Ok(new OutletState(device.OutletState));
                    default:
                        return Results.BadRequest(new ErrorResponse($"Invalid outlet state: '{newState}'"));
                }
            });

            app.Run();
        }

        record Device(string MacAddress, string Name);
        record ErrorResponse(string Error);
        record EnergyMonitor(double Voltage, double Current, double Power, double Total);
        record OutletState(bool State);
    }
}