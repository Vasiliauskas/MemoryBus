using MemoryBus;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MemoryBusDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IBus _bus = new MemoryBus.MemoryBus(new DefaultConfig());
        public MainWindow()
        {
            InitializeComponent();
            _bus.Subscribe<string>(ProcessMessage);
            _bus.SubscribeAsync<string>(s =>
            {
                ProcessMessage($"From async > 3: {s}");
                return Task.FromResult(0);
            }, s => s.Length > 3);

            _bus.SubscribeAsync<string>(s =>
            {
                ProcessMessage($"From async: {s}");
                return Task.FromResult(0);
            });

            _bus.RespondAsync<string, string>(s => Task.FromResult("response: " + s));
            _bus.Respond<int, string>(s => "response: " + s.ToString());
            int a;
            _bus.Respond<string, int>(s => int.Parse(s), s => int.TryParse(s, out a));
            _bus.StreamRespond<string, string>((s, o) =>
             {
                 Task.Run(() =>
                 {
                     for (int i = 0; i < 5; i++)
                     {
                         o.OnNext(s + i.ToString());
                         Thread.Sleep(200);
                     }
                     o.OnCompleted();
                 });
             });
        }

        private void ProcessMessage(string msg)
            => this.Dispatcher.Invoke(() => Items.Items.Add(msg));

        private void Publish(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this.Input.Text);
        }

        private void PublishAsync(object sender, RoutedEventArgs e)
        {
            _bus.PublishAsync(this.Input.Text);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            this.Items.Items.Clear();
        }

        private void Request(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request: {_bus.Request<string, string>(this.Result.Text)}");
        }

        private void RequestInt(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request: {_bus.Request<string, int>(this.Result.Text)}");
        }

        private async void RequestAsync(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request async: {await _bus.RequestAsync<string, string>(this.Result.Text)}");
        }

        private void RequestAsyncWait(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request async wait: {_bus.RequestAsync<string, string>(this.Result.Text).Result}");
        }

        private void StreamRequest(object sender, RoutedEventArgs e)
        {
            var msg = this.Result.Text;
            _bus.StreamRequest<string, string>(msg, ProcessMessage, () => ProcessMessage($"Stream {msg} done"));
            ProcessMessage($"Stream {_bus.RequestAsync<string, string>(this.Result.Text).Result}");
        }

        private void RequestResponder(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request responder: {_bus.Request<int, string>(int.Parse(this.Result2.Text))}");
        }

        private async void RequestAsyncResponder(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request responder async: {await _bus.RequestAsync<int, string>(int.Parse(this.Result2.Text))}");
        }

        private void RequestAsyncWaitResponder(object sender, RoutedEventArgs e)
        {
            ProcessMessage($"Request responder async wait: {_bus.RequestAsync<int, string>(int.Parse(this.Result2.Text)).Result}");
        }
    }
}
