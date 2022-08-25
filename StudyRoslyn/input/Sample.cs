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

    /// <summary>
    /// クラスコメント
    /// クラスコメント2行目
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// 通し番号
        /// 2行目
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 値
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 現在の時刻
        /// 日本標準時
        /// </summary>
        public DateTime Now { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public SampleSub NullData { get; set; }
        public SampleSub SubData { get; set; }
        public List<string> PrimitiveList { get; set; }
        public List<SampleSub> ObjectList { get; set; }
        public SampleSub[] ObjectArray { get; set; }

        // 1行コメント
        // 2行目

        /* 複数行コメント */

        /* 
         * 複数行コメント1
         * 複数行コメント2
         * "なんかダブルクォーテーションとか入ってるコメント"
         * 複数行コメント3
         */

        public Sample()
        {
            Id = 1;
            Value = 1;
            Now = DateTime.Now;
            Name = "なまえ";
            IsActive = true;
            SubData = new SampleSub();
            PrimitiveList = new List<string> { "aaaa", "bbbb", "cccc", "dddd"};

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

        /// <summary>
        /// メソッドコメント
        /// 2行目
        /// </summary>
        /// <param name="str">引数の説明1</param>
        /// <param name="str2">引数の説明2</param>
        /// <returns>戻り値の説明</returns>
        public string TestMethod(string str, string str2)
        {
            Console.WriteLine(str);
            Console.WriteLine(Value);
            Console.WriteLine(Name);
            return str;
        }

        /// <summary>
        /// ただのメソッドコメント
        /// </summary>
        public virtual void TestMethod2()
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
