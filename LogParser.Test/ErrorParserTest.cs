using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LogParser.Test
{
    [TestFixture]
    public class ErrorParserTest
    {
        [Test]
        public void ErrorDirectoryParserTest()
        {
            var target = new ErrorDirectoryParser();
            var result = target.Parse(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(20, result.Count());
        }

        [Test]
        public void ErrorFileParserTest()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            var parser = new ErrorFileParser();
            var logs = parser.Parse("error-123.log");
            var log = logs.First();
            Assert.AreEqual(10, logs.Count());
        }

        [Test]
        public void ErrorLineParserTest()
        {
            var target = new ErrorLineParser();
            var result = target.Parse(@"[Sun Mar  7 16:05:49 2004] [info] [client 64.242.88.10] (104)Connection reset by peer: client stopped connection before send body completed");
            Assert.AreEqual(new DateTime(2004,3,7,16,05,49), result.DateTime);
            Assert.AreEqual("64.242.88.10", result.ClientIp);
            Assert.AreEqual("info", result.ErrorType);
            Assert.AreEqual("(104)Connection reset by peer: client stopped connection before send body completed", result.Info);
            Assert.AreEqual(new DateTime(2004, 3, 7, 16, 05, 49), result.DateTime);
        }
    }
}
