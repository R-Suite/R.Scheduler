using System.Collections.Generic;
using System.IO;
using R.Scheduler.PipesAndFilters.Interfaces;

namespace R.Scheduler.FakePafFilter1
{
    public class Filter : IFilter<string> 
    {
        public IEnumerable<string> Execute(IEnumerable<string> input)
        {
            using (var writer = new StreamWriter("FakePafFilter1.txt", append: false))
            {
                foreach (var inp in input)
                {
                    var transformedData = inp + "_processedByFilter1";
                    writer.Write(transformedData);

                    yield return transformedData;
                }
            }
        }
    }
}
