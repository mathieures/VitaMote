// Service transforming the converted keypresses into actual Android keypresses used in any process

using System;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Views;

namespace VitaMote
{
    [Service(Label = "PSVita", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [MetaData(name: "android.view.im", Resource = "@xml/method")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    public class VitaIme : InputMethodService
    {
        public override void OnCreate()
        {
            base.OnCreate();

            // VitaConnection
        }

        public override View OnCreateInputView()
        {
            Console.WriteLine("OnCreateInputView");

            var keyboardView = LayoutInflater.Inflate(Resource.Layout.ime_view, null);

            return keyboardView;
        }
    }
}