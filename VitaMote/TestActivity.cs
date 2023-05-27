// Activity that will act as a network tester, to see if keys (or anything, really) are received from the PSVita

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace VitaMote
{
    [Activity(Label = "Network tester")]
    public class TestActivity: Activity
    {
        TcpClient tcpClient;
        TextView connectionStatusText;
        TextView displayText;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.test_network_activity);

            connectionStatusText = FindViewById<TextView>(Resource.Id.connectionStatus);
            displayText = FindViewById<TextView>(Resource.Id.displayText);

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
            int[] receivedInts = new int[8]; // Keycodes cannot be compared to bytes

            while (true)
            {
                try
                {
                    await Task.Delay(100); // TODO: check if this is necessary

                    // Request the PSVita for new input
                    RequestInput(stream);

                    // Read 8 bytes from the stream
                    stream.Read(receivedBytes, 0, 8);

                    // Convert them to ints
                    for (int i = 0; i < 8; i++)
                        receivedInts[i] = (int)receivedBytes[i];

                    ParseInts(receivedInts);

                    string convertedBytes = "";
                    foreach (var b in receivedBytes)
                    {
                        convertedBytes += b.ToString() + " ";
                    }
                    Console.WriteLine($"Received : {receivedBytes.Length} bytes: {convertedBytes}");
                    displayText.Text = convertedBytes;
                }
                catch (SocketException ex)
                {
                    Toast.MakeText(this, "PSVita disconnected", ToastLength.Long).Show();
                    Log.Error("Exception: ", ex.ToString());
                    break;
                }
            }
        }

        void ParseInts(int[] ints)
        {
            // Maps all keys to a boolean representing the state of the key:
            // if true, the key is Down (pressed), else it's Up (released)
            var dictionary = new Dictionary<Keycode, bool>
            {
                // DPAD
                { Keycode.DpadLeft, false },
                { Keycode.DpadDown, false },
                { Keycode.DpadRight, false },
                { Keycode.DpadUp, false },
                // Start, Select
                { Keycode.Back, false },
                { Keycode.DpadCenter, false },
                // SXCT
                { Keycode.ButtonX, false },
                { Keycode.ButtonA, false },
                { Keycode.ButtonB, false },
                { Keycode.ButtonY, false },
                // L-R Triggers
                { Keycode.ButtonR1, false },
                { Keycode.ButtonL1, false },
                // Analog sticks
                // LEFT
                { Keycode.W, false },
                { Keycode.S, false },
                { Keycode.A, false },
                { Keycode.D, false },
                // RIGHT
                { Keycode.I, false },
                { Keycode.K, false },
                { Keycode.J, false },
                { Keycode.L, false }
            };

            // First int takes care of DPAD, Start, Select
            switch (ints[0])
            {
                case ButtonHelper.btnL:
                    dictionary[ButtonHelper.bLe] = true;
                    break;
                case ButtonHelper.btnD:
                    dictionary[ButtonHelper.bDo] = true;
                    break;
                case ButtonHelper.btnR:
                    dictionary[ButtonHelper.bRi] = true;
                    break;
                case ButtonHelper.btnU:
                    dictionary[ButtonHelper.bUp] = true;
                    break;
                case ButtonHelper.btnSta:
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                case ButtonHelper.btnSel:
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Double combos (DPAD, Start, Select)
                // Left + Up
                case ButtonHelper.btnLU:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    break;
                // Left + Down
                case ButtonHelper.btnLD:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bDo] = true;
                    break;
                // Right + Up
                case ButtonHelper.btnRU:
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    break;
                // Down + Right
                case ButtonHelper.btnDR:
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bRi] = true;
                    break;
                // Up + Start
                case ButtonHelper.btnUSt:
                    dictionary[ButtonHelper.bUp] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Up + Select
                case ButtonHelper.btnUSe:
                    dictionary[ButtonHelper.bSe] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    break;
                // Down + Start
                case ButtonHelper.btnDSt:
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Down + Select
                case ButtonHelper.btnDSe:
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Left + Start
                case ButtonHelper.btnLSt:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Left + Select
                case ButtonHelper.btnLSe:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Right + Start
                case ButtonHelper.btnRSt:
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Right + Select
                case ButtonHelper.btnRSe:
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Start + Select
                case ButtonHelper.btnStSe:
                    dictionary[ButtonHelper.bSt] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Triple combos (DPAD, Start, Select)
                // Left + Up + Start
                case ButtonHelper.btnLUSt:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    dictionary[ButtonHelper.bSt] = true;
                        break;
                // Left + Up + Select
                case ButtonHelper.btnLUSe:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Left + Down + Start
                case ButtonHelper.btnLDSt:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Left + Down + Select
                case ButtonHelper.btnLDSe:
                    dictionary[ButtonHelper.bLe] = true;
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Right + Up + Start
                case ButtonHelper.btnRUSt:
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Right + Up + Select
                case ButtonHelper.btnRUSe:
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bUp] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                // Right + Down + Start
                case ButtonHelper.btnDRSt:
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bSt] = true;
                    break;
                // Right + Down + Select
                case ButtonHelper.btnDRSe:
                    dictionary[ButtonHelper.bDo] = true;
                    dictionary[ButtonHelper.bRi] = true;
                    dictionary[ButtonHelper.bSe] = true;
                    break;
                default:
                    throw new Exception(
                        "If this happens, it means not all the codes are listed in" +
                        "ButtonHelper (or there was an error in the received packet)"
                    );
            }

            // Second int takes care of SXCT, L trigger, R trigger
            switch (ints[1])
            {
                case ButtonHelper.btnS:
                    dictionary[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnX:
                    dictionary[ButtonHelper.bX] = true;
                    break;
                case ButtonHelper.btnC:
                    dictionary[ButtonHelper.bC] = true;
                    break;
                case ButtonHelper.btnT:
                    dictionary[ButtonHelper.bT] = true;
                    break;
                case ButtonHelper.btnRt:
                    dictionary[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnLt:
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                // Double combos (SXCT, L, R)
                case ButtonHelper.btnXC:
                    dictionary[ButtonHelper.bX] = true;
                    dictionary[ButtonHelper.bC] = true;
                    break;
                case ButtonHelper.btnXS:
                    dictionary[ButtonHelper.bX] = true;
                    dictionary[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnCT:
                    dictionary[ButtonHelper.bC] = true;
                    dictionary[ButtonHelper.bT] = true;
                    break;
                case ButtonHelper.btnTS:
                    dictionary[ButtonHelper.bT] = true;
                    dictionary[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnXLt:
                    dictionary[ButtonHelper.bX] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnXRt:
                    dictionary[ButtonHelper.bX] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnCLt:
                    dictionary[ButtonHelper.bC] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnCRt:
                    dictionary[ButtonHelper.bC] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnTLt:
                    dictionary[ButtonHelper.bT] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnTRt:
                    dictionary[ButtonHelper.bT] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnSLt:
                    dictionary[ButtonHelper.bS] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnSRt:
                    dictionary[ButtonHelper.bS] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRt:
                    dictionary[ButtonHelper.bLt] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                // Triple combos (SXCT, L, R)
                case ButtonHelper.btnLtRtC:
                    dictionary[ButtonHelper.bC] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtT:
                    dictionary[ButtonHelper.bT] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtS:
                    dictionary[ButtonHelper.bS] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtX:
                    dictionary[ButtonHelper.bX] = true;
                    dictionary[ButtonHelper.bLt] = true;
                    dictionary[ButtonHelper.bRt] = true;
                    break;
                default:
                    throw new Exception(
                        "If this happens, it means not all the codes are listed in" +
                        "ButtonHelper (or there was an error in the received packet)"
                    );
            }

            // Third int is not used
            // Fourth int is not used

            // The fifth int and half of the sixth take care of the left analog stick
            if (0 < ints[4] && ints[4] <= 50)
                dictionary[ButtonHelper.aLl] = true;

            if (200 < ints[4] && ints[4] <= 255)
                dictionary[ButtonHelper.aLr] = true;

            if (0 < ints[5] && ints[5] <= 50)
                dictionary[ButtonHelper.aLu] = true;

            if (200 < ints[5] && ints[5] <= 255)
                dictionary[ButtonHelper.aLd] = true;

            // Half of the sixth int and the seventh take care of the right analog stick
            if (0 < ints[6] && ints[6] <= 50)
                dictionary[ButtonHelper.aRl] = true;

            if (200 < ints[6] && ints[6] <= 255)
                dictionary[ButtonHelper.aRr] = true;

            if (0 < ints[6] && ints[7] <= 50)
                dictionary[ButtonHelper.aRu] = true;

            if (200 < ints[7] && ints[7] <= 255)
                dictionary[ButtonHelper.aRd] = true;

            // TODO: draw conclusions
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