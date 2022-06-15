using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public class Assert
    {
        // Roslynではなく、Reflectionを使ってオブジェクトの中身を見る。
        // Roslynはどっちかっつーとコードを見るので、オブジェクトの値を取ったりはしないのかな？

        // オブジェクトに対して、Assertを作成する
        public static string MakeAssert(object obj, string name)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();

            // フィールドの取得とAsser生成
            foreach (var field in type.GetFields())
            {
                // 基本的にプロパティを使うので、フィールドは普段使わない。
                var fld = type.GetField(field.Name);        // まずクラス情報からフィールド情報を取得する
                sb.AppendLine(GetAssertStr(name, fld.Name, fld.GetValue(obj)));
            }

            // プロパティの取得とAsser生成
            foreach (var property in type.GetProperties())
            {
                var prop = type.GetProperty(property.Name); // まずクラス情報からプロパティ情報を取得する
                sb.AppendLine(GetAssertStr(name, prop.Name, prop.GetValue(obj)));
            }

            return sb.ToString();
        }

        // インスタンス名、フィールドまたはプロパティ名、値
        static string GetAssertStr(string name, string propName, object obj)
        {
            // ListではCountを、ArrayではLengthを取得してみよう。is 演算子でできる？
            //bool isBase = (obj is Base);
            if (obj == null)
            {
                Console.WriteLine("ぬるぽ");
                return $"{name}.{propName}.Is({obj});";
            }
            var type = obj.GetType();
            if (type.IsPrimitive)
            {
                // プリミティブ
            }
            else if (type.IsValueType)
            {
                if (type.IsEnum)
                {
                    // enumはここ
                }
                else
                {
                    // struct
                }
            }
            else
            {
                // クラス
                // Stringはこっち
                Console.WriteLine($"クラス:{type.Name}");

                // 配列はここ
                if (type.IsArray)
                {
                    Console.WriteLine("配列発見");
                    return $"{name}.{propName}.Is({obj});";
                }

                if (type.IsGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>))
                    {
                        Console.WriteLine("リストだー！");
                    }
                    else
                    {
                        // それ以外のジェネリック。辞書とか。
                    }
                }


                // ListとかAtray以外はGetAssertStrを再帰で呼んだらいいんじゃない？

                return $"{name}.{propName}.Is({obj});";
            }
            Console.WriteLine($"クラスではない:{type.Name}");
            return $"{name}.{propName}.Is({obj});";
        }





        // xUnitで使う時は、テストクラスのコンストラクタでITestOutputHelperを作って出力させて使う感じ。

        //private readonly ITestOutputHelper _output;
        //public IndexTests(CommonTestFixture fixture, ITestOutputHelper output)
        //{
        //    _fixture = fixture;
        //    _output = output;
        //}
    }
}
