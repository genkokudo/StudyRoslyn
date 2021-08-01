using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public interface ISample
    {
        int Value { get; }
    }

    class Sample
    {
        public int Value { get; private set; }

        public virtual void Method()
        {
            Value = 1;
            Console.WriteLine(Value);
        }
    }
}
