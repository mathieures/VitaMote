using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views.InputMethods;
using Xamarin.Essentials;

namespace VitaMote {
    [Activity(Label = "VitaMote", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {

        TextView ipTextView;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            Platform.Init(this, bundle);

            SetContentView(Resource.Layout.Main);

            Button saveButton = FindViewById<Button>(Resource.Id.saveIpBtn);
            Button changeIMEButton = FindViewById<Button>(Resource.Id.changeIMEBtn);
            Button customMappingButton = FindViewById<Button>(Resource.Id.customMappingBtn);
            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            
            ipTextView = FindViewById<TextView>(Resource.Id.savedIPTextView);
            ipTextView.Text = GetSavedIP();
            
            saveButton.Click += delegate
            {
                SaveIP(ipEditText.Text);
                ipTextView.Text = GetSavedIP();
            };

            // Show the IME picker on click
            changeIMEButton.Click += delegate {
                InputMethodManager imeManager = (InputMethodManager)GetSystemService(InputMethodService);
                imeManager.ShowInputMethodPicker();
            };

            // Show the custom mapping configuration on click
            customMappingButton.Click += delegate {
                StartActivity(typeof(CusMap));
            };
        }
        private void SaveIP(string ip)
        {
            Preferences.Set("ip", ip);
            Toast.MakeText(this, "Successfully Saved", ToastLength.Long).Show();
        }
        private string GetSavedIP()
        {
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

