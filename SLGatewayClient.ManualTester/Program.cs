// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SLGatewayClient;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<HostedService>();
            });
}

public class HostedService : IHostedService
{
    private readonly ILogger _logger;

    public HostedService(ILogger<HostedService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Trace started");

        //var gateway = new SLGateway(new SLGatewayConfiguration
        //{
        //    ApiKey = "SLGAPI-DKQQBYBoiaPLCSVfYBna",
        //    GatewayUrl = "http://pixelcollider.net",
        //    Logger = _logger
        //});

        //var obj = gateway.UseObject(new Guid("a66b64dd-f82b-555c-4bad-d53592863bdd"));

        //var gateway = new Gateway(new GatewayConfiguration
        //{
        //    ApiKey = "SLGAPI-Babavjnxb_oWjHzasurN",
        //    GatewayUrl = "https://slgateway.herokuapp.com/",
        //    Logger = _logger
        //});

        //var obj = gateway.UseObject(new Guid("0876d5ba-1488-9e79-544b-10495adc2731"));

        //obj.EnableEvents = true;

        //var handle = await obj.ListenAsync(0, "", Guid.Empty, "");

        //if (handle != null)
        //{
        //    Console.WriteLine("Handle is: " + handle.Handle.ToString());
        //}

        //var owner = await obj.GetOwnerAsync();

        //Console.WriteLine("My owner is: " + owner);

        //var people = await obj.GetAgentListAsync(AgentListScope.Parcel);

        //Console.WriteLine(string.Join(" ", people));

        var world = new SLWorld();
        var agent = await world.GetAgentProfile(new Guid("878d0bdd-e2a1-46f4-9c4f-f7314bda7d4e"));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
