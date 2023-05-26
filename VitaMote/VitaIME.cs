using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Android.InputMethodServices.KeyboardView;

namespace VitaMote
{
    [Service(Label = "VitaIME", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [MetaData(name: "android.view.im", Resource = "@xml/method")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    class VitaIME : InputMethodService, IOnKeyboardActionListener
    {
        // Network object used to receive input
        readonly TcpClient clientSocket = new TcpClient();

        private KeyboardView kv;
        private Keyboard keyboard;
        private bool caps = false;
        
        // Connection between the input method and the running application
        IInputConnection ic;

        // Buttons
        int b1, b2, b3, b4, b5, b6, b7, b8;
        // int b1, b2, b3, b4, b5, b6, b7, b8, b9;
        // Type 1
        const int btnL = 128;
        const int btnD = 64;
        const int btnR = 32;
        const int btnU = 16;
        const int btnSta = 8;
        const int btnSel = 1;
        // Combos
        // Dpad Combos
        const int btnLU = btnL + btnU;
        const int btnLD = btnL + btnD;
        const int btnRU = btnR + btnU;
        const int btnRD = btnR + btnD;
        // Dpad + Sel or Sta
        const int btnUSt = btnU + btnSta;
        const int btnUSe = btnU + btnSel;
        const int btnDSt = btnD + btnSta;
        const int btnDSe = btnD + btnSel;
        const int btnLSt = btnL + btnSta;
        const int btnLSe = btnL + btnSel;
        const int btnRSt = btnR + btnSta;
        const int btnRSe = btnR + btnSel;
        const int btnSeSt = btnSel + btnSta;
        // Triple Combo (Dpad Combo + Sta)
        const int btnLUSt = btnLU + btnSta;
        const int btnLUSe = btnLU + btnSel;
        const int btnLDSt = btnLD + btnSta;
        const int btnLDSe = btnLD + btnSel;
        const int btnRUSt = btnRU + btnSta;
        const int btnRUSe = btnRU + btnSel;
        const int btnRDSt = btnRD + btnSta;
        const int btnRDSe = btnRD + btnSel;
        // Type 2
        const int btnS = 128;
        const int btnX = 64;
        const int btnC = 32;
        const int btnT = 16;
        const int btnRt = 2;
        const int btnLt = 1;
        // Double Combo
        const int btnXS = btnX + btnS;
        const int btnXC = btnX + btnC;
        const int btnCT = btnC + btnT;
        const int btnTS = btnT + btnS;
        const int btnXLt = btnX + btnLt;
        const int btnCLt = btnC + btnLt;
        const int btnTLt = btnT + btnLt;
        const int btnSLt = btnS + btnLt;
        const int btnXRt = btnX + btnRt;
        const int btnCRt = btnC + btnRt;
        const int btnTRt = btnT + btnRt;
        const int btnSRt = btnS + btnRt;
        const int btnLR = btnLt + btnRt;
        // Triple Combo (L+R+XTCS)
        const int btnLRC = btnLt + btnRt + btnC;
        const int btnLRS = btnLt + btnRt + btnS;
        const int btnLRT = btnLt + btnRt + btnT;
        const int btnLRX = btnLt + btnRt + btnX;

        // Input to keycode conversions
        
        // DPAD
        readonly Android.Views.Keycode bUp = Android.Views.Keycode.DpadUp;
        readonly Android.Views.Keycode bDo = Android.Views.Keycode.DpadDown;
        readonly Android.Views.Keycode bLe = Android.Views.Keycode.DpadLeft;
        readonly Android.Views.Keycode bRi = Android.Views.Keycode.DpadRight;
        // L-R Triggers
        readonly Android.Views.Keycode bLt = Android.Views.Keycode.ButtonL1;
        readonly Android.Views.Keycode bRt = Android.Views.Keycode.ButtonR1;
        // XCTS
        readonly Android.Views.Keycode bX = Android.Views.Keycode.ButtonA;
        readonly Android.Views.Keycode bC = Android.Views.Keycode.ButtonB;
        readonly Android.Views.Keycode bT = Android.Views.Keycode.ButtonY;
        readonly Android.Views.Keycode bS = Android.Views.Keycode.ButtonX;
        // SEL-STA
        readonly Android.Views.Keycode bSe = Android.Views.Keycode.DpadCenter;
        readonly Android.Views.Keycode bSt = Android.Views.Keycode.Back;
        // Analog sticks
        // LEFT
        readonly Android.Views.Keycode aLu = Android.Views.Keycode.W;
        readonly Android.Views.Keycode aLd = Android.Views.Keycode.S;
        readonly Android.Views.Keycode aLl = Android.Views.Keycode.A;
        readonly Android.Views.Keycode aLr = Android.Views.Keycode.D;
        // RIGHT
        readonly Android.Views.Keycode aRu = Android.Views.Keycode.I;
        readonly Android.Views.Keycode aRd = Android.Views.Keycode.K;
        readonly Android.Views.Keycode aRl = Android.Views.Keycode.J;
        readonly Android.Views.Keycode aRr = Android.Views.Keycode.L;

        bool running = false;

        public override void OnCreate()
        {
            base.OnCreate();

            // Load custom mapping
            LoadCM();

            // Connect to the PSVita
            Connect();
        }
        
        // Load current custom mapping and display it onto the screen
        public void LoadCM()
        {
            try
            {
                Android.Views.Keycode[] buttons = { bUp, bRi, bDo, bLe, bLt, bRt, bX, bC, bT, bS, bSe, bSt, aLu, aLd, aLl, aLr, aRu, aRd, aRl, aRr };

                var cmFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "cm.scf");

                var lines = File.ReadAllLines(cmFile);

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var button = buttons[i];

                    button = (Android.Views.Keycode)int.Parse(line);
                }
            }
            catch (System.Exception ex)
            {
                Log.Verbose("{1}", ex.ToString());
            }
        }
        
        // Called when a key is pressed on the custom keyboard (?)
        public void OnKey([GeneratedEnum] Android.Views.Keycode primaryCode, [GeneratedEnum] Android.Views.Keycode[] keyCodes)
        {
            // The connection between the keyboard and this app
            IInputConnection ic = CurrentInputConnection;

            switch ((int)primaryCode)
            {
                case (int)Android.Views.Keycode.Del:
                    //ic.SendKeyEvent(new KeyEvent(KeyEventActions.Down,Android.Views.Keycode.Del));
                    ic.DeleteSurroundingText(1, 0);
                    break;

                case -1:
                    caps = !caps;
                    keyboard.SetShifted(caps);
                    kv.InvalidateAllKeys();
                    break;

                case (int)Android.Views.Keycode.Enter:
                    ic.SendKeyEvent(new KeyEvent(KeyEventActions.Down, Android.Views.Keycode.Enter));
                    break;

                case (int)Android.Views.Keycode.Button9: // Left analog stick pushed in (L3)
                    try
                    {
                        Connect();
                    }
                    catch (System.Exception ex)
                    {
                        // TODO: why this message (?)
                        // Toast.MakeText(this, "PSVITA Connected", ToastLength.Long).Show();
                        Log.Info("Exception: ", ex.ToString());
                    }
                    break;

                // If it is a normal character, then make it upper and send it (?)
                default:
                    char code = (char)primaryCode;

                    if (char.IsLetter(code) && caps)
                        code = char.ToUpper(code);

                    ic.CommitText(code.ToString(), 1);
                    break;
            }
        }

        // This part of the code is useless, but there is no way to continue without it (?)
        public void OnPress([GeneratedEnum] Android.Views.Keycode primaryCode)
        {
            //OnKey(primaryCode);
            //throw new NotImplementedException();
        }

        public void OnRelease([GeneratedEnum] Android.Views.Keycode primaryCode)
        {
            // throw new NotImplementedException();
        }

        public void OnText(ICharSequence text)
        {
            //  throw new NotImplementedException();
        }

        public void SwipeDown()
        {
            //   throw new NotImplementedException();
        }

        public void SwipeLeft()
        {
            //throw new NotImplementedException();
        }

        public void SwipeRight()
        {
            //throw new NotImplementedException();
        }

        public void SwipeUp()
        {
            //  throw new NotImplementedException();
        }

        // Connect to the PSVita and start listening for input
        public void Connect()
        {
            running = true;

            string ip = Xamarin.Essentials.Preferences.Get("ip", null) ?? throw new System.Exception("The IP cannot be empty");
            int port = 5000;
            
            // Change the port in case it is specified
            if (ip.Contains(':'))
            {
                port = int.Parse(ip.Split(':')[1]);
                ip = ip.Split(':')[0];
            }

            try
            {
                clientSocket.Connect(ip, port);
                if (!clientSocket.Connected)
                {
                    Toast.MakeText(this, "Couldn't Connect", ToastLength.Long).Show();
                    return;
                }
                Toast.MakeText(this, "PS VITA Connected", ToastLength.Long).Show();
                RunUpdateLoop();
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, "Network Error, try again", ToastLength.Long).Show();
                Log.Info("Exception: ", ex.ToString());
            }
        }
        //public override void OnDestroy()
        //{
        //    base.OnDestroy();
        //}

        // Called when the custom keyboard is requested to appear
        public override View OnCreateInputView()
        {
            kv = (KeyboardView)LayoutInflater.Inflate(Resource.Layout.keyboard, null);
            keyboard = new Keyboard(this, Resource.Xml.qwerty);
            kv.Keyboard = keyboard;
            kv.OnKeyboardActionListener = this;
            return kv;
        }

        // Listen for input
        private async void RunUpdateLoop()
        {
            NetworkStream serverStream = clientSocket.GetStream();
            while (running)
            {
                try
                {
                    await Task.Delay(100);
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes("request");
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();
                    b1 = serverStream.ReadByte(); // DPAD + SEL + STA
                    b2 = serverStream.ReadByte(); // XCTS + LT + RT // Cross (X), Circle, Triangle, Square
                    b3 = serverStream.ReadByte(); // NOT USED
                    b4 = serverStream.ReadByte(); // NOT USED
                    b5 = serverStream.ReadByte(); // L ANALOG X DATA
                    b6 = serverStream.ReadByte(); // L ANALOG Y DATA
                    b7 = serverStream.ReadByte(); // R ANALOG X DATA
                    b8 = serverStream.ReadByte(); // R ANALOG Y DATA

                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                    serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                    // b9 = BitConverter.ToInt32(inStream, 0); // TOUCHSCREEN DATA (Not Used Yet)
                    ParseInput(b1, b2, b3, b4, b5, b6, b7, b8);
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this, "PSVITA Disconnected", ToastLength.Long).Show();
                    Log.Info("Exception: ", ex.ToString());
                    running = false;
                }
            }
        }

