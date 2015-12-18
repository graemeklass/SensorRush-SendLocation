using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using System.Threading.Tasks;
using SensorRush_SendLocation.Model;
using SensorRush_SendLocation.Service;

namespace SensorRush_SendLocation
{
    /// <summary>
    /// Records and sends Location data to Sensor Rush cloud via REST api
    /// References:
    /// https://developer.xamarin.com/recipes/android/os_device_resources/gps/get_current_device_location/
    /// </summary>
    [Activity(Label = "SensorRush_SendLocation", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        private static readonly object _syncLock = new object();
        LocationManager _locationManager;
        TextView _locationText;
        TextView _sendAPIText;
        String _locationProvider;
        private System.Threading.CancellationTokenSource _ctsLocation;
        private bool _recordLocation = false;
        int POLLING_TIME = 100; //in ms.
        int BATCHSEND = 20; //samples to store before we send
        int _uploadCounter = 0;
        /// <summary>
        /// Holds the location data in so we can send in batches.
        /// </summary>
        List<Model.MyLocation.LocationData> listLocation = new List<Model.MyLocation.LocationData>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var btnRecordGPS = FindViewById<Button>(Resource.Id.btnRecordLocation);
            btnRecordGPS.Click += (sender, e) =>
            {
                _ctsLocation = new System.Threading.CancellationTokenSource();
                _recordLocation = true;
                _locationText.Text = "Getting location data...";
                _uploadCounter = 0;
            };

            Button btnCancelLocation = this.FindViewById<Button>(Resource.Id.btnCancelLocation);
            btnCancelLocation.Click += (sender, e) =>
            {
                _ctsLocation.Cancel();
                _recordLocation = false;
                _locationText.Text = "Cancelled.";
            };

            _locationText = this.FindViewById<TextView>(Resource.Id.txtLocation);
            _sendAPIText = this.FindViewById<TextView>(Resource.Id.txtSendAPIMessage);

            InitializeLocationManager();
        }

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Count > 0)
            {
                _locationProvider = acceptableLocationProviders[0];
            }
            else
            {
                _locationProvider = String.Empty;
            }
        }

        public void OnLocationChanged(Android.Locations.Location location)
        {

            if (!_recordLocation)
                return;

            
            lock (_syncLock)
            {
                Model.MyLocation.LocationData locData = new Model.MyLocation.LocationData()
                {
                    timestampUTC = DateTime.UtcNow,
                    Lattitude = location.Latitude,
                    Longitude = location.Longitude,
                    Altitude = location.Altitude,
                    Accuracy = location.Accuracy
                };
                _locationText.Text = String.Format("Long: {0}; Lat: {1}; Alt: {2} (68% chance of being within {3} meters)",
                                                    locData.Longitude,
                                                    locData.Lattitude,
                                                    locData.Altitude,
                                                    locData.Accuracy);
                //let's add this to a list
                listLocation.Add(locData);

                if (listLocation.Count >= BATCHSEND) //check to see if we have enough samples to batch send.
                {
                    Service.SensorRushAPI.SendLocationViaApi("AndroidDemoLocationData", listLocation, _ctsLocation.Token);
                    _sendAPIText.Text = String.Format("{0} samples uploaded", _uploadCounter += listLocation.Count);
                    listLocation.Clear();
                }
            }
            

        }

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, POLLING_TIME, 0, this); 
        }
        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this); //to save on battery
        }

    }
}

