using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        private RabbitMQPersistentConnection _persistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IModel _consumerChannel;


        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            if (config.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(EventBusConfig.Connection, new JsonSerializerSettings()
                {
                    //self referencing loop detected for property
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);

            }
            else
            {
                connectionFactory = new ConnectionFactory();
            }

            _persistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount);

            _consumerChannel = CreateConsumerChannel();

            SubsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object? sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!_persistentConnection.IsConnection)
            {
                _persistentConnection.TryConnect();
            }

            //using var channel = _persistentConnection.CreateModel();

            _consumerChannel.QueueUnbind(queue: eventName,
                exchange: EventBusConfig.DefaultTopicName,
                routingKey: eventName);

            if (SubsManager.IsEmpty)
            {
                _consumerChannel.Close();
            }

        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnection)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {

                    });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            _consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = _consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2;//persistent

                _consumerChannel.QueueDeclare(queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _consumerChannel.BasicPublish(
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

            });

        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubsManager.HasSubscriptionsForEvent(eventName))
            {
                if (!_persistentConnection.IsConnection)
                {
                    _persistentConnection.TryConnect();
                }

                //ensure queue exists while consuming
                _consumerChannel.QueueDeclare(queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _consumerChannel.QueueBind(queue: GetSubName(eventName),
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName);
            }

            SubsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            SubsManager.RemoveSubscription<T, TH>();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnection)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName,
                type: "direct");

            return channel;
        }

        private void StartBasedConsume(string eventName)
        {
            if (_consumerChannel != null)
            {
                var consumer = new /*Async*/EventingBasicConsumer(_consumerChannel);

                consumer.Received += Consume_Received;

                _consumerChannel.BasicConsume(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async void Consume_Received(object? sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                //logging
            }

            _consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
        }
    }
}
