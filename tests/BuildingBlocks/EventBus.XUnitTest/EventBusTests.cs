using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.XUnitTest.Events.EventHandlers;
using EventBus.XUnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.XUnitTest
{
    public class EventBusTests
    {
        private ServiceCollection services;

        public EventBusTests()
        {
            this.services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }


        
        public void subscribe_event_on_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(e =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    SubscriberClientAppName = "EventBus.UnitTest",
                    DefaultTopicName = "SvsBusAppointmentTopicName",
                    EventBusType = EventBusType.RabbitMQ,
                    EventNamePrefix = "IntegrationEvent",
                    //Connection = new ConnectionFactory()
                    //{
                    //    HostName = "localhost",
                    //    Port = 5672,
                    //    UserName = "guest",
                    //    Password = "guest"
                    //};
                };
                return EventBusFactory.Create(config, e);
            });

            var sp = services.BuildServiceProvider();

            var eventbus = sp.GetRequiredService<IEventBus>();

            eventbus.Subscribe<OrderCreatedIntegrationEvent,OrderCreatedIntegrationEventHandler>();


        }
    }
}
