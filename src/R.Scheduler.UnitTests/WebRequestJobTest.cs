using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Quartz;
using R.Scheduler.Core;
using R.Scheduler.Interfaces;
using Xunit;
using Match = System.Text.RegularExpressions.Match;

namespace R.Scheduler.UnitTests
{
    public class WebRequestJobTest
    {
        [Fact]
        public void ShouldUseRegexToParseTokensInWebRequestUri()
        {
            // Arrange
            string uri = @"http://localhost:9000/api/jobs/execution?JobName=TestJob?FireInstanceId={$FireInstanceId}&test={$test}";

            // Act 
            var r = new Regex(Regex.Escape("{$") + "(.*?)" + Regex.Escape("}"));
            MatchCollection matches = r.Matches(uri);

            // Assert
            Assert.Equal(matches.Count, 2);
            Assert.Equal("{$FireInstanceId}", matches[0].ToString());
            Assert.Equal("{$test}", matches[1].ToString());
        }
    }
}
