using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; set; }
        [JsonProperty]
        public DateTime CreatedDate { get; set; }

        [JsonConstructor]
        public IntegrationEvent()
        {
            Id=Guid.NewGuid();
            CreatedDate = DateTime.Now;
        }

        public IntegrationEvent(Guid id, DateTime createdDate)
        {
            Id = id;
            CreatedDate = createdDate;
        }
    }
}
