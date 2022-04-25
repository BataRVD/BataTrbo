using NLog;
using NS.Enterprise.Objects.Devices;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TrboPortal.Mappers;
using TrboPortal.Model.Db;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.TrboNet
{
    /// <summary>
    /// Note, there are some other partial classes
    /// </summary>
    public sealed partial class TurboController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // instance specific 
        private static TurboController instance;
        private static readonly object LockObject = new object();

        // Total number of location request messages sent to MotoTrboServer since startup
        public static long LocationRequestCounter { get; private set; } = 0;
        // Total number of location response messages received from MotoTrboServer since startup
        public static long LocationResponseCounter { get; private set; } = 0;

        // some clients
        private static CiaBata.CiaBata ciaBataController;

        private static System.Timers.Timer heartBeat;

        private static DateTime lastLifeSign = DateTime.Now;

        private static object locki = new object();

        #region Instance

        public TurboController()
        {
            ConfigureAndStart();
        }

        private void ConfigureAndStart()
        {
            logger.Info("Starting the Controller!");

            CreateTrboClient();

            // fallback - load defaults
            LoadDefaultSettings();
            ciaBataController = new CiaBata.CiaBata(ciaBataUrl);
            // Start HeartBeat
            heartBeat = new System.Timers.Timer();
            heartBeat.Interval = serverInterval;
            heartBeat.Elapsed += TheServerDidATick;
            heartBeat.AutoReset = true;
            heartBeat.Enabled = true;


            // overwrite with latest values
            LoadSettingsFromDatabaseAsync().Wait();

            // Create CiabataControler
            Connect();
        }

        private void CreateTrboClient()
        {
            trboNetClient = new Client.TrboClient();
            trboNetClient.DevicesChanged(DevicesChanged);
            trboNetClient.DeviceLocationChanged(DeviceLocationChanged);
            trboNetClient.DeviceStateChanged(DeviceStateChanged);
            trboNetClient.TransmitReceiveChanged(TransmitReceiveChanged);
            trboNetClient.DeviceTelemetryChanged(DeviceTelemetryChanged);
            trboNetClient.WorkflowCommandFinished(WorkflowCommandFinished);

        }

        private void TheServerDidATick(object sender, ElapsedEventArgs e)
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(locki, ref lockTaken);
                if (lockTaken)
                {
                    try
                    {
                        logger.Trace("The server did a tick");
                        // populate the queue
                        PopulateQueue();
                        // Request info for next device
                        HandleQueue();

                        // Check if we need to let know that we are still alive (every minute)
                        if ((DateTime.Now - lastLifeSign).TotalMinutes > 1)
                        {
                            lastLifeSign = DateTime.Now;
                            ciaBataController.PostDeviceLifeSign(0, Environment.MachineName, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Something went horribly wrong during the ServerTick");
                    }
                }
                else
                {
                    logger.Warn("OMG! CPR! Server skipped a beat...");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(locki);
                }
            }
        }

        public static TurboController Instance
        {
            get
            {
                lock (LockObject)
                {
                    if (instance == null)
                    {
                        instance = new TurboController();
                    }

                    return instance;
                }
            }
        }

        public long GetInternalTrboNetQueueSize()
        {
            return trboNetClient.GetInternalQueueCount();
        }

        #endregion

        private async Task PostGpsLocation(GpsMeasurement gpsMeasurement)
        {
            try
            {
                // Send to CiaBatac
                // TODO TBD: We may want to post to CiaBata after inserting to DB to prevent reporting duplicates
                await ciaBataController.PostGpsLocation(CiaBataMapper.ToGpsLocation(gpsMeasurement));
                // Save to database
                await Repository.InsertOrUpdateAsync(DatabaseMapper.Map(gpsMeasurement));
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.InnerException;
                if (innerException != null && innerException.GetType() == typeof(System.Data.SQLite.SQLiteException))
                {
                    var errorCode = ((System.Data.SQLite.SQLiteException)innerException).ResultCode;
                    if (errorCode.HasFlag(System.Data.SQLite.SQLiteErrorCode.Constraint))
                    {
                        logger.Trace($"Not storing (duplicate) GpsMeasurement for RadioID '{gpsMeasurement.RadioID}' and Timestamp '{gpsMeasurement.Timestamp}'.");
                        return;
                    }
                }
                logger.Error(ex, "Error posting (or saving) gps.");
            }
        }


        private void QueryLocation(RequestMessage rm)
        {
            int deviceID = rm.deviceID;
            int radioID = rm.radioID;
            logger.Info($"Getting location for device with DeviceID: {deviceID}, RadioID: {radioID}");
            if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInfo) && deviceInfo?.Device != null)
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.Device, "", out DeviceCommand cmd);
                LocationRequestCounter++;
                logger.Debug($"response from querydevicelocation DeviceID: {deviceID}, RadioID: {radioID}, cmd: {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device with DeviceID: {deviceID}, RadioID: {radioID}");
            }
        }

    }
}
