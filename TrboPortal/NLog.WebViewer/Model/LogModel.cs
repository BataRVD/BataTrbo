using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace NLog.WebViewer
{
    [DataContract]
    public class LogModel
    {
        private readonly string[] _fields;

        private readonly int _levelIndex;

        public LogModel()
        {
            Items = new List<List<string>>();
            _fields = Config.Instance.FieldsList;
            Num = _fields.Length;
            _levelIndex = _fields.Select(item => !string.IsNullOrEmpty(item) ? item.ToLowerInvariant() : string.Empty).ToList().IndexOf("level");
        }

        public LogModel(string logsPath, DateTime date, string level, int minId = 0) : this()
        {
            Initialize(logsPath, date, level, minId);
        }

        public int Num { get; private set; }

        public DateTime Date { get; set; }

        [DataMember]
        public long LastId { get; private set; }

        [DataMember]
        public string Html
        {
            get { return GetHtmlRows(); }
            set { }
        }

        public List<List<string>> Items { get; private set; }

        public void Initialize(string logsPath, DateTime date, string level, long minId = 0)
        {
            Items = new List<List<string>>();
            LastId = 0;

            Date = date;
            string logFile = Path.Combine(logsPath, Config.Instance.FileFormat.Contains("{0") ? string.Format(Config.Instance.FileFormat, date) : Config.Instance.FileFormat);

            if (!File.Exists(logFile))
            {
                return;
            }

            List<string> logItem = null;

            using (FileStream file = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var stream = new StreamReader(file))
                {
                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (line == null)
                        {
                            continue;
                        }

                        var items = line.Split(new[] { Config.Instance.Separator }, StringSplitOptions.None);

                        if (items.Length == Num)
                        {
                            bool matchingLoglevel = false;
                            try
                            {
                                var itemLevel = _levelIndex >= 0 ? items[_levelIndex] : string.Empty;
                                var itemLevelOrd = NLog.LogLevel.FromString(itemLevel).Ordinal;
                                var referenceLevelOrd = NLog.LogLevel.FromString(level).Ordinal;
                                matchingLoglevel = (itemLevelOrd >= referenceLevelOrd);
                            } catch 
                            {
                                // Could not match - allow
                                matchingLoglevel = true;
                            }

                            LastId++;

                            if (LastId > minId && matchingLoglevel)
                            {
                                logItem = new List<string>(items);
                                Items.Insert(0, logItem);
                            }
                            else
                            {
                                logItem = null;
                                // break; only if we read reverse
                            }
                        }
                        else if (logItem != null)
                        {
                            if (logItem.Count == Num)
                            {
                                logItem.Add(string.Empty);
                            }

                            logItem[Num] += line + Environment.NewLine;
                        }
                    }
                }
            }
        }

        public string GetHtmlToolbar(HttpRequest request)
        {
            string dateFilter = request.QueryString["date"];
            string logLevel = request.QueryString["logLevel"];
            string url = request.Path;

            string pathWithLogLevel, pathWithDate, datePrefix, logLevelPrefix;
            if (logLevel != null)
            {
                pathWithLogLevel = $"{url}?logLevel={logLevel}";
                datePrefix = "&date=";
            } 
            else
            {
                pathWithLogLevel = $"{url}";
                datePrefix = "?date=";
            }
            
            if (dateFilter != null)
            {
                pathWithDate = $"{url}?date={dateFilter}";
                logLevelPrefix = "&logLevel=";
            } else
            {
                pathWithDate = url;
                logLevelPrefix = "?logLevel=";
            }

            var builder = new StringBuilder();

            // Date part
            builder.AppendFormat("<a href='{1}{2}{0:yyyy-MM-dd}'>&lt;&lt;</a>", Date.AddDays(-1), pathWithLogLevel, datePrefix);
            builder.AppendFormat("<span>{0:dd.MM.yyyy}</span>", Date);

            if (Date < DateTime.Today.AddDays(-2))
            {
                builder.AppendFormat("<a href='{1}{2}{0:yyyy-MM-dd}'>&gt;&gt;</a>", Date.AddDays(1), pathWithLogLevel, datePrefix);
            }

            if (Date < DateTime.Today && Date != DateTime.Today.AddDays(-1))
            {
                builder.AppendFormat("<a href='{1}{2}{0:yyyy-MM-dd}'>Yesterday</a>", DateTime.Today.AddDays(-1), pathWithLogLevel, datePrefix);
            }

            if (Date < DateTime.Today)
            {
                builder.AppendFormat("<a href='{0}'>Today</a>", pathWithLogLevel);
            }

            if (Date == DateTime.Today)
            {
                builder.Append("<span>Today</span>");
            }

            // Log level
            string[] logLevels = new string[] {"TRACE","DEBUG","INFO","WARNING","ERROR","CRITICAL"};
            builder.Append("<label for='logLevel'>Loglevel</label>" +
                    "<select name='logLevel' onchange='location = this.value;'>");
            foreach (var level in NLog.LogLevel.AllLoggingLevels)
            {
                string selected = (string.Equals(logLevel, level.Name, StringComparison.OrdinalIgnoreCase)) ? " selected " : string.Empty;
                builder.AppendFormat("<option {3} value='{0}{1}{2}'>{2}</option>", pathWithDate, logLevelPrefix, level.Name, selected);
            }
            builder.Append("</select>");

            // Refresh
            builder.Append("<label><input type='checkbox' data-bind='checked: isAutoRefresh' />Auto refresh</label>");
            builder.Append("<button data-bind='click: updateLogs, enable: !isAutoRefresh()'>Refresh</button>");

            return builder.ToString();
        }

        public string GetHtmlTableHeader()
        {
            var builder = new StringBuilder();
            builder.Append("<tr><th>#</th>");

            for (var i = 0; i < Num; i++)
            {
                builder.AppendFormat("<th>{0}</th>", _fields[i]);
            }

            builder.Append("</tr>");
            return builder.ToString();
        }

        private string GetHtmlRows()
        {
            var builder = new StringBuilder();
            var index = LastId;

            foreach (var item in Items)
            {
                var level = _levelIndex >= 0 ? item[_levelIndex] : string.Empty;

                builder.AppendFormat("<tr class='{0}", level);
                if (item.Count > Num)
                {
                    builder.Append(" has-details' onclick='toggleDetails(this)");
                }

                builder.Append("'>");
                builder.AppendFormat("<td>{0}</td>", index--);
                for (var i = 0; i < Num; i++)
                {
                    builder.AppendFormat("<td>{0}</td>", item[i]);
                }

                builder.Append("</tr>");

                if (item.Count > Num)
                {
                    builder.AppendFormat("<tr class='details {0}'><td colspan='{1}'>{2}</td></tr>", level, Num + 1, item[Num]);
                }
            }

            return builder.ToString();
        }
    }
}