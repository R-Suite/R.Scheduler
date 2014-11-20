using System.Collections.Generic;
using System.IO;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.FakePafFilter2
{
    public class Filter : IFilter<string>
    {
        public IEnumerable<string> Execute(IEnumerable<string> input)
        {
            using (var writer = new StreamWriter("FakePafFilter2.txt", append: false))
            {
                foreach (var inp in input)
                {
                    var transformedData = inp + "_processedByFilter2";
                    writer.Write(transformedData);

                    yield return transformedData;
                }
            }
        }
    }
}
