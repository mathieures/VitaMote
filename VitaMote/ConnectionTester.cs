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

namespace VitaMote
{
    [Activity(Label = "Network tester")]
    public class ConnectionTester : Activity
    {
        bool running = false;

        VitaConnection connection;

        TextView connectionStatusText;
        TextView displayText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.connection_tester);

            connection = VitaConnection.Instance;

            connectionStatusText = FindViewById<TextView>(Resource.Id.connectionStatus);
            displayText = FindViewById<TextView>(Resource.Id.displayText);
        }

        protected override async void OnStart()
        {
            base.OnStart();
            running = true;

            // Show connecting message
            connectionStatusText.Text = Resources.GetString(Resource.String.connecting);
            displayText.Text = connectionStatusText.Text;

            // Connect the TCP client

            // If the PSVita is not connected, try to connect. If it fails, stop the activity.
            if (connection.ConnectionStatus != ConnectionStatus.Connected
                && await connection.ConnectAsync() != ConnectionStatus.Connected)
            {
                Toast.MakeText(this, $"Couldn't connect to IP '{connection.Settings.IP}' and port '{connection.Settings.Port}'", ToastLength.Long).Show();
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
            base.OnStop();
            running = false;
        }

        async Task RunAsync()
        {
            try
            {
                while (running)
                {
                    var keyStates = await connection.UpdateAsync();
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