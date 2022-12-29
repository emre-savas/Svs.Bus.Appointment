using System.Net;
using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        private IConnectionFactory connectionFactory;
        private IConnection _connection;
        private object lock_object=new object();
        private int _retryCount;
        private bool _disposed;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount)
        {
            this.connectionFactory=connectionFactory;
            this._retryCount = retryCount;
        }

        public bool IsConnection => _connection != null && _connection.IsOpen;

        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            _disposed=true;
            _connection.Dispose();
        }

        public bool TryConnect()
        {
            lock (lock_object)
            {
                var policy= Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                        (ex, time) =>
                        {

                        }
                    );

                policy.Execute(() =>
                {
                    _connection = connectionFactory.CreateConnection();
                });
            }

            if (IsConnection)
            {
                _connection.ConnectionShutdown += Connection_ConnectionShutdown;
                _connection.CallbackException += Connection_CallbackException;
                _connection.ConnectionBlocked += Connection_ConnectionBlocked;


                //log
                return true;
            }
            return false;
        }

        private void Connection_ConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            if(_disposed)return;
            TryConnect();
        }

        private void Connection_CallbackException(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            //log

            TryConnect();

        }
    }
}
