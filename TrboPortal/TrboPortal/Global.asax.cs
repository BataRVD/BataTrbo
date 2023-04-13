using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using System.Timers;
using NLog.Config;
using NLog.LayoutRenderers;


namespace TrboPortal
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Timer watchdogTimer = new Timer(10000);
        List<Device> devices;
        private Client m_client;

        protected void Application_Start()
        {
            InitializeNLog();
            AreaRegistration.RegisterAllAreas();
            SwaggerConfig.Register();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Make sure we start the controller
            var controller = TrboNet.TurboController.Instance;
        }

        private static void InitializeNLog()
        {
//            LogManager.ThrowExceptions = true;
//            LogManager.ThrowConfigExceptions = true;
            LayoutRenderer.Register("unixtimestamp", (logEvent) => (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
            LogManager.Configuration = LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
        }

        private void trboTest()
        {
            m_client = new Client();
            var host = "145.126.127.13";
            var port = 4021;
            var user = "admin";
            var pass = "admin";
            m_client.Connect(new NS.Shared.Network.NetworkConnectionParam(host, port), new UserInfo(user, pass), ClientInitFlags.Empty);
            if (m_client.IsStarted)
            {
                m_client.GetAllWorkflowCommands();
                devices = m_client.LoadRegisteredDevicesFromServer();

                m_client.DevicesChanged += DevicesChanged;
                m_client.DeviceLocationChanged += DeviceLocationChanged;
                m_client.DeviceStateChanged += M_client_DeviceStateChanged;
                m_client.DeviceTelemetryChanged += M_client_DeviceTelemetryChanged;
                m_client.WorkflowCommandFinished += M_client_WorkflowCommandFinished;

                watchdogTimer.Elapsed += doeHenk;
                watchdogTimer.Start();
            }
        }

        private void doeHenk(object sender, ElapsedEventArgs e)
        {
            devices = m_client.LoadRegisteredDevicesFromServer();

            logger.Info("HEllo");
            if (devices.Any())
            {
                var rand = new Random();
                var device = devices.ElementAt(rand.Next(devices.Count()));
                if (device != null)
                {
                    m_client.QueryDeviceLocation(device, "Test" + DateTime.Now.ToString(), out DeviceCommand cmd);
                    logger.Debug($"Hai {cmd.ToString()}");
                }
            }
        }


        private void M_client_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            logger.Debug($"M_client_DeviceStateChanged ");
        }

        private void M_client_WorkflowCommandFinished(object sender, WorkflowCommandFinishedEventArgs e)
        {
            logger.Debug($"M_client_WorkflowCommandFinished {e.Command.Description}");
        }

        private void M_client_DeviceTelemetryChanged(object sender, DeviceTelemetryChangedEventArgs e)
        {
            logger.Debug($"M_client_DeviceTelemetryChanged {e.State}");
        }

        private void M_client_TransmitReceiveChanged(object sender, TransmitReceiveArgs e)
        {
            logger.Debug($"M_client_TransmitReceiveChanged {e.Info.State}");
        }

        private void DeviceLocationChanged(object sender, DeviceLocationChangedEventArgs e)
        {
            logger.Debug($"DeviceLocationChanged {e.GPSData.First().Latitude}");
        }

        private void DevicesChanged(object sender, BindableCollectionEventArgs2<Device> e)
        {
            logger.Debug($"DevicesChanged ");
        }
    }
}