using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views.InputMethods;
using Xamarin.Essentials;

namespace VitaMote {
    [Activity(Label = "VitaMote", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {
        TextView label1;
        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            Platform.Init(this, bundle);

            SetContentView(Resource.Layout.Main);

            Button button = FindViewById<Button>(Resource.Id.MyButton);
            Button btnime = FindViewById<Button>(Resource.Id.btnIME);
            Button btnmap = FindViewById<Button>(Resource.Id.btnMap);
            EditText text1 = FindViewById<EditText>(Resource.Id.editText1);
            label1 = FindViewById<TextView>(Resource.Id.textView7);
            label1.Text=ReFile();
            button.Click+=delegate {
                SaveFl(text1.Text);
                label1.Text=ReFile();
            };
            btnime.Click += delegate {
                InputMethodManager imeManager = (InputMethodManager)GetSystemService(InputMethodService);
                imeManager.ShowInputMethodPicker();
            };
            btnmap.Click += delegate {
                StartActivity(typeof(CusMap));           
                };
        }

        private void SaveFl(string texto) {
            Preferences.Set("ip", texto);
            Toast.MakeText(this, "Successfully Saved", ToastLength.Long).Show();
        }
        private string ReFile() {
            var ip = Preferences.Get("ip", null);
            if (ip == null)
            {
                Toast.MakeText(this, "Remember to store an IP", ToastLength.Long).Show();
                return "No IP Saved";
            }
            return ip;
        }
    }
}

