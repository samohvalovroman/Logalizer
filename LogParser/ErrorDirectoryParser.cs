using System.Collections.Generic;
using System.IO;

namespace LogParser
{
    public class ErrorDirectoryParser
    {
        public IEnumerable<ErrorLog> Parse(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "error-*.log");
            var parser = new ErrorFileParser();
            foreach (var file in files)
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
