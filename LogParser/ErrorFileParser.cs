using System.Collections.Generic;
using System.IO;

namespace LogParser
{
    public class ErrorFileParser
    {

        public IEnumerable<ErrorLog> Parse(string filePath)
        {
            var lineParser = new ErrorLineParser();
            var lines = File.ReadLines(filePath);

            foreach (var line in lines)
            {
                var log = lineParser.Parse(line);
                if (log != null) yield return log;

            }
        }
    }
}
