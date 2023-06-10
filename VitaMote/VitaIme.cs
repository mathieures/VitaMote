// Service transforming the converted keypresses into actual Android keypresses used in any process

using System;
using System.Net.Sockets;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace VitaMote
{
    [Service(Label = "PSVita", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [MetaData(name: "android.view.im", Resource = "@xml/method")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    public class VitaIme : InputMethodService
    {
        VitaConnection connection;

        public override void OnCreate()
        {
            base.OnCreate();

            // Prepare the connection to the PSVita
            connection = VitaConnection.Instance;
        }

        public override View OnCreateInputView()
        {
            var keyboardView = LayoutInflater.Inflate(Resource.Layout.ime_view, null);
            return keyboardView;
        }

        // Called every time an application starts listening for input (click in an input field for example)
        public override async void OnStartInput(EditorInfo attribute, bool restarting)
        {
            base.OnStartInput(attribute, restarting);

            // Check the connection status of the PSVita
            var connected = await connection.ConnectAsync();
            if (connected != ConnectionStatus.Connected)
            {
                Toast.MakeText(this, $"Couldn't connect to IP '{connection.Settings.IP}' and port '{connection.Settings.Port}'", ToastLength.Long).Show();
                return;
            }
            Toast.MakeText(this, "PSVita connected", ToastLength.Short).Show();

            // Start listening for packets
            try
            {
                while (connection.ConnectionStatus == ConnectionStatus.Connected)
                {
                    var ic = CurrentInputConnection;
                    var keyStates = await connection.UpdateAsync();

                    // For each key-value pair, if the key is true, send the associated key to the input connection
                    foreach (var kvp in keyStates)
                    {
                        if (kvp.Value)
                            ic.SendKeyEvent(new KeyEvent(KeyEventActions.Down, kvp.Key));
                        else
                            ic.SendKeyEvent(new KeyEvent(KeyEventActions.Up, kvp.Key));
                    }
                }
            }
            catch (Exception ex) when (
                    ex is SocketException
                    || ex is System.IO.IOException)
            {
                Toast.MakeText(this, "PSVita disconnected", ToastLength.Long).Show();
                Log.Error("Exception: ", ex.ToString());
            }
        }
    }
}