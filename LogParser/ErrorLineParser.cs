using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogParser
{
    public class ErrorLineParser
    {
        public ErrorLog Parse(string line)
        {
            //Sun Mar  7 16:05:49 2004] [info] [client 64.242.88.10] (104)Connection reset by peer: client stopped connection before send body completed
            var reg = new Regex(@"\[(.*)\] \[(\S*)\]( \[(\S*) (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\])? (.*)", RegexOptions.Compiled);
            var collections = reg.Matches(line);
            if (collections.Count == 0)
                return null;

            var matchs = collections[0];
            var log = new ErrorLog()
            {
                DateTime = ParseDateTime(matchs.Groups[1].Value),
                ErrorType = matchs.Groups[2].Value,
                ClientIp = matchs.Groups[5].Value,
                Info = matchs.Groups[6].Value
            };
            return log;
        }

        private DateTime ParseDateTime(string input)
        {
            //Sun Mar  7 16:02:00 2004
            var dateTime = input.Replace("  ", " ");
            return DateTime.ParseExact(dateTime, "ddd MMM d HH:mm:ss yyyy", new CultureInfo("en-US"));
        }
    }

    public class ErrorLog
    {
        public DateTime DateTime { get; set; }
        public string ErrorType { get; set; }
        public string ClientIp { get; set; }
        public string Info { get; set; }
    }
}
