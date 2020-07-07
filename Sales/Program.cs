using Messages;
using NServiceBus;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Sales
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Sales";

            var endpointConfiguration = new EndpointConfiguration("Sales");
            endpointConfiguration.EnableOutbox();
            var connection = "Server = C1; Database = Persistance ;Trusted_Connection=True; ";
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.CacheFor(TimeSpan.FromMinutes(1));
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(connection);
                });

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host= localhost:5672;username=guest;password=guest");
            endpointConfiguration.EnableInstallers();
           

            var routing = transport.Routing();
            routing.RouteToEndpoint(
            assembly: typeof(OrderPlaced).Assembly,
            destination: "Billing");

            //assemblyScaner
            //var scanner = endpointConfiguration.AssemblyScanner();
            //scanner.ExcludeAssemblies("Sales");

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
               .ConfigureAwait(false);


            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
