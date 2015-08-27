using System.Text.RegularExpressions;
using Xunit;

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
