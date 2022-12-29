using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBus.UnitTest
{
    [TestClass]
    public class EventBusTests
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
