using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    public class AssertMaker
    {
        // Roslynではなく、Reflectionを使ってオブジェクトの中身を見る。
        // Roslynはどっちかっつーとコードを見るので、オブジェクトの値を取ったりはしないのかな？

        // オブジェクトに対して、Assertを作成する
        public static string MakeAssert(object obj, string name)
        {
            var sb = new StringBuilder();
            Append(sb, obj, name);
            return sb.ToString();
        }

        /// <summary>
        /// 再帰処理
        /// </summary>
        /// <param name="sb">生成中のコードのバッファ</param>
        /// <param name="obj">Assert対象のオブジェクトで、クラスもプリミティブも対応</param>
        /// <param name="name">オブジェクト名</param>
        private static void Append(StringBuilder sb, object obj, string name)
        {
            var type = obj.GetType();

            // まず、送られてきたobjectが何なのか？
            // obj自体がListやDictionaryの場合は？
            // →その場合だけ特別扱いしたい。
            // objがPrimitiveの場合もあるよね？
            if (obj == null)
            {
                sb.AppendLine($"{name}.IsNull();");
            }
            else if (type.IsPrimitive)
            {
                // プリミティブ
                sb.AppendLine($"{name}.Is({obj});");
            }
            else if (type.IsValueType)
            {
                if (type.IsEnum)
                {
                    // enumはここ
                    sb.AppendLine($"{name}.Is({obj});");
                }
                else
                {
                    // struct
                    AppendNormalClassAssert(sb, obj, name);
                }
            }
            else
            {
                // クラス
                Console.WriteLine($"クラス:{type.Name}");

                if (type.IsArray)
                {
                    // 配列はここ
                    sb.AppendLine($"{name}.Length.Is();");
                    // TODO：objectがaaaa[]のような配列だと分かっている場合、どうやってaaaa[]として扱うのか？
                }

                if (type.IsGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>))
                    {
                        // リストはここ
                        sb.AppendLine($"{name}.Count.Is();");
                    }
                    else
                    {
                        // それ以外のジェネリック。辞書とか。
                    }
                }

                if (type.Name.ToLower() == "string")
                {
                    // stringとか、よく扱うクラスは特別扱いしたい。
                    sb.AppendLine($"{name}.Is(\"{obj}\");");
                }

                // それ以外の一般クラスはここ。
                AppendNormalClassAssert(sb, obj, name);


            }
        }

        // 多分、structもこの処理になる
        private static void AppendNormalClassAssert(StringBuilder sb, object obj, string name)
        {
            // TODO:例えば、DateTimeだとMinValueもDateTimeなのでMinValue.MinValue.MinValue...のような循環が出来てしまう。
            // 深度制限をかけるか、親と同じフィールド名は指せないようにしなければならない。
            // また、DateTimeは頻繁に使うので特別に処理を設ける必要がある。

            var type = obj.GetType();

            // フィールドの取得とAssert生成
            foreach (var field in type.GetFields())
            {
                // 基本的にプロパティを使うので、フィールドは普段使わない。
                var fld = type.GetField(field.Name);        // まずクラス情報からフィールド情報を取得する
                Append(sb, fld.GetValue(obj), $"{name}.{fld.Name}");
            }

            // プロパティの取得とAssert生成
            foreach (var property in type.GetProperties())
            {
                var prop = type.GetProperty(property.Name); // まずクラス情報からプロパティ情報を取得する
                Append(sb, prop.GetValue(obj), $"{name}.{prop.Name}");
            }
        }

    }

    // xUnitで使う時は、テストクラスのコンストラクタでITestOutputHelperを作って出力させて使う感じ。

    //private readonly ITestOutputHelper _output;
    //public IndexTests(CommonTestFixture fixture, ITestOutputHelper output)
    //{
    //    _fixture = fixture;
    //    _output = output;
    //}

}
