// Service transforming the converted keypresses into actual Android keypresses used in any process

// Trying to follow these steps: https://stackoverflow.com/a/72752768/14349477

using System;
using System.Xml;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace VitaMote
{
    [Service(Label = "Test IME Service", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [MetaData(name: "android.view.im", Resource = "@xml/method")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    public class TestIMEService : InputMethodService
    {
        View keyboardView;

        public override void OnCreate()
        {
            base.OnCreate();

            //GetSystemService(InputMethodService)
            //IInputConnection ic = CurrentInputConnection;

            //Console.WriteLine("ic : " + ic.ToString());
        }

        public override View OnCreateInputView()
        {
            Console.WriteLine("OnCreateInputView");

            keyboardView = LayoutInflater.Inflate(Resource.Layout.ime_view, null);

            return keyboardView;
        }

        public void OnKeyClick(View pressedButtonView)
        {
            // Après le test je pense qu’on peut s’en débarrasser, vu qu’on a pas besoin de bouton à cliquer, on enverra les keys programmatiquement
            var pressedButton = (Button)pressedButtonView;

            IInputConnection ic = CurrentInputConnection;

            if (ic == null)
            {
                Log.Info("OnKeyClick: no connection");
            }
            else
            {
                var tag = (string)pressedButton.GetTag(1); // aucune idée de ce qu’il faut donner en argu/paramètre
                //if ()
            }
        }

        protected override void OnCurrentInputMethodSubtypeChanged(InputMethodSubtype newSubtype)
        {
            Console.WriteLine("OnCurrentInputMethodSubtypeChanged");
            base.OnCurrentInputMethodSubtypeChanged(newSubtype);
        }

        public override void OnInitializeInterface()
        {
            Console.WriteLine("OnInitializeInterface");
            base.OnInitializeInterface();
        }

        public override bool OnShowInputRequested([GeneratedEnum] ShowFlags flags, bool configChange)
        {
            Console.WriteLine("OnShowInputRequested");
            return base.OnShowInputRequested(flags, configChange);
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            Console.WriteLine("OnStartInputView");
            base.OnStartInputView(info, restarting);
        }

        public override void OnWindowShown()
        {
            Console.WriteLine("OnWindowShown");
            base.OnWindowShown();
        }

        public override void OnWindowHidden()
        {
            Console.WriteLine("OnWindowHidden");
            base.OnWindowHidden();
        }

        public override bool OnKeyDown([GeneratedEnum] Android.Views.Keycode keyCode, KeyEvent e)
        {
            Console.WriteLine("OnKeyDown");
            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp([GeneratedEnum] Android.Views.Keycode keyCode, KeyEvent e)
        {
            Console.WriteLine("OnKeyUp");
            return base.OnKeyDown(keyCode, e);
        }

        public override void OnDestroy()
        {
            Console.WriteLine("OnDestroy");
            base.OnDestroy();
        }
    }
}