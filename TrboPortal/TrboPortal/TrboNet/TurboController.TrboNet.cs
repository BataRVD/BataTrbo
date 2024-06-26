﻿using NLog;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrboPortal.Model.Db;
using Device = NS.Enterprise.Objects.Devices.Device;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.TrboNet
{
    /// <summary>
    /// TrboNet Specific logic
    /// </summary>
    public sealed partial class TurboController
    {
        // some clients
        private static Client.ITrboClient trboNetClient;

        // This is a dictionary with DeviceID --> Operational info
        private static ConcurrentDictionary<int, DeviceInfo> devices = new ConcurrentDictionary<int, DeviceInfo>();

        private object ConnectLock = new object();

        private void Connect()
        {
            try
            {
                lock (ConnectLock)
                {
                    if (!trboNetClient.IsStarted)
                    {
                        ConnectToTurboNet();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private bool ConnectToTurboNet()
        {
            bool connected = false;
            logger.Info("Connect to turbonet server");
            trboNetClient.Disconnect();
            if (!string.IsNullOrEmpty(turboNetUrl))
            {
                trboNetClient.Connect(
                    new NS.Shared.Network.NetworkConnectionParam(turboNetUrl, turboNetPort),
                    new UserInfo(turboNetUser, turboNetPassword),
                    ClientInitFlags.Empty);

                if (trboNetClient.IsStarted)
                {
                    logger.Info("Connected to turbonet server");
                    connected = true;

                    trboNetClient.GetAllWorkflowCommands();

                    ReLoadDeviceList();
                }
                else
                {
                    // remove devices
                    ClearDeviceList();
                    logger.Error("Could not connect to turbonet server");
                }
            }
            else
            {
                logger.Warn("Turbonet server not configured");
            }

            ciaBataController.PutDeviceLifeSign(0, Environment.MachineName, connected, DateTime.Now);
            return connected;
        }

        private void WorkflowCommandFinished(object sender, WorkflowCommandFinishedEventArgs e)
        {
            try
            {
                int deviceID = e.DeviceId;
                logger.Info($"WorkflowCommandFinished for deviceID {deviceID}, state: {e.RequestId.ToString()}, state: {e.Result.ToString()}");
                if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInfo) && deviceInfo.Device != null)
                {
                    deviceInfo.LastMessageReceived = DateTime.Now;
                    ciaBataController.PutDeviceLifeSign(deviceInfo.RadioID, deviceInfo.Device.Name, true, deviceInfo.LastMessageReceived);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured at the WorkflowCommandFinished event");
            }
        }

        private void DeviceTelemetryChanged(object sender, DeviceTelemetryChangedEventArgs e)
        {
            try
            {
                int deviceID = e.DeviceId;
                logger.Info($"DeviceTelemetryChanged for deviceID {deviceID}");
                if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInfo) && deviceInfo.Device != null)
                {
                    deviceInfo.LastMessageReceived = DateTime.Now;
                    ciaBataController.PutDeviceLifeSign(deviceInfo.RadioID, deviceInfo.Device.Name, true, deviceInfo.LastMessageReceived);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured at the DeviceTelemetryChanged event");
            }
        }

        private void DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceStateChanged");

                foreach (var radio in e.Infos)
                {
                    int deviceID = radio.DeviceId;
                    if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInfo))
                    {
                        logger.Info($"DeviceStateChanged [Did:{deviceID}][Rid:{deviceInfo.RadioID}]: {radio.State.ToString()}");
                        deviceInfo.LastMessageReceived = DateTime.Now;
                        Device device = deviceInfo?.Device;
                        if (device != null)
                        {
                            ciaBataController.PutDeviceLifeSign(device.RadioID, device.Name, (radio.State & DeviceState.Active) == DeviceState.Active, deviceInfo.LastMessageReceived);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }


        public bool GetDeviceInfoByRadioID(int radioID, out DeviceInfo deviceInfo)
        {
            deviceInfo = devices.Where(d => d.Value.RadioID == radioID).FirstOrDefault().Value;
            return deviceInfo != null;
        }

        private bool GetDeviceInfoByDeviceID(int deviceID, out DeviceInfo deviceInfo)
        {
            bool deviceFound = devices.TryGetValue(deviceID, out deviceInfo);
            if (!deviceFound)
            {
                logger.Warn($"Can't find device with deviceID {deviceID}");
            }
            return deviceFound;
        }

        private void DeviceLocationChanged(object sender, DeviceLocationChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceLocationChanged");

                foreach (var gpsInfo in e.GPSData)
                {
                    try
                    {
                        int deviceID = gpsInfo.DeviceID;

                        GpsMeasurement gpsMeasurement = new GpsMeasurement
                        {
                            Latitude = gpsInfo.Latitude,
                            Longitude = gpsInfo.Longitude,
                            Timestamp = gpsInfo.InfoDate.ToString(), // right formatting?
                            Rssi = gpsInfo.Rssi,
                            DeviceID = deviceID,
                        };

                        DeviceInfo deviceInfo;
                        if (!GetDeviceInfoByDeviceID(deviceID, out deviceInfo)) {
                            logger.Warn($"Could not find deviceInfo to update GPSfor device with, created it for deviceID {deviceID} ");
                            deviceInfo = new DeviceInfo(deviceID);
                            devices.TryAdd(deviceID, deviceInfo);
                        }

                        deviceInfo.LastMessageReceived = DateTime.Now;
                        deviceInfo.LastGpsUpdateReceived = DateTime.Now;
                        int radioID = deviceInfo.RadioID;
                        gpsMeasurement.RadioID = radioID;
                        deviceInfo.GpsLocations.Push(gpsMeasurement);

                        // Previous existing logging magic
                        StringBuilder build = new StringBuilder();
                        build.Append("DeviceLocationChanged");
                        build.Append("Altitude: " + gpsInfo.Altitude + " ");
                        build.Append("Description: " + gpsInfo.Description + " ");
                        build.Append("DeviceID: " + gpsInfo.DeviceID + " ");
                        build.Append("Direction: " + gpsInfo.Direction + " ");
                        build.Append("GpsSource: " + gpsInfo.GpsSource + " ");
                        build.Append("InfoDate: " + gpsInfo.InfoDate.ToString() + " ");
                        build.Append("Latitude: " + gpsInfo.Latitude.ToString() + " ");
                        build.Append("Name: " + gpsInfo.Name + " ");
                        build.Append("Radius: " + gpsInfo.Radius.ToString() + " ");
                        build.Append("ReportId: " + gpsInfo.ReportId.ToString() + " ");
                        build.Append("Rssi: " + gpsInfo.Rssi.ToString() + " ");
                        build.Append("Speed: " + gpsInfo.Speed.ToString() + " ");
                        build.Append("StopTime: " + gpsInfo.StopTime.ToString() + " ");

                        logger.Info(build.ToString());

                        PostGpsLocation(gpsMeasurement);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Adding location failed for deviceID {gpsInfo?.DeviceID}");
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
                var device = e.ChangedObject;
                logger.Info($"DevicesChanged[{e.Action}]: DeviceID: '{device?.ID}'.");
                switch (e.Action)
                {
                    case NS.Shared.Common.ChangeAction.Add:
                        AddOrUpdateDevice(device);
                        break;
                    case NS.Shared.Common.ChangeAction.Remove:
                        RemoveDevice(device?.ID);
                        break;
                    case NS.Shared.Common.ChangeAction.ItemChanged:
                        AddOrUpdateDevice(device);
                        break;
                    case NS.Shared.Common.ChangeAction.MuchChanges:
                        ReLoadDeviceList();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void RemoveDevice(int? deviceID)
        {
            if (deviceID == null)
            {
                logger.Error("Can't remove device null!");
                return;
            }

            if (!devices.TryRemove((int)deviceID, out DeviceInfo deviceInfo))
            {
                logger.Error($"Can't remove Device with DeviceID: '{deviceID}'.");
                return;
            }
            logger.Info($"Remove device for deviceID {deviceID} radio {deviceInfo?.RadioID}.");
        }


        private void ClearDeviceList()
        {
            devices.Clear();
        }

        private void ReLoadDeviceList()
        {
            List<Device> registeredDevices = trboNetClient.LoadRegisteredDevicesFromServer();
            // Unregistered devices cannot be polled - let's not add them at the moment
            // List<Device> unregisteredDevices = trboNetClient.LoadUnregisteredDevicesFromServer();

            // clear current devices
            ClearDeviceList();

            registeredDevices.ForEach(d => AddOrUpdateDevice(d));
            // post all devices
            registeredDevices.ForEach(d => ciaBataController.PutDeviceLifeSign(d.RadioID, d.Name, false, DateTime.Now));
            // unregisteredDevices.ForEach(d => AddOrUpdateDevice(d));
        }

        private void AddOrUpdateDevice(Device device)
        {
            if (device != null)
            {
                int deviceID = device.ID;
                int radioID = device.RadioID;

                logger.Info($"AddOrUpdate deviceinfo for device with deviceID {deviceID} and radioID {radioID}");
                // Add device
                devices.AddOrUpdate(deviceID, new DeviceInfo(device), (did, oldInfo) =>
                {
                    logger.Info($"Updating deviceinfo for device with deviceID {deviceID} and radioID {radioID}");
                    oldInfo.UpdateDevice(device);
                    return oldInfo;
                });
                
                // Add RadioSettings if not already exists.
                if (!GetRadioByRadioID(radioID, out Radio radio))
                {
                    radio = new Model.Db.Radio
                    {
                        RadioId = radioID,
                        Name = $"Radio - {radioID}",
                        GpsMode = defaultGpsMode.ToString(),
                        RequestInterval = defaultRequestInterval
                    };
                    var result = Repository.InsertOrUpdateRadios(new List<Radio> { radio }).Result;
                    // Assume default settings were added if insert or update returns context modification
                    if (result != 0)
                    {
                        logger.Info($"Created standard settings for Radio with radioID {radioID}");
                    }
                }
            }
            else
            {
                logger.Warn("Can't add null device.");
            }
        }

    }
}