using EventBus.Base.Abstraction;
using EventBus.XUnitTest.Events.Events;

namespace EventBus.XUnitTest.Events.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        public Task Handle(OrderCreatedIntegrationEvent @event)
        {
            //

            return Task.CompletedTask;
        }
    }
}
