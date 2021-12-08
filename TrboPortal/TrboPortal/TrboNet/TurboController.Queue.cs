using System;
using System.Collections.Generic;
using System.Linq;
using TrboPortal.Model.Api;
using Radio = TrboPortal.Model.Api.Radio;

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

                bool isInterval = radio.GpsMode == GpsModeEnum.Interval;
                int minimumInterval = (radio.RequestInterval ?? 0);
                bool hasValidInterval = minimumInterval > 0;
                if (!isInterval || !hasValidInterval)
                {
                    // Not an interval radio
                    continue;
                }

                double secondsSinceUpdate = (DateTime.Now - deviceInfo.LastUpdateRequest).TotalSeconds;
                double secondsTillUpdate = minimumInterval - secondsSinceUpdate;
                if (secondsTillUpdate > 0)
                {
                    // Not ready for update
                    continue;
                }

                var requestMessage = CreateGpsRequestMessage(deviceID);
                // if device in queue --> continue

                deviceInfo.LastUpdateRequest = DateTime.Now;
                pollQueue.Add(requestMessage);
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

        public void PollForGps(IEnumerable<int> radioIds)
        {
            foreach (int radioID in radioIds)
            {
                if (GetDeviceInfoByRadioID(radioID, out DeviceInfo deviceInfo) && GetRadioByRadioID(radioID, out Radio radio))
                {
                    GpsModeEnum gpsMode = radio?.GpsMode ?? GpsModeEnum.None;
                    if (gpsMode != GpsModeEnum.None)
                    {
                        int deviceID = deviceInfo.DeviceID;
                        // Since we are going to jump the queue, remove all existing requests
                        RemoveDeviceFromQueueByDeviceID(deviceID);
                        // Jump the queue
                        var requestMessage = CreateGpsRequestMessage(deviceID);
                        deviceInfo.LastUpdateRequest = DateTime.Now;
                        pollQueue.Jump(requestMessage);
                        // also remove all other requests for this device from the queue, you jumped, you didn'magically duplicate yourself, what are you a gremlin?
                    }
                    else
                    {
                        logger.Info($"Could not poll info for radioID {radioID} since GpsMode is none");
                    }
                }
                else
                {
                    logger.Info($"Could not poll info for radioID {radioID} since there was no deviceInfo or RadioSettings available");
                }
            }
        }

        /// <summary>
        /// Remove all the entries from the queue with the given deviceID
        /// </summary>
        /// <param name="deviceID"></param>
        private void RemoveDeviceFromQueueByDeviceID(params int[] deviceIDs)
        {
            foreach (int deviceId in deviceIDs)
            {
                pollQueue.Remove(new RequestMessage(deviceId, RequestMessage.RequestType.Gps), (deviceA, deviceB) => { return (deviceA?.deviceID == deviceB?.deviceID); });
            }
        }

        private RequestMessage CreateGpsRequestMessage(int deviceID)
        {
            return new RequestMessage(deviceID, RequestMessage.RequestType.Gps);
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