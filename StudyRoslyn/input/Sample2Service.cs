using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.input
{
    internal class Sample2Service
    {
        private readonly ISampleService _sample;
        private readonly string _sampleString;

        public Sample2Service(ISampleService sample)
        {
            _sample = sample;
            _sampleString = sample.ToString();
        }
    }
}
