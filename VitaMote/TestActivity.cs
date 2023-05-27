// Activity that will act as a network tester, to see if keys (or anything, really) are received from the PSVita

using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Xamarin.Essentials;

namespace VitaMote
{
    [Activity(Label = "Network tester")]
    public class TestActivity: Activity
    {
        TcpClient tcpClient;
        TextView connectionStatusText;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.test_network_activity);

            connectionStatusText = FindViewById<TextView>(Resource.Id.connectionStatus);

            string ip = Preferences.Get("ip", null);
            int port = 5000;

            // Change the port in case it is specified
            if (ip.Contains(':'))
            {
                port = int.Parse(ip.Split(':')[1]);
                ip = ip.Split(':')[0];
            }

            // Connect the TCP client

            connectionStatusText.Text = "Connecting...";
            
            try
            {
                tcpClient = new TcpClient();
                await TryConnectionAsync(tcpClient, ip, port);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Couldn't connect to IP '{ip}' and port '{port}'", ToastLength.Long).Show();
                Console.WriteLine(ex.ToString());
                
                // TODO: figure out what is necessary to cancel showing the page and what is not
                SetResult(Result.Canceled);
                Finish();
                return;
            }

            // Change the connection status
            connectionStatusText.Text = "Connected";

            // Start listening
            await Run();
        }
        async Task TryConnectionAsync(TcpClient tcpClient, string ip, int port)
        {
            using Ping ping = new Ping();

            PingReply reply = await ping.SendPingAsync(ip);
            Console.WriteLine($"Ping status for ({ip}): {reply.Status}");
            if (reply is { Status: IPStatus.Success })
            {
                Console.WriteLine($"Address: {reply.Address}");
                Console.WriteLine($"Roundtrip time: {reply.RoundtripTime}");
                Console.WriteLine($"Time to live: {reply.Options?.Ttl}");
                Console.WriteLine();
            
                // Actually connect to the PSVita
                tcpClient.Connect(ip, port);
            }
            else
            {
                Console.WriteLine("Connection failed");
                throw new Exception("Connection failed");
            }
        }
        async Task Run()
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] receivedBytes = new byte[8];

            while (true)
            {
                try
                {
                    await Task.Delay(100); // TODO: check if this is necessary

                    // Request the PSVita for new input
                    RequestInput(stream);

                    stream.Read(receivedBytes, 0, 8);

                    string convertedBytes = "";
                    foreach (var b in receivedBytes)
                    {
                        convertedBytes += b.ToString() + " ";
                    }
                    Console.WriteLine($"Received : {receivedBytes.Length} bytes: {convertedBytes}");
                }
                catch (System.Exception ex)
                {
                    Log.Info("Exception: ", ex.ToString());
                    break;
                }
            }
        }

        private readonly byte[] _requestBytes = System.Text.Encoding.ASCII.GetBytes("request");

        // Send "request" to the PSVita to ask for input
        void RequestInput(NetworkStream stream)
        {
            stream.Write(_requestBytes, 0, _requestBytes.Length);
            stream.Flush();
        }
    }
}