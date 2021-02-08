using System;

namespace TestShuffler
{
    public sealed record Test(int CourseId, DateTime Date, int Id)
    {
        private const string _urlFormat = "https://mtamn.mta.ac.il/info/Scans/{0}_{1}_Tests/{0}_{1}_{2}.pdf";

        public Uri Uri => new(string.Format(_urlFormat, CourseId, Date.ToString("ddMMyyyy"), Id));
    }
}