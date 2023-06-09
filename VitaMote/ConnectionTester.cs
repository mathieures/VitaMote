// Activity acting as a connection tester, displaying the keys pressed on the PSVita

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Xamarin.Essentials;

namespace VitaMote
{
    [Activity(Label = "Network tester")]
    public class ConnectionTester : Activity
    {
        bool running = false;

        VitaConnection _connection = new VitaConnection();

        TextView connectionStatusText;
        TextView displayText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.connection_tester);

            connectionStatusText = FindViewById<TextView>(Resource.Id.connectionStatus);
            displayText = FindViewById<TextView>(Resource.Id.displayText);
        }

        protected override async void OnStart()
        {
            base.OnStart();
            running = true;

            string ip = Preferences.Get("ip", null);
            int port = 5000;

            // Change the port in case it is specified
            if (ip.Contains(':'))
            {
                port = int.Parse(ip.Split(':')[1]);
                ip = ip.Split(':')[0];
            }

            // Connect the TCP client

            // Show connecting message
            connectionStatusText.Text = Resources.GetString(Resource.String.connecting);
            displayText.Text = connectionStatusText.Text;

            try
            {
                await _connection.Init(ip, port);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Couldn't connect to IP '{ip}' and port '{port}'", ToastLength.Long).Show();
                Log.Error("TryConnectionAsync", ex.ToString());

                Finish();
                return;
            }

            // Change the connection status
            connectionStatusText.Text = Resources.GetString(Resource.String.connected);
            displayText.Text = Resources.GetString(Resource.String.pressButton);

            // Start listening
            await RunAsync();
        }
        protected override void OnStop()
        {
            Log.Info("On stop", "running = false");
            base.OnStop();
            running = false;
        }
        async Task RunAsync()
        {
            try
            {
                // TODO: find a stop condition
                while (running)
                {
                    var keyStates = await _connection.UpdateAsync();
                    // Display the text on the screen
                    var sb = new StringBuilder();

                    // For each key-value pair, if the key is true, add the Keycode to the display text
                    foreach (var kvp in keyStates)
                    {
                        if (kvp.Value)
                            sb.AppendLine(kvp.Key.ToString());
                    }
                    displayText.Text = sb.ToString();
                    if (displayText.Text != "")
                        Log.Info("Pressed:", displayText.Text);
                }
            }
            catch (Exception ex) when (
                    ex is SocketException
                    || ex is IOException)
            {
                Toast.MakeText(this, "PSVita disconnected", ToastLength.Long).Show();
                Log.Error("Exception: ", ex.ToString());
                Finish();
            }
        }
    }
}