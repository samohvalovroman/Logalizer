using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;


namespace LogParser
{
    public class AccessLineParser
    {
        public AccessLog Parse(string line)
        {
            //64.242.88.10 - - [07 / Mar / 2004:16:05:49 - 0800] "GET /twiki/bin/edit/Main/Double_bounce_sender?topicparent=Main.ConfigurationVariables HTTP/1.1" 401 12846
            var reg = new Regex(@"(\S*) (\S*) (\S*) \[(.*)\] (\""(\S*) (\S*) (\S*)\"") (\d*) (\d*)", RegexOptions.Compiled);
            var collections = reg.Matches(line);
            if (collections.Count == 0)
                return null;

            var matchs = collections[0];

            var log = new AccessLog()
            {
                ClientIp = matchs.Groups[1].Value,
                DateTime = ParseDateTime(matchs.Groups[4].Value),
                Url = matchs.Groups[7].Value,
                StatusCode = int.Parse(matchs.Groups[9].Value),
                Bytes = GetBytes(matchs),
                ResourcePath = GetResourcePath(matchs.Groups[7].Value),
            };
            return log;
        }

        private string GetResourcePath(string value)
        {
            var containsQueryString = value.Contains('?');
            if (!containsQueryString) return value;
            else return value.Split('?')[0];
        }

        private static int? GetBytes(Match matchs)
        {
            string value = matchs.Groups[10].Value;
            if (value == "-" || value == "") return null;
            return int.Parse(value);
        }

        private DateTime ParseDateTime(string input)
        {
            //06/Oct/2015:17:07:42 +0800
            var dateTime = input.Split(' ')[0];
            return DateTime.ParseExact(dateTime, "dd/MMM/yyyy:HH:mm:ss", new CultureInfo("en-US"));
        }


    }

    public class AccessLog
    {
        public string ClientIp { get; set; }
        public DateTime DateTime { get; set; }
        public string Url { get; set; }

        public int StatusCode { get; set; }
        public int? Bytes { get; set; }

        public string ResourcePath { get; set; }

    }    
}

