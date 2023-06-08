// Service transforming the converted keypresses into actual Android keypresses used in any process

// Trying to follow these steps: https://stackoverflow.com/a/72752768/14349477

using System;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace VitaMote
{
    [Service(Label = "Test IME Service", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [MetaData(name: "android.view.im", Resource = "@xml/method")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    public class TestIMEService : InputMethodService, View.IOnClickListener
    {
        //public override void OnCreate()
        //{
        //    base.OnCreate();
        //}

        public override View OnCreateInputView()
        {
            Console.WriteLine("OnCreateInputView");

            var keyboardView = LayoutInflater.Inflate(Resource.Layout.ime_view, null);

            // Add a listener to the buttons; to add keys to the keyboard, add ID's to the buttons and add them here.
            // (Here, two are enough to test the keyboard)
            Button[] buttons = {
                keyboardView.FindViewById<Button>(Resource.Id.keyG),
                keyboardView.FindViewById<Button>(Resource.Id.keyShift)
            };

            foreach (var button in buttons)
            {
                button.SetOnClickListener(this);
            }

            return keyboardView;
        }

        public void OnClick(View v)
        {
            var t = v.Tag.ToString();
            Log.Info("OnClick", t);

            // handle all the keyboard key clicks here

            IInputConnection ic = CurrentInputConnection;
            ic.CommitText(t, t.Length);
        }
    }
}