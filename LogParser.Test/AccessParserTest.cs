using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LogParser.Test
{
    [TestFixture]
    public class AccessParserTest
    {
        [Test]
        public void AccessDirectoryParserTest()
        {
            var target = new AccessDirectoryParser();
            var result = target.Parse(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(20, result.Count());
        }

        [Test]
        public void AccessFileParserTest()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            var parser = new AccessFileParser();
            var logs = parser.Parse("access-123.log");
            var log = logs.First();
            Assert.AreEqual(10, logs.Count());
        }

        [Test]
        public void AccessLineParserTest()
        {
            var target = new AccessLineParser();
            var result = target.Parse(@"202.39.77.14 - - [06/Oct/2015:17:07:43 +0800] ""POST /cvs/ap_interface.php HTTP/1.1"" 200 289");
            Assert.AreEqual("202.39.77.14", result.ClientIp);
            Assert.AreEqual(new DateTime(2015, 10, 6, 17, 07, 43), result.DateTime);
            Assert.AreEqual("/cvs/ap_interface.php", result.Url);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(289, result.Bytes);
        }
    }
}
