using System;
using System.Threading.Tasks;
using TrboPortal.Model.Api;
using TrboPortal.Model.Db;
using Radio = TrboPortal.Model.Db.Radio;

namespace TrboPortal.TrboNet
{
    /// <summary>
    /// Storing and retrieving settings
    /// </summary>
    public sealed partial class TurboController
    {
        // Config section
        private static int ciabataEdition;
        private static string ciaBataUrl;
        private static string ciaBataToken;
        private static int serverInterval;
        private static string turboNetUrl;
        private static int turboNetPort;
        private static string turboNetUser;
        private static string turboNetPassword;
        private static GpsModeEnum defaultGpsMode;
        private static int defaultRequestInterval;

        private static void LoadDefaultSettings()
        {
            var settings = new SystemSettings();
            serverInterval = Math.Max(1, settings.ServerInterval ?? 15);
            ciabataEdition = settings.CiaBataSettings?.Edition ?? 52;
            ciaBataUrl = settings.CiaBataSettings?.Host;
            ciaBataToken = settings.CiaBataSettings?.Token;

            turboNetUrl = settings.TurboNetSettings?.Host;
            turboNetPort = settings.TurboNetSettings?.Port ?? 0;
            turboNetUser = settings.TurboNetSettings?.User;
            turboNetPassword = settings.TurboNetSettings?.Password;

            defaultGpsMode = settings.DefaultGpsMode ?? GpsModeEnum.None;
            defaultRequestInterval = settings.DefaultInterval ?? 60;
        }

        private async Task LoadSettingsFromDatabaseAsync()
        {
            try
            {
                await LoadGenericSettingsFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not load the settings from the database");
            }
        }

        public async Task LoadGenericSettingsFromDatabaseAsync()
        {
            try
            {
                var settings = await Repository.GetLatestSystemSettingsAsync();
                if (settings != null)
                {
                    serverInterval = Math.Max(1, settings.ServerInterval);
                    ciabataEdition = settings.BataEdition;
                    ciaBataUrl = settings.CiaBataHost;
                    ciaBataToken = settings.CiaBataToken;

                    turboNetUrl = settings.TrboNetHost;
                    turboNetPort = settings.TrboNetPort;
                    turboNetUser = settings.TrboNetUser;
                    turboNetPassword = settings.TrboNetPassword;

                    // Update Ciabata
                    ciaBataController.edition = ciabataEdition;
                    ciaBataController.url = ciaBataUrl;
                    ciaBataController.token = ciaBataToken;

                    // update heartbeat
                    heartBeat.Interval = serverInterval * 1000;

                    if (!Enum.TryParse(settings.DefaultGpsMode, out defaultGpsMode))
                    {
                        defaultGpsMode = GpsModeEnum.None;
                    }

                    defaultRequestInterval = settings.DefaultInterval;
                }
                else
                {
                    logger.Info("No generic settings in the database");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not load the generic settings from the database");
            }
        }

        private bool GetRadioByRadioID(int radioID, out Radio radio)
        {
            radio = Repository.GetRadioById(radioID).Result;
            return radio != null;
        }

        /*public List<Radio> GetRadioSettings(IEnumerable<int> radioIds)
        {
            if (radioIds == null)
            {
                radioIds = Enumerable.Empty<int>();
            }
            return Repository.GetRadiosById(radioIds);
            var selectedRadios =
                radios.Values.Where(r => (!radioIds.Any()) || radioIds.Contains(r.RadioId));
            return new List<Radio>(selectedRadios);
        }*/
    }
}