using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public interface ISample
    {
        int Value { get; }
        string Name { get; }
    }

    class Sample
    {
        public int Value { get; private set; }
        public string Name { get; private set; }

        public virtual void Method()
        {
            Value = 1;
            Name = "なまえ";
            Console.WriteLine(Value);
            Console.WriteLine(Name);
        }
    }
}
