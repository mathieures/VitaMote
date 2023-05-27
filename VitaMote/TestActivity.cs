// Activity that will act as a network tester, to see if keys (or anything, really) are received from the PSVita

using Android.App;
using Android.OS;

namespace VitaMote
{
    [Activity(Label = "Network tester")]
    public class TestActivity: Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.test_network_activity);
        }
    }
}