using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Humanizer;

namespace TestShuffler
{
    public interface ITestScraper
    {
        public Task<ScrapeResult> ScrapeAsync(int courseCode);
    }

    public sealed class TestScraper : Module ,ITestScraper
    {
        private static readonly DateTime _startDate = DateTime.Parse("1.1.2016", CultureInfo.InvariantCulture);

        private readonly ICourseManager _courseManager;
        private readonly IServiceProfileManager _serviceProfileManager;

        private readonly HttpClient _httpClient = new();
        private readonly ExtendedActionBlock<(int courseId, DateTime startDate, int startTestId)> _courseScraperBlock;
        private readonly ExtendedActionBlock<(int courseId, DateTime date, int startTestId)> _courseDateScrapeBlock;
        private readonly ExtendedActionBlock<Test> _testScrapeBlock;

        public TestScraper(ICourseManager courseManager, IServiceProfileManager serviceProfileManager)
        {
            _courseManager = courseManager;
            _serviceProfileManager = serviceProfileManager;

            _courseScraperBlock =
                new ExtendedActionBlock<(int courseId, DateTime startDate, int startTestId)>(
                    ScrapeCourseAsync,
                    50,
                    1000);
            _courseDateScrapeBlock =
                new ExtendedActionBlock<(int courseId, DateTime date, int startTestId)>(
                    ScrapeCourseDateAsync,
                    50,
                    1000);
            _testScrapeBlock =
                new ExtendedActionBlock<Test>(
                    ScrapeTestAsync,
                    50,
                    1000);

            RegisterPeriodicTask(UpdateCoursesAsync, 1.Days());
        }

        public async Task<ScrapeResult> ScrapeAsync(int courseCode)
        {
            if ((await _serviceProfileManager.
                GetServiceProfileAsync()).
                CourseIds.
                Contains(courseCode))
            {
                return ScrapeResult.AlreadyExist;
            }

            return _courseScraperBlock.Post((courseCode, _startDate, 0))
                ? ScrapeResult.Started
                : ScrapeResult.SystemBusy;
        }

        private async Task UpdateCoursesAsync()
        {
            foreach (var (courseId, dateTime, id) in (await _serviceProfileManager.GetServiceProfileAsync()).LatestTests)
            {
                await _courseScraperBlock.SendAsync((courseId,dateTime,id));
            }
        }

        private async Task ScrapeTestAsync(Test test)
        {
            if (await IsTestExistAsync(test))
            {
                (await _courseManager.GetCourseAsync(test.CourseId)).AddTest(test);
                (await _serviceProfileManager.GetServiceProfileAsync()).Aggregate(test);
            }
        }

        private async Task ScrapeCourseAsync((int courseCode, DateTime startDate, int startTestId) tuple)
        {
            var (courseCode, startDate, startTestId) = tuple;
            await _courseDateScrapeBlock.SendAsync((courseCode,startDate.Date,startTestId));

            foreach (var elapsedDate in startDate.GetElapsedDates().Skip(1))
            {
                await _courseDateScrapeBlock.SendAsync((courseCode, elapsedDate, 0));
            }
        }

        private async Task ScrapeCourseDateAsync((int courseCode, DateTime date, int startTestId) tuple)
        {
            var (courseCode, date, startTestId) = tuple;

            if (!await IsTestExistAsync(new Test(courseCode, date, 0)))
            {
                return;
            }

            foreach (var testId in Enumerable.Range(startTestId, 30))
            {
                await _testScrapeBlock.SendAsync(new Test(courseCode, date, testId));
            }
        }

        private async Task<bool> IsTestExistAsync(Test test)
        {
            var response = await _httpClient.HeadAsync(test.Uri);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return false;
        }
    }

    public enum ScrapeResult
    {
        AlreadyExist,
        Started,
        SystemBusy
    }
}