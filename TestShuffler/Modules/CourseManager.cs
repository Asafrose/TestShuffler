using Humanizer;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestShuffler
{
    public interface ICourseManager
    {
        Task<IReadOnlyCollection<int>> GetCourseIdsAsync();
        Task<Course> GetCourseAsync(int courseId);
    }

    public class CourseManager : Module, ICourseManager
    {
        private readonly IDatabase _database;
        private readonly IServiceProfileManager _serviceProfileManager;

        private readonly ExtendedCache<int, Course> _courseCache;
        private ConcurrentBag<Course> _upsertCoursesBag;

        public CourseManager(IDatabase database, IServiceProfileManager serviceProfileManager)
        {
            _database = database;
            _serviceProfileManager = serviceProfileManager;
            _upsertCoursesBag = new ConcurrentBag<Course>();
            _courseCache = new ExtendedCache<int, Course>(1.Days(), _ => _upsertCoursesBag.Add(_));

            RegisterPeriodicTask(UpsertCourses, 10.Minutes());
        }

        public async Task<IReadOnlyCollection<int>> GetCourseIdsAsync() =>
            (await _serviceProfileManager.GetServiceProfileAsync()).CourseIds;

        public async Task<Course> GetCourseAsync(int courseId) =>
            _courseCache.Contains(courseId)
                ? _courseCache.Get(courseId)
                : _courseCache.GetOrAdd(
                    courseId,
                    (await GetCourseIdsAsync()).Contains(courseId)
                        ? (Course)(await _database.DocumentsCollection.FindAsync(_ => _.Id == courseId.ToString(), cancellationToken: CancellationToken)).Single()
                        : new Course(courseId));

        private async Task UpsertCourses()
        {
            var currentBag = _upsertCoursesBag;
            _upsertCoursesBag = new ConcurrentBag<Course>();

            if (currentBag.Count == 0)
            {
                return;
            }

            await _database.DocumentsCollection.BulkWriteAsync(
                currentBag.Select(
                    course => new ReplaceOneModel<Document>(
                        Builders<Document>.Filter.Where(storedCourse => storedCourse.Id == course.Id),
                        course)
                    {
                        IsUpsert = true
                    }).ToList(),
                cancellationToken: CancellationToken);
        }
    }
}