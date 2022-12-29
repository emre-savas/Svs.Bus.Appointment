using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBus.TestProject
{
    public class Class1
    {
        private ServiceCollection services;

        public EventBusTests()
        {
            this.services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }


        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {

        }
    }
}