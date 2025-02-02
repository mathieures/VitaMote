﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;
using Xamarin.Essentials;

namespace VitaMote
{
    [Activity(Label = "VitaMote", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView ipTextView;
        VitaConnection vitaConnection;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Platform.Init(this, bundle);

            SetContentView(Resource.Layout.Main);

            Button saveButton = FindViewById<Button>(Resource.Id.saveIpBtn);
            Button changeIMEButton = FindViewById<Button>(Resource.Id.changeIMEBtn);

            Button customMappingButton = FindViewById<Button>(Resource.Id.customMappingBtn);
            Button testConnectionButton = FindViewById<Button>(Resource.Id.testConnectionButton);
            Button testIMEButton = FindViewById<Button>(Resource.Id.testIMEButton);

            EditText ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);

            ipTextView = FindViewById<TextView>(Resource.Id.savedIPTextView);

            // If no IP was saved or if it is empty, set the text to the default IP
            var settings = new SavedSettings(); // Load the current settings
            if (string.IsNullOrEmpty(settings.IP))
            {
                Toast.MakeText(this, "Remember to store an IP", ToastLength.Long).Show();
                ipTextView.Text = "No IP Saved";
            }
            else
            {
                ipEditText.Text = settings.IP;
                ipTextView.Text = ipEditText.Text;
            }

            vitaConnection = VitaConnection.Instance;

            // On click, save the IP
            saveButton.Click += delegate
            {
                SaveIP(ipEditText.Text);
                ipTextView.Text = ipEditText.Text;
            };

            // On click, show the IME picker
            changeIMEButton.Click += delegate
            {
                InputMethodManager imeManager = (InputMethodManager)GetSystemService(InputMethodService);
                imeManager.ShowInputMethodPicker();
            };

            // On click, show the custom mapping configuration page
            customMappingButton.Click += delegate
            {
                StartActivity(typeof(CusMap));
            };

            // On click show the connection testing page
            testConnectionButton.Click += delegate
            {
                var ip = GetSavedIP();
                if (!string.IsNullOrEmpty(ip))
                    StartActivity(typeof(ConnectionTester));
                else
                    Toast.MakeText(this, "The IP cannot be empty.", ToastLength.Long).Show();
            };

            // On click show the IME testing page
            testIMEButton.Click += delegate
            {
                StartActivity(typeof(ImeTester));
            };
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Start the service
            var intent = new Intent(this, typeof(VitaIme));
            try
            {
                StartService(intent);
            }
            catch (BackgroundServiceStartNotAllowedException)
            {
                Log.Error("OnStart", "Could not start service. Maybe the application is in the background.");
            }
        }

        private void SaveIP(string ip)
        {
            Preferences.Set("ip", ip);
            Toast.MakeText(this, "Successfully Saved", ToastLength.Long).Show();
        }
        private string GetSavedIP()
        {
            return Preferences.Get("ip", null);
        }
    }
}

