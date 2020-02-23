using Newtonsoft.Json;
using NLog;
using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace BataTurboNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Client m_client = new Client();
        private List<Device> devices = new List<Device>();
        private static HttpClient httpClient = new HttpClient();

        private Timer watchdogTimer = new Timer(60000);

        public MainWindow()
        {
            InitializeComponent();
            urlTextBox.Text = Properties.Settings.Default.CdbUrl;
            SimulationCheckBox.IsChecked = Properties.Settings.Default.Simulation;

            watchdogTimer.Elapsed += WatchdogTimer_Elapsed;

            var args = Environment.GetCommandLineArgs();

            if (args.Count() >= 2)
            {
                switch (args.ElementAt(1).ToLower())
                {
                    case "start":
                        logger.Info("Auto start");
                        Task task = new Task(() =>
                        {
                            Task.Delay(60000);
                            Connect();
                        });
                        task.Start();
                        break;
                    default:
                        logger.Info("Unkown argument:" + args.First().ToLower());
                        break;
                }

            }
        }

        private void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PostDeviceLifeSign(Environment.MachineName, 0, true);
        }

        #region Post the results
        private async void PostGpsLocation(GPSLocation gps)
        {
            try
            {
                string json = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PostGpsLocation " + json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(Properties.Settings.Default.CdbUrl, content);

                logger.Info("PostObject :" + result.StatusCode);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.Info("Error with PostGpsLocation " + json);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private async void PostDeviceLifeSign(string deviceName, int RadioID, bool online)
        {
            try
            {
                DeviceLifeSign gps = new DeviceLifeSign();
                gps.deviceName = deviceName;
                gps.RadioID = RadioID;

                if (online)
                {
                    gps.status = "online";
                }
                else
                {
                    gps.status = "offline";
                }

                string json = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PostDeviceLifeSign " + json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(Properties.Settings.Default.CdbUrl, content);

                logger.Info("PostObject :" + result.StatusCode);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.Info("Error with PostDeviceLifeSign " + json);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
        #endregion
        #region TurboNet
        private void ConnectToTurboNet()
        {
            logger.Info("Connect to turbonet server");

            m_client.Disconnect();

            m_client.Connect(new NS.Shared.Network.NetworkConnectionParam(Properties.Settings.Default.TurboNetHost, Properties.Settings.Default.TurboNetPort), new UserInfo(Properties.Settings.Default.TurboNetUser, Properties.Settings.Default.TurboNetPassword), ClientInitFlags.Empty);
            if (m_client.IsStarted)
            {
                logger.Info("Connected to turbonet server");
            }

            m_client.GetAllWorkflowCommands();

            devices = m_client.LoadRegisteredDevicesFromServer();
            foreach (var device in devices)
            {
                PostDeviceLifeSign(device.Name, device.RadioID, false);
            }

            foreach (var dev in m_client.LoadUnregisteredDevicesFromServer())
            {
                dev.Name = "Radio " + dev.RadioID;
                devices.Add(dev);
            }

            m_client.BeaconSignal += M_client_BeaconSignal;
            m_client.DevicesChanged += DevicesChanged;
            m_client.DeviceLocationChanged += DeviceLocationChanged;
            m_client.DeviceStateChanged += M_client_DeviceStateChanged;
            m_client.TransmitReceiveChanged += M_client_TransmitReceiveChanged;
            m_client.DeviceTelemetryChanged += M_client_DeviceTelemetryChanged;
            m_client.WorkflowCommandFinished += M_client_WorkflowCommandFinished;

            PostDeviceLifeSign(Environment.MachineName, 0, true);
        }

        private void M_client_WorkflowCommandFinished(object sender, WorkflowCommandFinishedEventArgs e)
        {
            try
            {
                logger.Info("M_client_WorkflowCommandFinished");
                Device device;
                lock (devices)
                    device = devices.FirstOrDefault(r => r.ID == e.DeviceId);

                if (device != null)
                {
                    StringBuilder build = new StringBuilder();
                    build.Append("M_client_WorkflowCommandFinished");
                    build.Append("device: " + device.Name + " ");
                    build.Append("state: " + e.RequestId.ToString());
                    build.Append("state: " + e.Result.ToString());

                    logger.Info(build.ToString());

                    PostDeviceLifeSign(device.Name, device.RadioID, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void M_client_DeviceTelemetryChanged(object sender, DeviceTelemetryChangedEventArgs e)
        {
            try
            {
                logger.Info("M_client_DeviceTelemetryChanged");

                Device device;
                lock (devices)
                    device = devices.FirstOrDefault(r => r.ID == e.DeviceId);

                if (device != null)
                {
                    StringBuilder build = new StringBuilder();
                    build.Append("M_client_DeviceTelemetryChanged");
                    build.Append("device: " + device.Name + " ");
                    build.Append("state: " + e.State.ToString());

                    logger.Info(build.ToString());

                    PostDeviceLifeSign(device.Name, device.RadioID, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void M_client_TransmitReceiveChanged(object sender, TransmitReceiveArgs e)
        {
            try
            {
                logger.Info("M_client_TransmitReceiveChanged");

                Device device;
                lock (devices)
                {
                    device = devices.FirstOrDefault(r => r.ID == e.Info.TransmitDeviceID);
                }

                if (device != null)
                {
                    StringBuilder build = new StringBuilder();
                    build.Append("M_client_TransmitReceiveChanged");
                    build.Append("device: " + device.Name + " ");

                    logger.Info(build.ToString());

                    PostDeviceLifeSign(device.Name, device.RadioID, true);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void M_client_BeaconSignal(object sender, NS.Enterprise.Objects.Beacons.BeaconSignalEventArgs e)
        {
            try
            {
                logger.Info("M_client_BeaconSignal");

                foreach (var radio in e.Infos)
                {
                    Device device;
                    lock (devices)
                    {
                        device = devices.FirstOrDefault(r => r.ID == radio.DeviceID);
                    }

                    StringBuilder build = new StringBuilder();
                    build.Append("M_client_BeaconSignal");
                    build.Append("device: " + device.Name + " ");
                    build.Append("batterylevel" + radio.BatteryLevel + " ");

                    logger.Info(build.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void M_client_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            try
            {
                logger.Info("M_client_DeviceStateChanged");

                foreach (var radio in e.Infos)
                {
                    Device device;
                    lock (devices)
                    {
                            device = devices.FirstOrDefault(r => r.ID == radio.DeviceId);
                    }

                    StringBuilder build = new StringBuilder();
                    build.Append("M_client_DeviceStateChanged");
                    build.Append("device: " + device.Name + " ");
                    build.Append("state: " + radio.State.ToString() + " ");

                    PostDeviceLifeSign(device.Name, device.RadioID, (radio.State & DeviceState.Active) == DeviceState.Active);

                    logger.Info(build.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void DeviceLocationChanged(object sender, DeviceLocationChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceLocationChanged");

                foreach (var i in e.GPSData)
                {
                    Device device;
                    lock (devices)
                    {
                        device = devices.FirstOrDefault(r => r.ID == i.DeviceID);

                        if (device == null)
                        {
                            devices.Clear();
                            devices = m_client.LoadRegisteredDevicesFromServer();

                            foreach (var dev in m_client.LoadUnregisteredDevicesFromServer())
                            {
                                dev.Name = "Radio " + dev.RadioID;
                                devices.Add(dev);
                            }

                            device = devices.FirstOrDefault(r => r.ID == i.DeviceID);
                        }
                    }

                    if (device != null)
                    {
                        StringBuilder build = new StringBuilder();
                        build.Append("DeviceLocationChanged");
                        build.Append("device: " + device.Name + " ");
                        build.Append("Altitude: " + i.Altitude + " ");
                        build.Append("Description: " + i.Description + " ");
                        build.Append("DeviceID: " + i.DeviceID + " ");
                        build.Append("Direction: " + i.Direction + " ");
                        build.Append("GpsSource: " + i.GpsSource + " ");
                        build.Append("InfoDate: " + i.InfoDate.ToString() + " ");
                        build.Append("InfoDateUtc: " + i.InfoDateUtc.ToString() + " ");
                        build.Append("Latitude: " + i.Latitude.ToString() + " ");
                        build.Append("Name: " + i.Name + " ");
                        build.Append("Radius: " + i.Radius.ToString() + " ");
                        build.Append("ReportId: " + i.ReportId.ToString() + " ");
                        build.Append("Rssi: " + i.Rssi.ToString() + " ");
                        build.Append("Speed: " + i.Speed.ToString() + " ");
                        build.Append("StopTime: " + i.StopTime.ToString() + " ");

                        logger.Info(build.ToString());

                        if (i.Longitude != 0 && i.Longitude != 0)
                        {
                            var gpsInfo = new GPSLocation();
                            gpsInfo.deviceName = device.Name;
                            gpsInfo.RadioID = device.RadioID;
                            gpsInfo.Latitude = i.Latitude;
                            gpsInfo.Longitude = i.Longitude;
                            gpsInfo.Rssi = i.Rssi;

                            PostGpsLocation(gpsInfo);
                        }

                        PostDeviceLifeSign(device.Name, device.RadioID, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void DevicesChanged(object sender, BindableCollectionEventArgs2<Device> e)
        {
            try
            {
                logger.Info("DevicesChanged");
                lock (devices)
                {
                    switch (e.Action)
                    {
                        case NS.Shared.Common.ChangeAction.Add:
                            lock (devices)
                                devices.Add(e.ChangedObject);
                            break;
                        case NS.Shared.Common.ChangeAction.Remove:
                            {

                                Device device = null;
                                foreach (Device item in devices)
                                {
                                    if (e.ChangedObject.ID == item.ID)
                                    {
                                        device = item;
                                        break;
                                    }
                                }
                                if (device != null)
                                {
                                    devices.Remove(device);
                                }
                            }
                            break;
                        case NS.Shared.Common.ChangeAction.ItemChanged:
                            {
                                Device device = null;
                                foreach (Device item in devices)
                                {
                                    if (e.ChangedObject.ID == item.ID)
                                    {
                                        device = item;
                                        break;
                                    }
                                }
                                if (device != null)
                                {
                                    device.Update(e.ChangedObject);
                                }
                            }
                            break;
                        case NS.Shared.Common.ChangeAction.MuchChanges:
                            devices.Clear();
                            devices = m_client.LoadRegisteredDevicesFromServer();

                            foreach (var dev in m_client.LoadUnregisteredDevicesFromServer())
                            {
                                dev.Name = "Radio " + dev.RadioID;
                                devices.Add(dev);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Simulation = (bool)SimulationCheckBox.IsChecked;
            Properties.Settings.Default.Save();

            Connect();
        }

        private bool Connected = false;
        private object ConnectLock = new object();
        private void Connect()
        {
            try
            {
                lock (ConnectLock)
                {
                    if (!Connected)
                    {
                        if (Properties.Settings.Default.Simulation)
                        {
                            var gpsInfo = new GPSLocation();
                            gpsInfo.deviceName = "test";
                            gpsInfo.Latitude = 52.4897266086191;
                            gpsInfo.Longitude = 6.13765982910991;
                            gpsInfo.Rssi = (float)-59.5864372253418;

                            PostGpsLocation(gpsInfo);

                            PostDeviceLifeSign("test-online", 1, true);
                            PostDeviceLifeSign("test-offline", 2, false);

                        }
                        else
                        {
                            ConnectToTurboNet();
                        }

                        watchdogTimer.Start();

                        ConnectButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { ConnectButton.IsEnabled = false; }));
                        urlTextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { urlTextBox.IsEnabled = false; }));
                        SimulationCheckBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { SimulationCheckBox.IsEnabled = false; }));

                        Connected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(urlTextBox.Text))
            {
                Properties.Settings.Default.CdbUrl = urlTextBox.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
