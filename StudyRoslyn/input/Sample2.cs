using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public interface ISample2
    {
        int Value { get; }
    }

    class Sample2
    {
        public int Value { get; private set; }

        public virtual void Method()
        {
            Value = 1;
            Console.WriteLine(Value);
        }
    }
}
