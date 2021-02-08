using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace TestShuffler
{
    public sealed class ServiceProfile : Document
    {
        private readonly ConcurrentDictionary<int, Test> _courseIdToLatestTestMapping = new();

        public IReadOnlyCollection<int> CourseIds => _courseIdToLatestTestMapping.Keys.ToList();
        public IReadOnlyCollection<Test> LatestTests => _courseIdToLatestTestMapping.Values.ToList();

        public override string Id => new(nameof(ServiceProfile));

        public void Aggregate(Test test)
        {
            Ensure.NotNull(nameof(test), test);

            _courseIdToLatestTestMapping.AddOrUpdate(
                test.CourseId,
                test,
                (_, existingTest) =>
                    test.Date > existingTest.Date || (test.Date == existingTest.Date && test.Id > existingTest.Id)
                        ? test
                        : existingTest);
        }

        public Test GetLatestTest(int courseId) =>
            _courseIdToLatestTestMapping[courseId];
    }
}