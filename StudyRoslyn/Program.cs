using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Extensions.DependencyInjection;
using StudyRoslyn.input;
using StudyRoslyn.Labo;
using StudyRoslyn.Summary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace StudyRoslyn
{
    public class Program
    {

        /// <summary>
        /// 現在の登録サービス一覧を取得する
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetRegisterdServiceList(DiLibrary library)
        {
            var result = new List<string>();
            return result;  // TODO:Distinct
        }

        // DiLibrary.CommunityToolkit の場合について
        // どんなパターンがあるか分からないので、取り敢えずnew ServiceCollection()を行ってる場合のみ対応させておく。

        /// <summary>
        /// 指定したサービスをDI登録する
        /// </summary>
        /// <param name="library"></param>
        /// <param name="filePath"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static void RegisterdService(DiLibrary library, string filePath, string className, string methodName, string serviceName)
        {
            // 対象のソースコードを読み込む
            var source = File.ReadAllText(filePath);

            // シンタックス ツリーに変換してルートのノードを取得する
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var rootNode = syntaxTree.GetRoot();

            // 対象のメソッドのノードを特定して取得
            var walker = new SearchWalker();
            var targetNode = walker.SearchMethod(rootNode, className, methodName);
            if (targetNode == null) return;

            // 書き換え後の記述をセット
            if (library == DiLibrary.CommunityToolkit)
            {
                // コンストラクタかメソッドかで分岐してしまう
                if (targetNode.GetType().Name == nameof(MethodDeclarationSyntax))
                {
                    Console.WriteLine();
                }
                else if (targetNode.GetType().Name == nameof(ConstructorDeclarationSyntax))
                {
                    // ※ここ無理なので諦める。
                    // メソッドチェーンをRoslynで作るのが無理。

                    Console.WriteLine("---- before ----");
                    Console.WriteLine(targetNode);

                    // 取り敢えず
                    // コンストラクタ版
                    var newConstructor = targetNode as ConstructorDeclarationSyntax;

                    // "ConfigureServices"を含む命令を取得
                    var targetStatement = newConstructor.Body.Statements.FirstOrDefault(x => x.GetText().ToString().Contains("ConfigureServices"));
                    if (targetStatement == null) return;

                    // Ioc.Default.ConfigureServices(new ServiceCollection()
                    //     .AddTransient<ITestService, TestService>()
                    //     .AddSingleton<ITest2Service, Test2Service>()
                    //     .AddSingleton<ITest3Service, Test3Service>();
                    var expressionStatement = targetStatement as ExpressionStatementSyntax;

                    // ----"new ServiceCollection()"を取得----
                    var oldSyntax = targetStatement.DescendantNodes().FirstOrDefault(x => x.GetType().Name == nameof(ObjectCreationExpressionSyntax));
                    if (oldSyntax == null) return;
                    var oldTest = oldSyntax as ObjectCreationExpressionSyntax;
                    var newTest = oldTest.WithArgumentList(oldTest.ArgumentList.AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(serviceName))));
                    targetNode = targetNode.ReplaceNode(oldTest, newTest);
                    // ----これで書き換えできるんだけど、ArgumentListってのは引数を追加するだけだからまちがい。----

                    // そもそも、メソッドチェーンをRoslynで処理しようとするとどんどん階層が増えていってしまうので、向かないのかも？
                    // 末尾を取得して、そこに追加するというのがいいのかもしれない。
                    // ↑それもダメ。噛んでるから。

                    // となると、一覧取得もこれと同じくらい無理っぽい。
                    // 一覧取得さえできれば、無理矢理構文作ってくっつければ以下の方法で取り敢えずできそう。
                    //var expressionSyntax = targetStatement as ExpressionStatementSyntax;
                    //expressionSyntax = expressionSyntax.WithExpression(
                    //    SyntaxFactory.ParseExpression(
                    //        $"Ioc.Default.ConfigureServices(new ServiceCollection().AddTransient<{serviceName}, {serviceName}>())"
                    //    )
                    //);

                    Console.WriteLine("---- after ----");
                    Console.WriteLine(targetNode.NormalizeWhitespace());

                }
            }
            else if (library == DiLibrary.HostedCommunityToolkit)
            {
                // コンストラクタかメソッドかで分岐してしまう
                if (targetNode.GetType().Name == nameof(MethodDeclarationSyntax))
                {
                    var newMethod = targetNode as MethodDeclarationSyntax;

                    // メソッドからIServiceCollectionパラメータを探す
                    var identifier = string.Empty;
                    foreach (var item in newMethod.ParameterList.Parameters)
                    {
                        if (item.Type.GetText().ToString().Contains("ServiceCollection"))
                        {
                            identifier = item.Identifier.Text;  // "services"
                        }
                    }

                    newMethod = newMethod.AddBodyStatements(SyntaxFactory.ParseStatement($"{identifier}.AddSingleton(IAaaaService, AaaaService);")
                        .WithLeadingTrivia(newMethod.Body.OpenBraceToken.LeadingTrivia.Add(SyntaxFactory.Whitespace("    ")))   // インデントを前のメソッドから取ってきて付ける
                        .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)                                               // 改行を付ける
                        );
                    Console.WriteLine("-------------------------------- 書き換える --------------------------------");
                    // NormalizeWhitespace()は空行が消されるからダメ
                    // WithAdditionalAnnotations(Formatter.Annotation)は実行後にフォーマットしてくれるとかなんとか？？？？
                    Console.WriteLine(newMethod.WithAdditionalAnnotations(Formatter.Annotation).ToFullString());
                }
                else if (targetNode.GetType().Name == nameof(ConstructorDeclarationSyntax))
                {
                    // コンストラクタ版
                    Console.WriteLine();
                }
            }


            //Console.WriteLine("-------------------------------- 書き換える --------------------------------");
            //Console.WriteLine(newMethod.NormalizeWhitespace());
            //// 適用する
            //Console.WriteLine("-------------------------------- 書き換え後 --------------------------------");
            //var newTree = rootNode.ReplaceNode(targetNode, newMethod);
            //Console.WriteLine(newTree.NormalizeWhitespace());

            // コンストラクタ書き換えは多分これを参考に。
            // http://mousouprogrammer.blogspot.com/2014/
        }


        // Ioc.Default.ConfigureServicesを行っているクラスを探す
        // ConfigureServicesを探せばいいけど、パターンが複数ある
        // ・IServiceCollectionを実装したコレクションが引数にあるメソッド
        // ・IServiceCollectionを実装したクラスがnewされている処理があるメソッド(new ServiceCollectionがある処理)
        private static void SearchConfigureServices()
        {
            // 対象のソースコードを読み込む
            var source = File.ReadAllText("./input/DiSample.cs");

            // シンタックス ツリーに変換してルートのノードを取得する
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var rootNode = syntaxTree.GetRoot();

            Console.WriteLine("-------------------------------- DI探し --------------------------------");
            // DIを行なっているクラスとメソッドを見つける
            var walker = new DiWalker();
            var aaaa = walker.FindDiClass(rootNode);
            Console.WriteLine(aaaa);

            Console.WriteLine("-------------------------------- DI登録 --------------------------------");
            // 見つけるのは出来たから、次は登録方法。
            // パターンが2つあるのでどちらなのかを自動判別。
            // 
            // DiSampleをそれぞれのパターンで実験。
            RegisterdService(walker.DiPattern, "./input/DiSample.cs", walker.DiClassName, walker.DiMethodName, "TesTesService");

            Console.WriteLine("-------------------------------- 現在の登録サービス一覧 --------------------------------");
            // 現在の登録サービス一覧を取得する
            // DI登録はファイル・クラス・メソッドが分かっている状態。
            Console.WriteLine(walker.DiPattern);
            var serviceList = GetRegisterdServiceList(walker.DiPattern);
            foreach (var service in serviceList)
            {
                Console.WriteLine(service);
            }

            //Console.WriteLine("-------------------------------- コード変更 --------------------------------");
            //Console.WriteLine("----- ReplaceNodeによるコード変更 -----");
            //// https://www.thinktecture.com/en/net/roslyn-source-generators-analyzers-code-fixes/
            //// 上のNodeの中から、TypeがClassDeclarationSyntaxの物を指定することでクラス定義を見つける。
            //var oldClass = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            //var newKlass = oldClass.WithIdentifier(SyntaxFactory.ParseToken("Aaaa"));               // クラス名を変更
            //var rewriten = rootNode.ReplaceNode(oldClass, newKlass);
            //Console.WriteLine(rewriten.NormalizeWhitespace()); // 整形して表示


            //Console.WriteLine("----- SyntaxRewriterによるコード変更 -----");
            //// http://blog.shos.info/archives/2013/12/csharp_roslynsyntaxrewriter.html
            //// これは、SyntaxRewriterを継承して作成したクラスにメソッドをオーバーライドすることで
            //// どのノードを変更するかを指定して、そのノードを変更する処理を記述する。
            //// そしてノードのルートに対してVisitメソッドを呼び出すことで、ソースの修正ができる。

        }

        static void Main(string[] args)
        {
            SearchConfigureServices();

            // inputフォルダのファイルを全て読み込む
            string[] filenames = Directory.GetFiles("./input", "*.cs", SearchOption.AllDirectories);

            #region ソースコードからサービス名か、インタフェース名を取得してみる
            //foreach (var filename in filenames)
            //{
            //    var name = Path.GetFileName(filename);
            //    Console.WriteLine(name);
            //    switch (name)
            //    {
            //        case "ISampleService.cs":
            //            Console.WriteLine($"ソースからサービス名を取得:{name}\n{string.Join(",", GetServiceNames(filename))}\n");
            //            break;
            //        case "SampleService.cs":
            //            Console.WriteLine($"ソースからサービス名を取得:{name}\n{string.Join(",", GetServiceNames(filename))}\n");
            //            break;
            //        default:
            //            break;
            //    }
            //}
            #endregion

            #region コメントを除外してみる
            Console.WriteLine("--- コメント除外テスト ---");
            Console.WriteLine(RemoveComments(File.ReadAllText("./input/Sample.cs")));
            #endregion

            // それぞれのソースコードに対して構文木を生成する
            var syntaxTrees = GetSyntaxTrees(filenames);
            
            // ソース内のメソッドコメントを取得するテスト
            GetMethodComments(syntaxTrees);

            // 全ての構文木で組み立てて解析する
            // 第1引数は適当でよい
            var compilation = CSharpCompilation.Create("sample", syntaxTrees, references);

            Console.WriteLine("--- 各ソースコードの構文木について解析します ---");
            // 各ソースコードの構文木について解析
            foreach (var tree in syntaxTrees)
            {
                Console.WriteLine();
                Console.WriteLine($"--- {tree.FilePath}を解析します ---");
                // コンパイラからセマンティックモデルの取得
                var semanticModel = compilation.GetSemanticModel(tree);
                // 構文木からルートの子ノード群を取得
                var nodes = tree.GetRoot().DescendantNodes();

                // ノード群からクラスに関する構文情報群を取得
                // クラスはClassDeclarationSyntax
                // インタフェースはInterfaceDeclarationSyntax
                // 列挙型はEnumDeclarationSyntax
                // 構造体はStructDeclarationSyntax
                // デリゲートはDelegateDeclarationSyntax
                Console.WriteLine($"--- クラスの解析をします ---");
                var classSyntaxArray = nodes.OfType<ClassDeclarationSyntax>();
                foreach (var syntax in classSyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine($"アクセス修飾子: {symbol.DeclaredAccessibility} {symbol}");
                    Console.WriteLine($" クラス名（フル）: {symbol}");                 // StudyRoslyn.Sample
                    Console.WriteLine($" クラス名: {symbol.Name}");                 // Sample
                    Console.WriteLine($" 名前空間: {symbol.ContainingSymbol}");
                    Console.WriteLine($" Abstractか: {symbol.IsAbstract}");
                    Console.WriteLine($" Staticか: {symbol.IsStatic}");

                    // 継承しているクラスやインタフェースがあるかどうか
                    if (syntax.BaseList != null)
                    {
                        // 継承しているクラスなどのシンボルを取得
                        var inheritanceList = from baseSyntax in syntax.BaseList.Types
                                              let symbolInfo = semanticModel.GetSymbolInfo(baseSyntax.Type)
                                              let sym = symbolInfo.Symbol
                                              select sym;

                        // 継承しているクラスなどを出力
                        Console.WriteLine(" 継承:");
                        foreach (var inheritance in inheritanceList)
                            Console.WriteLine($"  {inheritance.ToDisplayString()}");
                    }
                }

                // メソッド
                Console.WriteLine("--- メソッドの解析をします ---");
                // コンストラクタはConstructorDeclarationSyntax
                var methodSyntaxArray = nodes.OfType<MethodDeclarationSyntax>();
                foreach (var syntax in methodSyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine("アクセス修飾子: {0}", symbol.DeclaredAccessibility);        // Public
                    Console.WriteLine(" メソッド名（フル）: {0}", symbol);                // Public StudyRoslyn.Sample.Method()
                    Console.WriteLine(" 名前空間: {0}", symbol.ContainingSymbol);                      // StudyRoslyn.Sample
                    Console.WriteLine(" Staticか: {0}", symbol.IsStatic);
                    Console.WriteLine(" 拡張メソッドか: {0}", symbol.IsExtensionMethod);

                    // 引数の型と名前をひとまとめに
                    var parameters = from param in symbol.Parameters select new { Name = param.Name, ParamType = param.ToString() };

                    // 引数の出力
                    Console.WriteLine(" 引数:");
                    foreach (var elem in parameters)
                        Console.WriteLine("  {0} {1}", elem.ParamType, elem.Name);                      //

                    // 戻り値の出力
                    Console.WriteLine(" 戻り値の型: {0}", symbol.ReturnType);                           // ReturnType: void
                }

                // プロパティ
                Console.WriteLine("--- プロパティの解析をします ---");
                var propertySyntaxArray = nodes.OfType<PropertyDeclarationSyntax>();
                // インタフェースとクラスの両方分出る。
                foreach (var syntax in propertySyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine("{0} {1}", symbol.DeclaredAccessibility, symbol);
                    Console.WriteLine(" Namespace: {0}", symbol.ContainingSymbol);
                    Console.WriteLine(" {0}: {1}", nameof(symbol.IsStatic), symbol.IsStatic);

                    // アクセサの取得
                    var accessors = syntax.AccessorList.Accessors.Select(
                        accessor => new
                        {
                            Name = accessor.Keyword.ToString(),
                            Access = accessor.Modifiers.Count > 0 ?
                            semanticModel.GetDeclaredSymbol(accessor).DeclaredAccessibility :
                            Accessibility.Public
                        });

                    // アクセサの出力
                    Console.WriteLine(" アクセサ:");
                    foreach (var accessor in accessors)
                        Console.WriteLine("  {0} {1}", accessor.Access, accessor.Name); //  Public get, Private set

                    // 戻り値に関するSymbolInfoを取得
                    var symbolInfo = semanticModel.GetSymbolInfo(syntax.Type);
                    // SymbolInfoからシンボルを取得
                    var sym = symbolInfo.Symbol;
                    // 戻り値の出力
                    Console.WriteLine(" 戻り値Type: {0}", sym.ToDisplayString());             // Type: int
                }

                // フィールド
                Console.WriteLine("--- フィールドの解析をします ---");
                var fieldSyntaxArray = nodes.OfType<FieldDeclarationSyntax>();
                foreach (var syntax in fieldSyntaxArray)
                {
                    // アクセス修飾子と名前を出力
                    Console.WriteLine("{0} {1}", syntax.Modifiers.First(), syntax.Declaration.Variables.ToFullString());
                    // 型に関するSymbolInfoを取得
                    var symbolInfo = semanticModel.GetSymbolInfo(syntax.Declaration.Type);
                    // 型からシンボルを取得
                    var sym = symbolInfo.Symbol;
                    // 型の出力
                    Console.WriteLine(" Type: {0}", sym.ToDisplayString());
                }
                Console.WriteLine($"--- --- {tree.FilePath}の解析終わり --- ---");
            }


        }

        #region 邪魔なのでどかす。
        /// <summary>
        /// それぞれのソースコードに対して構文木を生成する
        /// 
        /// .cs全てを使って解析するので、解析対象のソースは全部突っ込むこと。
        /// </summary>
        /// <param name="filenames">.csのファイル全て</param>
        /// <returns></returns>
        private static SyntaxTree[] GetSyntaxTrees(string[] filenames)
        {
            var syntaxTrees = filenames.Select(
                filename => CSharpSyntaxTree.ParseText(
                File.ReadAllText(filename), // ソースコードをテキストとして読み込む
                CSharpParseOptions.Default,
                filename)
            ).ToArray();
            return syntaxTrees;
        }
        // 解析用コンパイラで参照するdll
        // ぶっちゃけよく分かってない。おまじない。
        // 多分、コンパイルエラーが出たらtypeof(クラス名).Assembly.Locationみたいな感じでdll参照増やしていく感じだと思う。
        static readonly PortableExecutableReference[] references = new[]{
            // microlib.dll
            // intは内部的にはSystem.Int32を利用している。
            // メタリファレンスは何も指定しないとSystem.Int32等がインポートされていない。
            // コンパイルエラーを回避するため、objectクラスが属するアセンブリをメタリファレンスに指定しておく。
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // System.dll
            MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
            // System.Core.dll
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        /// <summary>
        /// ソース内のメソッドコメントを取得する
        /// </summary>
        /// <param name="syntaxTrees"></param>
        static void GetMethodComments(SyntaxTree[] syntaxTrees)
        {
            foreach (var syntaxTree in syntaxTrees)
            {
                // MethodDocumentCommentWalker.Visit()を呼び出せば、
                // MethodDocumentCommentWalker.DocumentCommentsに収集した結果が格納される
                var walker = new MethodDocumentCommentWalker(syntaxTree);
                walker.Visit(syntaxTree.GetRoot());

                foreach (var docComment in walker.DocumentComments)
                {
                    MethodDeclarationSyntax method = docComment.Key;
                    DocumentComment comment = docComment.Value;

                    // ここで取得したメソッドコメントを整形してファイルに出力したりする
                    Console.WriteLine("#" + method.Identifier);
                    Console.WriteLine("##Summary");
                    Console.WriteLine(comment.Summary);
                    Console.WriteLine("##parameters");
                    foreach (var param in comment.Params)
                    {
                        Console.WriteLine(param.Name + "\t" + param.Comment);
                    }
                    Console.WriteLine("##returns");
                    Console.WriteLine(comment.Returns);
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// ソースコードのサービスまたはインタフェースから、サービス名を取得
        /// 末尾にServiceが無いものは除外。
        /// インタフェースの場合は先頭の"I"を除外する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns>検出したサービス名全て</returns>
        public static IEnumerable<string> GetServiceNames(string path)
        {
            var result = new List<string>();

            // 1ファイルごとに解析しているので、別ファイルに定義したものは拾えない事に注意
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
            var compilation = CSharpCompilation.Create("sample", new SyntaxTree[] { syntaxTree }, references);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var nodes = syntaxTree.GetRoot().DescendantNodes();

            // ノード群からクラスに関する構文情報群を取得
            var classSyntaxArray = nodes.OfType<ClassDeclarationSyntax>();
            foreach (var syntax in classSyntaxArray)
            {
                var name = semanticModel.GetDeclaredSymbol(syntax).Name;
                if (name.EndsWith("Service"))
                {
                    result.Add($"{name}");
                }
            }

            // ノード群からインタフェースに関する構文情報群を取得
            var interfaceSyntaxArray = nodes.OfType<InterfaceDeclarationSyntax>();
            foreach (var syntax in interfaceSyntaxArray)
            {
                var name = semanticModel.GetDeclaredSymbol(syntax).Name;
                // "先頭のIは取る"
                name = name.StartsWith("I") ? name[1..] : name;
                if (name.EndsWith("Service"))
                {
                    result.Add($"{name}");
                }
            }

            return result.Distinct();   // 重複は除外
        }

        private static string RemoveComments(string text)
        {
            // コメントを除外したソースにする。
            // 文字列に"//"とか"/*"とか入ってるのは知らない…。

            // 優先順は"*/", "//", "/*"のようだ。
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(text, re, "$1");
        }

        // Roslynでコンストラクタやフィールドに何か追加したりできたらいいなあ
        private static void ReWriteSample()
        {
            // 対象のソースコードを読み込む
            var source = File.ReadAllText("./input/ITestService.cs");

            // シンタックス ツリーに変換する
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var rootNode = syntaxTree.GetRoot();             // ルートのノードを取得

            Console.WriteLine("-------------------------------- Node解析 --------------------------------");
            new NodeWalker().Visit(rootNode);

            Console.WriteLine("-------------------------------- Token解析 --------------------------------");
            new TokenWalker().Visit(rootNode);

            Console.WriteLine("-------------------------------- コード変更 --------------------------------");
            Console.WriteLine("----- ReplaceNodeによるコード変更 -----");
            // https://www.thinktecture.com/en/net/roslyn-source-generators-analyzers-code-fixes/
            // 上のNodeの中から、TypeがClassDeclarationSyntaxの物を指定することでクラス定義を見つける。
            var oldClass = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var newKlass = oldClass.WithIdentifier(SyntaxFactory.ParseToken("Aaaa"));               // クラス名を変更
            var rewriten = rootNode.ReplaceNode(oldClass, newKlass);
            Console.WriteLine(rewriten.NormalizeWhitespace()); // 整形して表示


            Console.WriteLine("----- SyntaxRewriterによるコード変更 -----");
            // http://blog.shos.info/archives/2013/12/csharp_roslynsyntaxrewriter.html
            // これは、SyntaxRewriterを継承して作成したクラスにメソッドをオーバーライドすることで
            // どのノードを変更するかを指定して、そのノードを変更する処理を記述する。
            // そしてノードのルートに対してVisitメソッドを呼び出すことで、ソースの修正ができる。

        }
        #endregion
    }
}
