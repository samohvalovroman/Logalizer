using System.Collections.Generic;
using System.IO;

namespace LogParser
{
    public class AccessFileParser
    {

        public IEnumerable<AccessLog> Parse(string filePath)
        {
            var lineParser = new AccessLineParser();
            var lines = File.ReadLines(filePath);
            
            foreach (var line in lines)
            {
                var log = lineParser.Parse(line);
                if (log != null) yield return log;

            }
        }
    }
}
