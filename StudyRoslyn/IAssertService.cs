using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn
{
    // TODO:Dictionaryが未対応。必要になったら実装しましょう。

    // 拡張機能で使うには？
    // クラスや構造体のオブジェクトのインスタンスを生成するには、TypeクラスのCreateInstanceメソッドを使用します。

    /// <summary>
    /// オブジェクトを渡すと、そのクラス定義を読み取ってAssertを生成する
    /// </summary>
    public interface IAssertService
    {
        /// <summary>
        /// オブジェクトに対して、Assertを作成する 
        /// 循環参照による無限ループを防止するため、同時に2回以上同じフィールド名を含むAssertを生成できない。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MakeAssert(object obj, string name);
    }

    public class AssertService : IAssertService
    {
        // Roslynではなく、Reflectionを使ってオブジェクトの中身を見る。
        // Roslynはどっちかっつーとコードを見るので、オブジェクトの値を取ったりはしないのかな？

        public string MakeAssert(object obj, string name)
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
            // まず、送られてきたobjectが何なのか？
            // obj自体がListやDictionaryの場合は？
            // →その場合だけ特別扱いしたい。
            // objがPrimitiveの場合もあるよね？
            if (obj == null)
            {
                sb.AppendLine($"{name}.IsNull();");
                return;
            }

            var type = obj.GetType();
            if (type.IsPrimitive)
            {
                // プリミティブ
                if (type.Name == "Boolean")
                {
                    // "True"とか"False"とかなので小文字にする
                    sb.AppendLine($"{name}.Is({obj.ToString().ToLower()});");
                }
                else
                {
                    sb.AppendLine($"{name}.Is({obj});");
                }
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
                    if (type.Name.ToLower() == "datetime")
                    {
                        // stringとか、よく扱うクラスは特別扱いしたい。
                        sb.AppendLine($"{name}.ToString().Is(\"{obj}\");");
                    }
                    else
                    {
                        AppendNormalClassAssert(sb, obj, name);
                    }
                }
            }
            else
            {
                // クラス
                Console.WriteLine($"クラス:{type.Name}");

                if (type.IsArray)
                {
                    // 配列はここ
                    // type.Name;               // "SampleSub[]"
                    // type.GetElementType();   // "SampleSub"

                    // Lengthを取得
                    var prop = type.GetProperty("Length");
                    var length = (int)prop.GetValue(obj);
                    sb.AppendLine($"{name}.Length.Is({length});");

                    var objArray = (object[])obj;
                    foreach (var item in objArray)
                    {
                        Append(sb, item, $"{name}");
                    }
                }
                else if (type.IsGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>))
                    {
                        // リストはここ
                        var prop = type.GetProperty("Count");
                        var count = prop.GetValue(obj);
                        sb.AppendLine($"{name}.Count.Is({count});");

                        var objList = (IEnumerable)obj;
                        foreach (var item in objList)
                        {
                            Append(sb, item, $"{name}");
                        }
                    }
                    else
                    {
                        // それ以外のジェネリック。辞書とか。
                    }
                }
                else if (type.Name.ToLower() == "string")
                {
                    // stringとか、よく扱うクラスは特別扱いしたい。
                    sb.AppendLine($"{name}.Is(\"{obj}\");");
                }
                else
                {
                    // それ以外の一般クラスはここ。
                    AppendNormalClassAssert(sb, obj, name);
                }

            }
        }

        // 多分、structもこの処理になる
        private static void AppendNormalClassAssert(StringBuilder sb, object obj, string name)
        {
            var type = obj.GetType();

            // フィールドの取得とAssert生成
            foreach (var field in type.GetFields())
            {
                // 基本的にプロパティを使うので、フィールドは普段使わない。
                var fld = type.GetField(field.Name);        // まずクラス情報からフィールド情報を取得する
                if (CheckName(name, fld.Name))              // 循環参照チェックをする
                {
                    Append(sb, fld.GetValue(obj), $"{name}.{fld.Name}");
                }
            }

            // プロパティの取得とAssert生成
            foreach (var property in type.GetProperties())
            {
                var prop = type.GetProperty(property.Name); // まずクラス情報からプロパティ情報を取得する
                if (CheckName(name, prop.Name))
                {
                    Append(sb, prop.GetValue(obj), $"{name}.{prop.Name}");
                }
            }

        }

        /// <summary>
        /// 循環参照対策
        /// 名前をチェックして、続けて生成しても良いか判定する
        /// </summary>
        /// <param name="name">ピリオドで繋いだ今までのパンくず形式の名前</param>
        /// <param name="propName">生成対象名</param>
        /// <returns></returns>
        private static bool CheckName(string name, string propName)
        {
            // 例えば、DateTimeだとMinValueもDateTimeなのでMinValue.MinValue.MinValue...のような循環が出来てしまう。
            // MinValue.MaxValue.MinValue...のパターンもある。

            //var splitedName = name.Split('.');
            //var lastName = splitedName[splitedName.Length - 1];
            //return lastName != propName;

            // ↑の方法じゃダメだったので単純にこっちで。
            // 今までの名前と同名のフィールドは生成対象にしない。
            return !name.Contains(propName);

            // 他に考えられる方法だと、同じ型が登場したら拒否するとかかなあ？DateTypeの中のDateTypeは対象外にするとか。
            //→それだと、多対多の構造でループすると思う。
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
