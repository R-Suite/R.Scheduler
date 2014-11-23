using System;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    public class CustomJob : ICustomJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Params { get; set; }
        public string JobType { get; set; }
    }
}