        private void ParseInput(int a, int b, int c, int d, int e, int f, int g, int h)
        {
            ic = CurrentInputConnection;
            KeyEvent ks, kd, ka;

            // DPAD + SEL STA

            // Up
            if (a == btnU)
                ks = new KeyEvent(KeyEventActions.Down, bUp);
            else
                ks = new KeyEvent(KeyEventActions.Up, bUp);

            ic.SendKeyEvent(ks);
            
            // Right
            if (a == btnR)
                ks = new KeyEvent(KeyEventActions.Down, bRi);
            else
                ks = new KeyEvent(KeyEventActions.Up, bRi);

            ic.SendKeyEvent(ks);
            
            // Down
            if (a == btnD)
                ks = new KeyEvent(KeyEventActions.Down, bDo);
            else
                ks = new KeyEvent(KeyEventActions.Up, bDo);

            ic.SendKeyEvent(ks);
            
            // Left
            if (a == btnL)
                ks = new KeyEvent(KeyEventActions.Down, bLe);
            else
                ks = new KeyEvent(KeyEventActions.Up, bLe);

            ic.SendKeyEvent(ks);
            
            // Select
            if (a == btnSel)
                ks = new KeyEvent(KeyEventActions.Down, bSe);
            else
                ks = new KeyEvent(KeyEventActions.Up, bSe);

            ic.SendKeyEvent(ks);
            
            // Start
            if (a == btnSta)
                ks = new KeyEvent(KeyEventActions.Down, bSt);
            else
                ks = new KeyEvent(KeyEventActions.Up, bSt);

            ic.SendKeyEvent(ks);

            // Combos (Dpad & Dpad + Sel or Sta)

            // Left + Up
            if (a == btnLU)
            {
                ks = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(kd);
            }
            // Left + Down
            if (a == btnLD)
            {
                ks = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(kd);
            }
            // Right + Up
            if (a == btnRU)
            {
                ks = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(kd);
            }
            // Right + Down
            if (a == btnRD)
            {
                ks = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(kd);
            }
            // Up + Start
            if (a == btnUSt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Up + Select
            if (a == btnUSe)
            {
                ks = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(kd);
            }
            // Down + Start
            if (a == btnDSt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Down + Select
            if (a == btnDSe)
            {
                ks = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(kd);
            }
            // Left + Start
            if (a == btnLSt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Left + Select
            if (a == btnLSe)
            {
                ks = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(kd);
            }
            // Right + Start
            if (a == btnRSt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Right + Select
            if (a == btnRSe)
            {
                ks = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(kd);
            }

            // Select + Start
            if (a == btnSeSt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }

            // Triple combos (Dpad + Sel or Sta)

            // Left + Up + Start
            if (a == btnLUSt)
            {
                ka = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(kd);
            }
            // Left + Up + Select
            if (a == btnLUSe)
            {
                ka = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(kd);
            }
            // Left + Down + Start
            if (a == btnLDSt)
            {
                ka = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Left + Down + Select
            if (a == btnLDSe)
            {
                ka = new KeyEvent(KeyEventActions.Down, bLe);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(kd);
            }

            // Right + Up + Start
            if (a == btnRUSt)
            {
                ka = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Right + Up + Select
            if (a == btnRUSe)
            {
                ka = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bUp);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(kd);
            }
            // Right + Down + Start
            if (a == btnRDSt)
            {
                ka = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSt);
                ic.SendKeyEvent(kd);
            }
            // Right + Down + Select
            if (a == btnRDSe)
            {
                ka = new KeyEvent(KeyEventActions.Down, bRi);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bDo);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bSe);
                ic.SendKeyEvent(kd);
            }

            // XCTS + L R

            if (b == btnT)
                ka = new KeyEvent(KeyEventActions.Down, bT);
            else
                ka = new KeyEvent(KeyEventActions.Up, bT);
            
            ic.SendKeyEvent(ka);
            
            if (b == btnC)
                ka = new KeyEvent(KeyEventActions.Down, bC);
            else
                ka = new KeyEvent(KeyEventActions.Up, bC);
            
            ic.SendKeyEvent(ka);

            if (b == btnX)
                ka = new KeyEvent(KeyEventActions.Down, bX);
            else
                ka = new KeyEvent(KeyEventActions.Up, bX);
                
            ic.SendKeyEvent(ka);
            
            if (b == btnS)
                ka = new KeyEvent(KeyEventActions.Down, bS);
            else
                ka = new KeyEvent(KeyEventActions.Up, bS);
            
            ic.SendKeyEvent(ka);

            if (b == btnLt)
                ka = new KeyEvent(KeyEventActions.Down, bLt);
            else
                ka = new KeyEvent(KeyEventActions.Up, bLt);

            ic.SendKeyEvent(ka);

            if (b == btnRt)
                ka = new KeyEvent(KeyEventActions.Down, bRt);
            else
                ka = new KeyEvent(KeyEventActions.Up, bRt);

            ic.SendKeyEvent(ka);

            if (b == btnXC)
            {
                ks = new KeyEvent(KeyEventActions.Down, bX);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bC);
                ic.SendKeyEvent(kd);
            }
            if (b == btnXS)
            {
                ks = new KeyEvent(KeyEventActions.Down, bX);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bS);
                ic.SendKeyEvent(kd);
            }
            if (b == btnCT)
            {
                ks = new KeyEvent(KeyEventActions.Down, bC);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bT);
                ic.SendKeyEvent(kd);
            }
            if (b == btnTS)
            {
                ks = new KeyEvent(KeyEventActions.Down, bT);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bS);
                ic.SendKeyEvent(kd);
            }
            if (b == btnXLt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bX);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnXRt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bX);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnCLt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bC);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnCRt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bC);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnTLt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bT);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnTRt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bT);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnSLt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bS);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnSRt)
            {
                ks = new KeyEvent(KeyEventActions.Down, bS);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnLR)
            {
                ks = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnLRC)
            {
                ka = new KeyEvent(KeyEventActions.Down, bC);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnLRT)
            {
                ka = new KeyEvent(KeyEventActions.Down, bT);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnLRS)
            {
                ka = new KeyEvent(KeyEventActions.Down, bS);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }
            if (b == btnLRX)
            {
                ka = new KeyEvent(KeyEventActions.Down, bX);
                ic.SendKeyEvent(ka);
                ks = new KeyEvent(KeyEventActions.Down, bLt);
                ic.SendKeyEvent(ks);
                kd = new KeyEvent(KeyEventActions.Down, bRt);
                ic.SendKeyEvent(kd);
            }

            // Left analog stick

            if (e <= 50 && e >= 0)
                ka = new KeyEvent(KeyEventActions.Down, aLl);
            else
                ka = new KeyEvent(KeyEventActions.Up, aLl);
            
            ic.SendKeyEvent(ka);

            if (e >= 200 && e <= 255)
                ka = new KeyEvent(KeyEventActions.Down, aLr);
            else
                ka = new KeyEvent(KeyEventActions.Up, aLr);

            ic.SendKeyEvent(ka);
            
            if (f <= 50 && f >= 0)
                ka = new KeyEvent(KeyEventActions.Down, aLu);
            else
                ka = new KeyEvent(KeyEventActions.Up, aLu);

            ic.SendKeyEvent(ka);

            if (f >= 200 && f <= 255)
                ka = new KeyEvent(KeyEventActions.Down, aLd);
            else
                ka = new KeyEvent(KeyEventActions.Up, aLd);

            ic.SendKeyEvent(ka);

            // Right analog stick

            if (g <= 50 && g >= 0)
                ka = new KeyEvent(KeyEventActions.Down, aRl);
            else
                ka = new KeyEvent(KeyEventActions.Up, aRl);

            ic.SendKeyEvent(ka);
            
            if (g >= 200 && g <= 255)
                ka = new KeyEvent(KeyEventActions.Down, aRr);
            else
                ka = new KeyEvent(KeyEventActions.Up, aRr);

            ic.SendKeyEvent(ka);

            if (h <= 50 && g >= 0)
                ka = new KeyEvent(KeyEventActions.Down, aRu);
            else
                ka = new KeyEvent(KeyEventActions.Up, aRu);

            ic.SendKeyEvent(ka);
            
            if (h >= 200 && h <= 255)
                ka = new KeyEvent(KeyEventActions.Down, aRd);
            else
                ka = new KeyEvent(KeyEventActions.Up, aRd);

            ic.SendKeyEvent(ka);
        }
    }
}