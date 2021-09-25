// See https://aka.ms/new-console-template for more information
using SLGatewayClient;

Console.WriteLine("Hello, World!");

_ = Task.Run(Run);

Console.ReadKey();

async void Run()
{
    var gateway = new SLGateway(new SLGatewayConfiguration
    {
        ApiKey = "SLGAPI-Babavjnxb_oWjHzasurN",
        GatewayUrl = "https://slgateway.herokuapp.com"
    });

    var obj = gateway.UseObject(new Guid("0876d5ba-1488-9e79-544b-10495adc2731"));

    var owner = await obj.GetOwnerAsync();

    Console.WriteLine("My owner is: " + owner);

    var people = await obj.GetAgentListAsync(AgentListScope.Parcel);

    Console.WriteLine(string.Join(" ", people));
}