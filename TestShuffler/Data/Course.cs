using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace TestShuffler
{
    public sealed class Course : Document
    {
        private readonly ConcurrentDictionary<DateTime, ConcurrentBag<int>> _dateToTestIdsMapping;

        private readonly int _courseId;
        public override string Id => new(_courseId.ToString());

        public Course(int id)
        {
            _courseId = id;
            _dateToTestIdsMapping = new ConcurrentDictionary<DateTime, ConcurrentBag<int>>();
        }

        public IReadOnlyCollection<DateTime> GetTestDates() => _dateToTestIdsMapping.Keys.ToList();
        public IReadOnlyCollection<int> GetTestIds(DateTime date) => _dateToTestIdsMapping[date];

        public void AddTest(Test test)
        {
            Ensure.NotNull(nameof(test), test);

            if (_courseId != test.CourseId)
            {
                throw new Exception("Cannot Aggregate Courses with different ids");
            }

            _dateToTestIdsMapping.AddOrUpdate(
                test.Date,
                new ConcurrentBag<int>(
                    new[]
                    {
                        test.Id
                    }),
                (_, existingTestIds) =>
                {
                    existingTestIds.Add(test.Id);

                    return existingTestIds;
                });
        }
    }
}