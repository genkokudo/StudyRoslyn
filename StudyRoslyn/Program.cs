using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StudyRoslyn
{
    public class Program
    {
        static void Main(string[] args)
        {
            // 読み込むソースファイル群
            string[] filenames = { "../../../Sample.cs" };
            // 構文木を格納するリスト
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var filename in filenames)
            {
                // ソースコードをテキストとして読み込む
                var code = File.ReadAllText(filename);
                // 構文木の生成
                var syntaxTree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default, filename);
                // 構文木をリストへ格納
                syntaxTrees.Add(syntaxTree);
            }

            // LINQ
            // var syntaxTrees = (from filename in filenames
            //            let code = File.ReadAllText(filename)
            //            let syntaxTree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default, filename)
            //            select syntaxTree).ToArray();

            var references = new[]{
                // microlib.dll
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                // System.dll
                MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
                // System.Core.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                // External library
                // MetadataReference.CreateFromFile("library path"),
            };

            // 解析用コンパイラの生成
            var compilation = CSharpCompilation.Create("sample", syntaxTrees, references);

            foreach (var tree in syntaxTrees)
            {
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
                var classSyntaxArray = nodes.OfType<ClassDeclarationSyntax>();
                foreach (var syntax in classSyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine("{0} {1}", symbol.DeclaredAccessibility, symbol);
                    Console.WriteLine(" Namespace: {0}", symbol.ContainingSymbol);
                    Console.WriteLine(" {0}: {1}", nameof(symbol.IsAbstract), symbol.IsAbstract);
                    Console.WriteLine(" {0}: {1}", nameof(symbol.IsStatic), symbol.IsStatic);

                    // 継承しているクラスやインタフェースがあるかどうか
                    if (syntax.BaseList != null)
                    {
                        // 継承しているクラスなどのシンボルを取得
                        var inheritanceList = from baseSyntax in syntax.BaseList.Types
                                              let symbolInfo = semanticModel.GetSymbolInfo(baseSyntax.Type)
                                              let sym = symbolInfo.Symbol
                                              select sym;

                        // 継承しているクラスなどを出力
                        Console.WriteLine(" Inheritance:");
                        foreach (var inheritance in inheritanceList)
                            Console.WriteLine("  {0}", inheritance.ToDisplayString());
                    }
                }

                // コンストラクタはConstructorDeclarationSyntax
                var methodSyntaxArray = nodes.OfType<MethodDeclarationSyntax>();
                foreach (var syntax in methodSyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine("{0} {1}", symbol.DeclaredAccessibility, symbol);
                    Console.WriteLine(" Namespace: {0}", symbol.ContainingSymbol);
                    Console.WriteLine(" {0}: {1}", nameof(symbol.IsStatic), symbol.IsStatic);
                    Console.WriteLine(" IsExtensionMethod: {0}", symbol.IsExtensionMethod);

                    // 引数の型と名前をひとまとめに
                    var parameters = from param in symbol.Parameters select new { Name = param.Name, ParamType = param.ToString() };

                    // 引数の出力
                    Console.WriteLine(" Parameters:");
                    foreach (var elem in parameters)
                        Console.WriteLine("  {0} {1}", elem.ParamType, elem.Name);

                    // 戻り値の出力
                    Console.WriteLine(" ReturnType: {0}", symbol.ReturnType);
                }

                var propertySyntaxArray = nodes.OfType<PropertyDeclarationSyntax>();
                foreach (var syntax in propertySyntaxArray)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    Console.WriteLine("{0} {1}", symbol.DeclaredAccessibility, symbol);
                    Console.WriteLine(" Namespace: {0}", symbol.ContainingSymbol);
                    Console.WriteLine(" {0}: {1}", nameof(symbol.IsStatic), symbol.IsStatic);

                    // アクセサの取得
                    var accessors = from accessor in syntax.AccessorList.Accessors
                                    select new
                                    {
                                        Name = accessor.Keyword.ToString(),
                                        Access = accessor.Modifiers.Count > 0 ?
                                            semanticModel.GetDeclaredSymbol(accessor).DeclaredAccessibility :
                                            Accessibility.Public
                                    };

                    // クエリ式使わない場合
                    //var accessors = new List<(string Name, Accessibility Access)>();
                    //foreach (var accessor in syntax.AccessorList.Accessors)
                    //{
                    //    var accessibility = Accessibility.Public;
                    //    var keyword = accessor.Keyword.ToString();
                    //    if (accessor.Modifiers.Count > 0)
                    //    {
                    //        var msym = semanticModel.GetDeclaredSymbol(accessor);
                    //        accessibility = msym.DeclaredAccessibility;
                    //    }
                    //    accessors.Add((keyword, accessibility));
                    //}

                    // アクセサの出力
                    Console.WriteLine(" Accessors:");
                    foreach (var accessor in accessors)
                        Console.WriteLine("  {0} {1}", accessor.Access, accessor.Name);

                    // 戻り値に関するSymbolInfoを取得
                    var symbolInfo = semanticModel.GetSymbolInfo(syntax.Type);
                    // SymbolInfoからシンボルを取得
                    var sym = symbolInfo.Symbol;
                    // 戻り値の出力
                    Console.WriteLine(" Type: {0}", sym.ToDisplayString());
                }

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

            }
        }
    }
}
