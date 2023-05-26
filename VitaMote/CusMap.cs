using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace VitaMote
{

    [Activity(Label = "Custom Mapping")]
    public class CusMap : Activity
    {
        // KeyCodes
        readonly List<Keycode> keycodes = new List<Keycode>{ Keycode.A, Keycode.B, Keycode.C, Keycode.D, Keycode.E, Keycode.F, Keycode.G, Keycode.H, Keycode.I, Keycode.J, Keycode.K, Keycode.L, Keycode.M, Keycode.N, Keycode.O, Keycode.P, Keycode.Q, Keycode.R, Keycode.S, Keycode.T, Keycode.U, Keycode.V, Keycode.W, Keycode.X, Keycode.Y, Keycode.Z, Keycode.AltLeft, Keycode.AltRight, Keycode.Apostrophe, Keycode.AppSwitch, Keycode.Assist, Keycode.At, Keycode.AvrInput, Keycode.AvrPower, Keycode.Back, Keycode.Backslash, Keycode.Bookmark, Keycode.Break, Keycode.BrightnessUp, Keycode.BrightnessDown, Keycode.Button1, Keycode.Button2, Keycode.Button3, Keycode.Button4, Keycode.Button5, Keycode.Button6, Keycode.Button7, Keycode.Button8, Keycode.Button9, Keycode.Button10, Keycode.Button11, Keycode.Button12, Keycode.Button13, Keycode.Button14, Keycode.Button15, Keycode.Button16, Keycode.ButtonA, Keycode.ButtonB, Keycode.ButtonC, Keycode.ButtonX, Keycode.ButtonY, Keycode.ButtonZ, Keycode.ButtonL1, Keycode.ButtonL2, Keycode.ButtonR1, Keycode.ButtonR2, Keycode.ButtonSelect, Keycode.ButtonStart, Keycode.ButtonMode, Keycode.ButtonThumbl, Keycode.ButtonThumbr, Keycode.Calculator, Keycode.Calendar, Keycode.Call, Keycode.Camera, Keycode.CapsLock, Keycode.Captions, Keycode.ChannelUp, Keycode.ChannelDown, Keycode.Clear, Keycode.Comma, Keycode.Contacts, Keycode.Copy, Keycode.CtrlLeft, Keycode.CtrlRight, Keycode.Cut, Keycode.Del, Keycode.DpadCenter, Keycode.DpadDownLeft, Keycode.DpadDownRight, Keycode.DpadUpLeft, Keycode.DpadUpRight, Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp, Keycode.DpadDown, Keycode.Dvr, Keycode.Eisu, Keycode.Endcall, Keycode.Enter, Keycode.Envelope, Keycode.Equals, Keycode.Escape, Keycode.Explorer, Keycode.F1, Keycode.F2, Keycode.F3, Keycode.F4, Keycode.F5, Keycode.F6, Keycode.F7, Keycode.F8, Keycode.F9, Keycode.F10, Keycode.F11, Keycode.F12, Keycode.Focus, Keycode.Forward, Keycode.ForwardDel, Keycode.Function, Keycode.Grave, Keycode.Guide, Keycode.Headsethook, Keycode.Help, Keycode.Henkan, Keycode.Home, Keycode.Info, Keycode.Insert, Keycode.K11, Keycode.K12, Keycode.Kana, Keycode.KatakanaHiragana, Keycode.LanguageSwitch, Keycode.LastChannel, Keycode.LeftBracket, Keycode.MannerMode, Keycode.MediaAudioTrack, Keycode.MediaClose, Keycode.MediaEject, Keycode.MediaFastForward, Keycode.MediaNext, Keycode.MediaPause, Keycode.MediaPlay, Keycode.MediaPlayPause, Keycode.MediaPrevious, Keycode.MediaRecord, Keycode.MediaRewind, Keycode.MediaStop, Keycode.Menu, Keycode.Minus, Keycode.Music, Keycode.Mute, Keycode.NavigateIn, Keycode.NavigateOut, Keycode.NavigatePrevious, Keycode.Notification, Keycode.Num, Keycode.Num0, Keycode.Num1, Keycode.Num2, Keycode.Num3, Keycode.Num4, Keycode.Num5, Keycode.Num6, Keycode.Num7, Keycode.Num8, Keycode.Num9, Keycode.NumLock, Keycode.Numpad0, Keycode.Numpad1, Keycode.Numpad2, Keycode.Numpad3, Keycode.Numpad4, Keycode.Numpad5, Keycode.Numpad6, Keycode.Numpad7, Keycode.Numpad8, Keycode.Numpad9, Keycode.NumpadAdd, Keycode.NumpadComma, Keycode.NumpadDivide, Keycode.NumpadDot, Keycode.NumpadEnter, Keycode.NumpadEquals, Keycode.NumpadMultiply, Keycode.NumpadSubtract, Keycode.NumpadLeftParen, Keycode.NumpadRightParen, Keycode.PageDown, Keycode.PageUp, Keycode.Pairing, Keycode.Paste, Keycode.Period, Keycode.Plus, Keycode.Pound, Keycode.Power, Keycode.ProgGreen, Keycode.ProgRed, Keycode.ProgBlue, Keycode.ProgYellow, Keycode.RightBracket, Keycode.Search, Keycode.Semicolon, Keycode.Settings, Keycode.ShiftLeft, Keycode.ShiftRight, Keycode.Slash, Keycode.Sleep, Keycode.Space, Keycode.Star, Keycode.Sym, Keycode.Sysrq, Keycode.Tab, Keycode.VolumeDown, Keycode.VolumeUp, Keycode.VolumeMute, Keycode.Wakeup, Keycode.Window, Keycode.ZoomIn, Keycode.ZoomOut };

        // Key Strings (some may be missing, but are not too important)
        readonly string[] dispKeys = {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Alt Left", "Alt Right", "Apostrophe", "App Switch", "Assist", "At", "Avr Input", "Avr Power", "Back", "Backslash", "Bookmark", "Break", "Brightness Up", "Brightness Down", "Button 1", "Button 2", "Button 3", "Button 4", "Button 5", "Button 6", "Button 7", "Button 8", "Button 9", "Button 10", "Button 11", "Button 12", "Button 13", "Button 14", "Button 15", "Button 16", "Button A", "Button B", "Button C", "Button X", "Button Y", "Button Z", "Button L1", "Button L2", "Button R1", "Button R2", "Button Select", "Button Start", "Button Mode", "Button Thumb L", "Button Thumb R", "Calculator", "Calendar", "Call", "Camera", "Caps Lock", "Caption", "Channel Up", "Channel Down", "Clear", "Comma", "Contacts", "Copy", "Ctrl Left", "Ctrl Right", "Cut", "Del", "Dpad Center", "Dpad DownLeft", "Dpad DownRight", "Dpad UpLeft", "Dpad UpRight", "Dpad Left", "Dpad Right", "Dpad Up", "Dpad Down", "Dvr", "Eisu", "End Call", "Enter", "Envelope", "Equals", "Escape", "Explorer", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Focus", "Forward", "Forward Del", "Function", "Grave", "Guide", "Headset Hook", "Help", "Henkan", "Home", "Info", "Insert", "K11", "K12", "Kana", "Katakana Higragana", "Languaje Switch", "Last Channel", "Left Bracket", "Manner Mode", "Media Audio Track", "Media Close", "Media Eject", "Media Fast Forward", "Media Next", "Media Pause", "Media Play", "Media Play Pause", "Media Previous", "Media Record", "Media Rewind", "Media Stop", "Menu", "Minus", "Music", "Mute", "Navigate In", "Navigate Out", "Navigate Previous", "Notification", "Num", "Num 0 ", "Num 1", "Num 2", "Num 3", "Num 4", "Num 5", "Num 6", "Num 7", "Num 8", "Num 9", "Num Lock", "NumPad 0 ", "NumPad 1", "NumPad 2", "NumPad 3", "NumPad 4", "NumPad 5", "NumPad 6", "NumPad 7", "NumPad 8", "NumPad 9", "NumPad Add", "NumPad Comma", "NumPad Divide", "NumPad Dot", "NumPad Enter", "NumPad Equals", "NumPad Multiply", "NumPad Substract", "NumPad Left Parent", "NumPad Right Parent", "Page Down", "Page Up", "Pairing", "Paste", "Period", "Plus", "Pound", "Power", "Prog Green", "Prog Red", "Prog Blue", "Prog Yellow", "Right Bracket", "Search", "Semicolon", "Settings", "Shift Left", "Shift Right", "Slash", "Sleep", "Space", "Star (*)", "Sym", "SysRq", "Tab", "Volume Down", "Volume Up", "Volume Mute", "Wake Up", "Window", "Zoom In", "Zoom Out"
        };

        Spinner[] spinners;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Mapping);

            // UI elements
            spinners = new[]
            {
                (Spinner)FindViewById(Resource.Id.spinner1),
                (Spinner)FindViewById(Resource.Id.spinner2),
                (Spinner)FindViewById(Resource.Id.spinner3),
                (Spinner)FindViewById(Resource.Id.spinner4),
                (Spinner)FindViewById(Resource.Id.spinner5),
                (Spinner)FindViewById(Resource.Id.spinner6),
                (Spinner)FindViewById(Resource.Id.spinner7),
                (Spinner)FindViewById(Resource.Id.spinner8),
                (Spinner)FindViewById(Resource.Id.spinner9),
                (Spinner)FindViewById(Resource.Id.spinner10),
                (Spinner)FindViewById(Resource.Id.spinner11),
                (Spinner)FindViewById(Resource.Id.spinner12),
                (Spinner)FindViewById(Resource.Id.spinner13),
                (Spinner)FindViewById(Resource.Id.spinner14),
                (Spinner)FindViewById(Resource.Id.spinner15),
                (Spinner)FindViewById(Resource.Id.spinner16),
                (Spinner)FindViewById(Resource.Id.spinner17),
                (Spinner)FindViewById(Resource.Id.spinner18),
                (Spinner)FindViewById(Resource.Id.spinner19),
                (Spinner)FindViewById(Resource.Id.spinner20)
            };

            Button butLo = (Button)FindViewById(Resource.Id.btnLoad);
            Button butSa = (Button)FindViewById(Resource.Id.btnSve);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleSpinnerItem,
                dispKeys
            );

            foreach (var spinner in spinners)
            {
                spinner.Adapter = adapter;
            }

            LoadCM();
            butLo.Click += delegate
            {
                SetDefaults();
            };
            butSa.Click += delegate
            {
                SaveCM();
            };
        }
        private void SaveCM()
        {
            var cmFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "cm.scf");
            StringBuilder sb = new StringBuilder();

            foreach (var spinner in spinners)
            {
                sb.AppendLine(((int)keycodes[(int)spinner.SelectedItemId]).ToString());
            }

            File.WriteAllText(cmFile, sb.ToString());

            Toast.MakeText(this, "Successfully Saved", ToastLength.Long).Show();
        }
        public void LoadCM()
        {
            try
            {
                var cmFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "cm.scf");

                var lines = File.ReadAllLines(cmFile);

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var spinner = spinners[i];

                    spinner.SetSelection(keycodes.IndexOf((Keycode)int.Parse(line)));
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                SetDefaults();
                SaveCM();
            }
        }
        private void SetDefaults()
        {
            // Set the default key for each spinner
            Keycode[] defaultKeys = {
                Keycode.DpadUp,
                Keycode.DpadRight,
                Keycode.DpadDown,
                Keycode.DpadLeft,
                Keycode.ButtonL1,
                Keycode.ButtonR1,
                Keycode.ButtonA,
                Keycode.ButtonB,
                Keycode.ButtonY,
                Keycode.ButtonX,
                Keycode.DpadCenter,
                Keycode.Back,
                Keycode.W,
                Keycode.D,
                Keycode.S,
                Keycode.A,
                Keycode.I,
                Keycode.L,
                Keycode.K,
                Keycode.J
            };

            for (int i = 0; i < defaultKeys.Length; i++)
            {
                spinners[i].SetSelection(keycodes.IndexOf(defaultKeys[i]));
            }
        }
    }
}