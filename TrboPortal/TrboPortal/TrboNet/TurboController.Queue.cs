using System;
using System.Collections.Generic;
using System.Linq;
using TrboPortal.Mappers;
using TrboPortal.Model.Api;
using Radio = TrboPortal.Model.Db.Radio;

namespace TrboPortal.TrboNet
{
    /// <summary>
    /// Queueing logic
    /// </summary>
    public sealed partial class TurboController
    {
        // In memory state of the server
        private static Queue<RequestMessage> pollQueue = new Queue<RequestMessage>();


        /// <summary>
        /// populates the queue with devices that need an locationUpdate
        /// </summary>
        private void PopulateQueue()
        {
            foreach (KeyValuePair<int, DeviceInfo> device in devices)
            {
                int deviceID = device.Key;
                DeviceInfo deviceInfo = device.Value;
                if (deviceInfo == null)
                {
                    // We don't have deviceinfo
                    continue;
                }

                int radioID = deviceInfo.RadioID;
                if (!GetRadioByRadioID(radioID, out Radio radio))
                {
                    // We have no settings
                    continue;
                }

                bool isInterval = DatabaseMapper.MapGpsMode(radio.GpsMode) == GpsModeEnum.Interval;
                int minimumInterval = radio.RequestInterval;
                bool hasValidInterval = minimumInterval > 0;
                if (!isInterval || !hasValidInterval)
                {
                    // Not an interval radio
                    continue;
                }

                double secondsSinceUpdateRequest = (DateTime.Now - deviceInfo.LastUpdateRequest).TotalSeconds;
                double secondsTillUpdateRequest = minimumInterval - secondsSinceUpdateRequest;
                double secondsSinceGpsUpdate = (DateTime.Now - deviceInfo.LastGpsUpdateReceived).TotalSeconds;
                double secondsTillGpsUpdate = minimumInterval - secondsSinceGpsUpdate;

                // We're not going to queue GPS update request if either:
                // - Last update request is less then minimumInterval ago.
                // OR
                // - Last GPS update (response) is less then minimumInterval ago.
                if (secondsTillUpdateRequest > 0 || secondsTillGpsUpdate > 0)
                {
                    // Not ready for update
                    continue;
                }

                var requestMessage = CreateGpsRequestMessage(device.Value);
                // if device in queue --> continue

                deviceInfo.LastUpdateRequest = DateTime.Now;
                pollQueue.AddIfNotExists(requestMessage);
            }

        }

        internal void ClearRequestQueue(IEnumerable<int> radioIds)
        {
            if (radioIds == null || radioIds.Count() == 0)
            {
                // Remove all
                pollQueue.Remove(null, (deviceA, deviceB) => { return true; });
            }
            else
            {
                var deviceIds = radioIds
                    .Select(radioId => { GetDeviceInfoByRadioID(radioId, out DeviceInfo deviceInfo); return deviceInfo?.DeviceID; })
                    .Where(deviceId => deviceId.HasValue)
                    .Select(deviceId => deviceId.Value);

                RemoveDeviceFromQueueByDeviceID(deviceIds.ToArray());
            }
        }

        /// <summary>
        /// Bump the supplied radioID to the top of the queue, ensuring this Radio's GPS is polled next.
        /// </summary>
        /// <param name="radioID"></param>
        /// <exception cref="RadioNotFoundException"></exception>
        /// <exception cref="DeviceNotFoundException"></exception>
        /// <exception cref="InvalidGpsModeException">When Radio's GPS Mode is set to None</exception>
        public void PollForGps(int radioID)
        {
            if (!GetRadioByRadioID(radioID, out Radio radio))
            {
                throw new RadioNotFoundException(radioID, $"Could not poll info for radioID {radioID} since there was no RadioSettings available");
            }
            if (!GetDeviceInfoByRadioID(radioID, out DeviceInfo deviceInfo))
            {
                throw new DeviceNotFoundException(radioID, $"Could not poll info for radioID {radioID} since there was no DeviceInfo available");
            }

            GpsModeEnum gpsMode = DatabaseMapper.MapGpsMode(radio.GpsMode);

            if (gpsMode == GpsModeEnum.None)
            {
                throw new InvalidGpsModeException(radioID, $"Could not poll info for radioID {radioID} since GpsMode is none");
            }

            int deviceID = deviceInfo.DeviceID;
            // Since we are going to jump the queue, remove all existing requests
            RemoveDeviceFromQueueByDeviceID(deviceID);
            // Jump the queue
            var requestMessage = CreateGpsRequestMessage(deviceInfo);
            deviceInfo.LastUpdateRequest = DateTime.Now;
            pollQueue.Jump(requestMessage);
            // also remove all other requests for this device from the queue, you jumped, you didn'magically duplicate yourself, what are you a gremlin?
        }

        /// <summary>
        /// Remove all the entries from the queue with the given deviceID
        /// </summary>
        /// <param name="deviceID"></param>
        private void RemoveDeviceFromQueueByDeviceID(params int[] deviceIDs)
        {

            foreach (int deviceId in deviceIDs)
            {
                pollQueue.Remove(new RequestMessage(deviceId, -1, RequestMessage.RequestType.Gps), (deviceA, deviceB) => { return (deviceA?.deviceID == deviceB?.deviceID); });
            }
        }

        private RequestMessage CreateGpsRequestMessage(DeviceInfo device)
        {
            return new RequestMessage(device, RequestMessage.RequestType.Gps);
        }

        public List<RequestMessage> GetRequestQueue()
        {
            return pollQueue.GetQueue();
        }

        private void HandleQueue()
        {
            var requestMessage = pollQueue.Pop();
            if (requestMessage != null)
            {
                QueryLocation(requestMessage);
            }
        }

    }
}