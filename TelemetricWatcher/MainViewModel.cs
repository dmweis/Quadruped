using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelemetricWatcher.Servo;

namespace TelemetricWatcher
{
    class MainViewModel : ViewModelBase, IDisposable
    {
        public ObservableCollection<ServoViewModel> Servos { get; } = new ObservableCollection<ServoViewModel>();

        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;
        private readonly Timer _timeoutTImer = new Timer();

        public MainViewModel()
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("DynamixelTelemetrics", "fanout");
            string queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, "DynamixelTelemetrics", string.Empty);
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += ConsumerOnReceived;
            _channel.BasicConsume(queueName, true, _consumer);
            // initialize timer
            _timeoutTImer.Interval = 5 * 1000;
            _timeoutTImer.AutoReset = true;
            _timeoutTImer.Elapsed += TimeoutTImerOnElapsed;
            _timeoutTImer.Start();
        }

        private void TimeoutTImerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                List<ServoViewModel> old = new List<ServoViewModel>();
                foreach (var servo in Servos)
                {
                    if (servo.IsOld())
                    {
                        old.Add(servo);
                    }
                }
                foreach (var servo in old)
                {
                    Servos.Remove(servo);
                }
            });
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            string json = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);
            ICollection<ServoTelemetrics> telemetrics = ServoTelemetrics.DeserealizeCollection(json);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                foreach (var servoTelemetricse in telemetrics)
                {
                    ServoViewModel servoVM = Servos.FirstOrDefault(servo => servo.Id == servoTelemetricse.Id);
                    if (servoVM == null)
                    {
                        servoVM = new ServoViewModel(servoTelemetricse.Id);
                        Servos.Add(servoVM);
                    }
                    servoVM.Update(servoTelemetricse);
                }
            });
        }

        public void Dispose()
        {
            // close
            _channel?.Close();
            _connection?.Close();
            // dispose
            _channel?.Dispose();
            _connection?.Dispose();
            _timeoutTImer?.Dispose();
        }
    }
}
