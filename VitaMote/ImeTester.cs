// Activity aiming to test the IME in a text field

using Android.App;
using Android.InputMethodServices;
using Android.OS;

namespace VitaMote
{
    [Activity(Label = "IME tester", WindowSoftInputMode = Android.Views.SoftInput.StateVisible)]
    public class ImeTester : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ime_tester);
        }
    }
}