using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace ClientUI
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");

            //LearningTransport
            // var transport = endpointConfiguration.UseTransport<LearningTransport>();
            //var routing = transport.Routing();
            //routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");
            //var routing = transport.Routing();
            //routing.RouteToEndpoint(
            //    assembly: typeof(PlaceOrder).Assembly,
            //    destination: "Sales");

            //routing.RegisterPublisher(
            //     assembly: typeof(OrderPlaced).Assembly,
            //     publisherEndpoint: "Sales");

            //persistence
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


            //RabitMQ
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host= localhost:5672;username=guest;password=guest");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableOutbox();

            var routing = transport.Routing();
            routing.RouteToEndpoint(
            assembly: typeof(PlaceOrder).Assembly,
            destination: "Sales");



            var endpointInstance = await Endpoint.Start(endpointConfiguration)
          .ConfigureAwait(false);

          
            await RunLoop(endpointInstance)
                .ConfigureAwait(false);

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        static ILog log = LogManager.GetLogger<Program>();

        static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            while (true)
            {
                log.Info("Press 'P' to place an order, or 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new PlaceOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command
                        log.Info($"Sending PlaceOrder command, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command)
                            .ConfigureAwait(false);

                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
    }

}
