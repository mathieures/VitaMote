using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.Util;
using Android.Views;
using Xamarin.Essentials;

namespace VitaMote
{
    public enum ConnectionStatus
    {
        Online,      // The PSVita can be pinged, but cannot receive or send data packets
        Unreachable, // The PSVita cannot be pinged
        Connected    // The PSVita can receive and send data packets
    }

    // Class representing the saved settings at the time it is instantiated
    public class SavedSettings
    {
        public string IP { get; }
        public int Port { get; }

        public SavedSettings()
        {
            IP = Preferences.Get("ip", null);
            Port = 5000;
            // Change the port in case it is specified
            if (!string.IsNullOrEmpty(IP) && IP.Contains(':'))
            {
                Port = int.Parse(IP.Split(':')[1]);
                IP = IP.Split(':')[0];
            }
        }
    }
    class DataPacket
    {
        public const int size = 16; // 16 bytes in total // je crois, s’il y a du padding // je suis assez confiant que 16 est correct

        public UInt32 buttons; // 4 bytes
        public byte lx; // 1 byte
        public byte ly; // 1 byte
        public byte rx; // 1 byte
        public byte ry; // 1 byte
        public UInt16 tx; // 2 bytes
        public UInt16 ty; // 2 bytes
        public UInt32 click; // 4 bytes // je crois, s’il y a du padding

        public DataPacket() { }
        public DataPacket(byte[] rawData)
        {
            buttons = BitConverter.ToUInt32(rawData[0..4], 0);

            lx = rawData[4];
            ly = rawData[5];
            rx = rawData[6];
            ry = rawData[7];

            tx = BitConverter.ToUInt16(rawData[8..10], 0);
            ty = BitConverter.ToUInt16(rawData[10..12], 0);

            click = BitConverter.ToUInt32(rawData[11..], 0);
        }
    }

    // Singleton class representing the network connection with the PSVita (started or not)
    public sealed class VitaConnection
    {
        private static VitaConnection instance = null;

        private VitaConnection()
        {
            settings = new SavedSettings();
        }

        public static VitaConnection Instance
        {
            get
            {
                instance ??= new VitaConnection();
                return instance;
            }
        }
        public SavedSettings Settings => settings;

        public ConnectionStatus ConnectionStatus => connectionStatus;

        SavedSettings settings; // Contains the SavedSettings used for the last call at ConnectAsync()
        ConnectionStatus connectionStatus;

        TcpClient tcpClient;
        NetworkStream stream;

        // Try to connect to the PSVita and return the connection status
        public async Task<ConnectionStatus> ConnectAsync()
        {
            using Ping ping = new Ping();
            settings = new SavedSettings(); // Apply the most recent saved settings

            PingReply reply = await ping.SendPingAsync(settings.IP);
            Log.Info("Ping", $"Ping status for ({settings.IP}): {reply.Status}");

            // If the ping failed, we are not connected
            if (!(reply is { Status: IPStatus.Success }))
            {
                Log.Error("ConnectAsync", $"{settings.IP}:{settings.Port} is unreachable");
                connectionStatus = ConnectionStatus.Unreachable;
            }
            // If the ping is successful and we are not connected, do so
            else if (connectionStatus != ConnectionStatus.Connected)
            {
                Log.Info("Ping", $"Address: {reply.Address}");
                Log.Info("Ping", $"Roundtrip time: {reply.RoundtripTime}");
                Log.Info("Ping", $"Time to live: {reply.Options?.Ttl}");

                // Actually connect to the PSVita
                try
                {
                    tcpClient = new TcpClient(settings.IP, settings.Port);
                    stream = tcpClient.GetStream();
                    connectionStatus = ConnectionStatus.Connected;
                }
                catch (SocketException)
                {
                    tcpClient?.Close();
                    Log.Error("ConnectAsync", $"Connection refused to {settings.IP}:{settings.Port}");
                    connectionStatus = ConnectionStatus.Online;
                }
            }
            // Else, we are already connected
            return connectionStatus;
        }

