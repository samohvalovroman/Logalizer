using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogParser
{
    public class AccessDirectoryParser
    {
        public IEnumerable<AccessLog> Parse(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "access-*.log");
            var parser = new AccessFileParser();
            foreach (var file in files.AsParallel())
            {
                var logs = parser.Parse(file);
                foreach (var log in logs)
                {
                    yield return log;
                }
            }
        }        
    }
}
