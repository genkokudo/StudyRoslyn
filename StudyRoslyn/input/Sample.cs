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

    public class Sample
    {
        public long Id { get; set; }
        public int Value { get; set; }
        public DateTime Now { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public SampleSub NullData { get; set; }
        public SampleSub SubData { get; set; }
        public List<string> PrimitiveList { get; set; }
        public List<SampleSub> ObjectList { get; set; }
        public SampleSub[] ObjectArray { get; set; }

        public Sample()
        {
            Id = 1;
            Value = 1;
            Now = DateTime.Now;
            Name = "なまえ";
            IsActive = true;
            SubData = new SampleSub();
            PrimitiveList = new List<string>();

            ObjectList = new List<SampleSub>
            {
                new SampleSub { IntData = 1, StringData = "a" },
                new SampleSub { IntData = 2, StringData = "b" },
                new SampleSub { IntData = 3, StringData = "c" }
            };

            ObjectArray = new SampleSub[] { 
                new SampleSub { IntData = 11, StringData = "A" },
                new SampleSub { IntData = 22, StringData = "B" }, 
                new SampleSub { IntData = 33, StringData = "C" } 
            };
        }

        public virtual void Method()
        {
            Console.WriteLine(Value);
            Console.WriteLine(Name);
        }
    }

    public class SampleSub
    {
        public int IntData { get; set; }
        public string StringData { get; set; }
        public SampleSub()
        {
            IntData = 1;
            StringData = "なまえ";
        }
    }
}