        public async Task<Dictionary<Keycode, bool>> UpdateAsync()
        {
            byte[] receivedBytes = new byte[DataPacket.size];
            DataPacket data;

            // Maps all keys to a boolean representing the state of the key:
            // if true, the key is Down (pressed), else it's Up (released)
            var keyStates = new Dictionary<Keycode, bool>
            {
                // DPAD
                { Keycode.DpadLeft, false },
                { Keycode.DpadDown, false },
                { Keycode.DpadRight, false },
                { Keycode.DpadUp, false },
                // Start, Select
                { Keycode.Back, false },
                { Keycode.DpadCenter, false },
                // TCXS
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

            // TODO: check if a delay is necessary (I don't think so)
            //await Task.Delay(100);
            //await Task.Delay(50);

            // Request the PSVita for new input
            await RequestInputAsync(stream);

            // Read the data and convert it into a DataPacket
            // TODO: fix the lag if possible
            await stream.ReadAsync(receivedBytes, 0, receivedBytes.Length);
            //stream.Read(receivedBytes, 0, receivedBytes.Length);

            data = new DataPacket(receivedBytes);

            ParseButtons(data.buttons, keyStates);

            // TODO: parse the rest

            return keyStates;
        }

        void ParseButtons(UInt32 data, Dictionary<Keycode, bool> keyStates)
        {
            // SEL-STA
            if ((data & ButtonHelper.SCE_CTRL_SELECT) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_SELECT)] = true;
            if ((data & ButtonHelper.SCE_CTRL_START) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_START)] = true;
            // DPAD
            if ((data & ButtonHelper.SCE_CTRL_UP) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_UP)] = true;
            if ((data & ButtonHelper.SCE_CTRL_RIGHT) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_RIGHT)] = true;
            if ((data & ButtonHelper.SCE_CTRL_DOWN) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_DOWN)] = true;
            if ((data & ButtonHelper.SCE_CTRL_LEFT) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_LEFT)] = true;
            // L-R Triggers
            if ((data & ButtonHelper.SCE_CTRL_LTRIGGER) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_LTRIGGER)] = true;
            if ((data & ButtonHelper.SCE_CTRL_RTRIGGER) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_RTRIGGER)] = true;
            // TCXS
            if ((data & ButtonHelper.SCE_CTRL_TRIANGLE) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_TRIANGLE)] = true;
            if ((data & ButtonHelper.SCE_CTRL_CIRCLE) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_CIRCLE)] = true;
            if ((data & ButtonHelper.SCE_CTRL_CROSS) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_CROSS)] = true;
            if ((data & ButtonHelper.SCE_CTRL_SQUARE) != 0)
                keyStates[ButtonHelper.ButtonToKeycode(ButtonHelper.SCE_CTRL_SQUARE)] = true;



            // Commented for now to test bitwise operations

            /*
            // First int takes care of DPAD, Start, Select
            switch (ints[0])
            {
                case ButtonHelper.btnL:
                    keyStates[ButtonHelper.bLe] = true;
                    break;
                case ButtonHelper.btnD:
                    keyStates[ButtonHelper.bDo] = true;
                    break;
                case ButtonHelper.btnR:
                    keyStates[ButtonHelper.bRi] = true;
                    break;
                case ButtonHelper.btnU:
                    keyStates[ButtonHelper.bUp] = true;
                    break;
                case ButtonHelper.btnSta:
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                case ButtonHelper.btnSel:
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Double combos (DPAD, Start, Select)
                // Left + Up
                case ButtonHelper.btnLU:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    break;
                // Left + Down
                case ButtonHelper.btnLD:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bDo] = true;
                    break;
                // Right + Up
                case ButtonHelper.btnRU:
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    break;
                // Down + Right
                case ButtonHelper.btnDR:
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bRi] = true;
                    break;
                // Up + Start
                case ButtonHelper.btnUSt:
                    keyStates[ButtonHelper.bUp] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Up + Select
                case ButtonHelper.btnUSe:
                    keyStates[ButtonHelper.bSe] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    break;
                // Down + Start
                case ButtonHelper.btnDSt:
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Down + Select
                case ButtonHelper.btnDSe:
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Left + Start
                case ButtonHelper.btnLSt:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Left + Select
                case ButtonHelper.btnLSe:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Right + Start
                case ButtonHelper.btnRSt:
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Right + Select
                case ButtonHelper.btnRSe:
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Start + Select
                case ButtonHelper.btnStSe:
                    keyStates[ButtonHelper.bSt] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Triple combos (DPAD, Start, Select)
                // Left + Up + Start
                case ButtonHelper.btnLUSt:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Left + Up + Select
                case ButtonHelper.btnLUSe:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Left + Down + Start
                case ButtonHelper.btnLDSt:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Left + Down + Select
                case ButtonHelper.btnLDSe:
                    keyStates[ButtonHelper.bLe] = true;
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Right + Up + Start
                case ButtonHelper.btnRUSt:
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Right + Up + Select
                case ButtonHelper.btnRUSe:
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bUp] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                // Right + Down + Start
                case ButtonHelper.btnDRSt:
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bSt] = true;
                    break;
                // Right + Down + Select
                case ButtonHelper.btnDRSe:
                    keyStates[ButtonHelper.bDo] = true;
                    keyStates[ButtonHelper.bRi] = true;
                    keyStates[ButtonHelper.bSe] = true;
                    break;
                default:
                    break;
                    //throw new Exception(
                    //    "If this happens, it means not all the codes are listed in" +
                    //    "ButtonHelper (or there was an error in the received packet)"
                    //);
            }

            // Second int takes care of TCXS, L trigger, R trigger
            switch (ints[1])
            {
                case ButtonHelper.btnS:
                    keyStates[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnX:
                    keyStates[ButtonHelper.bX] = true;
                    break;
                case ButtonHelper.btnC:
                    keyStates[ButtonHelper.bC] = true;
                    break;
                case ButtonHelper.btnT:
                    keyStates[ButtonHelper.bT] = true;
                    break;
                case ButtonHelper.btnRt:
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLt:
                    keyStates[ButtonHelper.bLt] = true;
                    break;
                // Double combos (TCXS, L, R)
                case ButtonHelper.btnXC:
                    keyStates[ButtonHelper.bX] = true;
                    keyStates[ButtonHelper.bC] = true;
                    break;
                case ButtonHelper.btnXS:
                    keyStates[ButtonHelper.bX] = true;
                    keyStates[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnCT:
                    keyStates[ButtonHelper.bC] = true;
                    keyStates[ButtonHelper.bT] = true;
                    break;
                case ButtonHelper.btnTS:
                    keyStates[ButtonHelper.bT] = true;
                    keyStates[ButtonHelper.bS] = true;
                    break;
                case ButtonHelper.btnXLt:
                    keyStates[ButtonHelper.bX] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnXRt:
                    keyStates[ButtonHelper.bX] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnCLt:
                    keyStates[ButtonHelper.bC] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnCRt:
                    keyStates[ButtonHelper.bC] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnTLt:
                    keyStates[ButtonHelper.bT] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnTRt:
                    keyStates[ButtonHelper.bT] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnSLt:
                    keyStates[ButtonHelper.bS] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    break;
                case ButtonHelper.btnSRt:
                    keyStates[ButtonHelper.bS] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRt:
                    keyStates[ButtonHelper.bLt] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                // Triple combos (TCXS, L, R)
                case ButtonHelper.btnLtRtC:
                    keyStates[ButtonHelper.bC] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtT:
                    keyStates[ButtonHelper.bT] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtS:
                    keyStates[ButtonHelper.bS] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                case ButtonHelper.btnLtRtX:
                    keyStates[ButtonHelper.bX] = true;
                    keyStates[ButtonHelper.bLt] = true;
                    keyStates[ButtonHelper.bRt] = true;
                    break;
                default:
                    //throw new Exception(
                    //    "If this happens, it means not all the codes are listed in" +
                    //    "ButtonHelper (or there was an error in the received packet)"
                    //);
                    break;
            }

            // Third int is not used
            // Fourth int is not used

            // The fifth int and half of the sixth take care of the left analog stick
            if (0 <= ints[4] && ints[4] <= 50)
                keyStates[ButtonHelper.aLl] = true;

            if (200 <= ints[4] && ints[4] <= 255)
                keyStates[ButtonHelper.aLr] = true;

            if (0 <= ints[5] && ints[5] <= 50)
                keyStates[ButtonHelper.aLu] = true;

            if (200 <= ints[5] && ints[5] <= 255)
                keyStates[ButtonHelper.aLd] = true;

            // Half of the sixth int and the seventh take care of the right analog stick
            if (0 <= ints[6] && ints[6] <= 50)
                keyStates[ButtonHelper.aRl] = true;

            if (200 <= ints[6] && ints[6] <= 255)
                keyStates[ButtonHelper.aRr] = true;

            if (0 <= ints[6] && ints[7] <= 50)
                keyStates[ButtonHelper.aRu] = true;

            if (200 <= ints[7] && ints[7] <= 255)
                keyStates[ButtonHelper.aRd] = true;

            */
        }

        void ParseSticks(UInt32 data, Dictionary<Keycode, bool> keyStates) { }

        void ParseClicks(UInt32 data, Dictionary<Keycode, bool> keyStates) { }

        private readonly byte[] _requestBytes = System.Text.Encoding.ASCII.GetBytes("request");

        // Send "request" to the PSVita to ask for input (any 7 bytes should work, since they are discarded)
        async Task RequestInputAsync(NetworkStream stream)
        {
            await stream.WriteAsync(_requestBytes, 0, _requestBytes.Length);
            await stream.FlushAsync();
        }
    }
}