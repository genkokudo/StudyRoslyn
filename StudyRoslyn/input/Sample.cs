using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public interface ISample
    {
        int Value { get; set; }
        string Name { get; set; }
    }

    class Sample
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public Sample()
        {
            Value = 1;
            Name = "なまえ";
        }

        public virtual void Method()
        {
            Console.WriteLine(Value);
            Console.WriteLine(Name);
        }
    }
}
