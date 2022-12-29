using EventBus.Base.Events;

namespace EventBus.XUnitTest.Events.Events
{
    public class OrderCreatedIntegrationEvent:IntegrationEvent
    {
        public OrderCreatedIntegrationEvent()
        {
            
        }

        public OrderCreatedIntegrationEvent(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
