using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace StudyRoslyn
{
    public class Program
    {

        static void Main(string[] args)
        {
            // inputフォルダのファイルを全て読み込む
            string[] filenames = Directory.GetFiles("./input", "*.cs", SearchOption.AllDirectories);

            // それぞれのソースコードに対して構文木を生成する
            var syntaxTrees = filenames.Select(
                filename => CSharpSyntaxTree.ParseText(
                File.ReadAllText(filename), // ソースコードをテキストとして読み込む
                CSharpParseOptions.Default,
                filename)
            ).ToArray();

            // 解析用コンパイラで参照するdll
            // ぶっちゃけよく分かってない。おまじない。
            // 多分、コンパイルエラーが出たらtypeof(クラス名).Assembly.Locationみたいな感じでdll参照増やしていく感じだと思う。
            var references = new[]{
                // microlib.dll
                // intは内部的にはSystem.Int32を利用している。
                // メタリファレンスは何も指定しないとSystem.Int32等がインポートされていない。
                // コンパイルエラーを回避するため、objectクラスが属するアセンブリをメタリファレンスに指定しておく。
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                // System.dll
                MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
                // System.Core.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                // External library
                // MetadataReference.CreateFromFile("library path"),
            };

            // それぞれの構文木と参照dllから解析用コンパイラを生成する
            // 第1引数は適当でよい？
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
                        accessor => new {
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

            Console.WriteLine($"--- --- ★★★★各フィールドと値を取得する実験★★★★ --- ---");
            var testSample = new Sample();
            Console.WriteLine(AssertMaker.MakeAssert(testSample, nameof(testSample)));


        }
    }
}
