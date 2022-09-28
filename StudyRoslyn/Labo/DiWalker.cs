using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StudyRoslyn.Labo
{
    /// <summary>
    /// DIを行っているクラスとメソッドを探すためのWalker
    /// </summary>
    public class DiWalker : SyntaxWalker
    {
        /// <summary>
        /// DIを行っているクラス名
        /// </summary>
        public string DiClassName { get; set; } = null;

        /// <summary>
        /// DIを行っているメソッド名
        /// コンストラクタで行っている場合はクラス名と同じになる
        /// </summary>
        public string DiMethodName { get; set; } = null;

        /// <summary>
        /// 各ノードを辿り、以下の要素を探してメソッド名とクラス名を記憶する
        /// ・ServiceCollectionまたはIServiceCollectionをパラメータに持つメソッド
        /// ・Ioc.Default.ConfigureServicesを呼んでいるメソッド
        /// 
        /// 今のところこれで十分なので、他の条件があれば必要に応じて随時追加
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            void constructor(SyntaxNode node)
            {
                var constructor = node as ConstructorDeclarationSyntax;
                FindServiceCollectionParameter(constructor.ParameterList.Parameters, constructor.Parent as ClassDeclarationSyntax, constructor.Identifier.Text);
            }
            void method(SyntaxNode node)
            {
                var method = node as MethodDeclarationSyntax;
                FindServiceCollectionParameter(method.ParameterList.Parameters, method.Parent as ClassDeclarationSyntax, method.Identifier.Text);
            }

            // ServiceCollectionまたはIServiceCollectionをパラメータに持つメソッド（コンストラクタ）を探す
            if (node != null)
            {
                if (node.GetType().Name == nameof(ConstructorDeclarationSyntax))
                {
                    // コンストラクタの場合
                    constructor(node);
                }
                else if (node.GetType().Name == nameof(MethodDeclarationSyntax))
                {
                    // メソッド定義の場合
                    method(node);
                }
                else if (node.GetType().Name == nameof(MemberAccessExpressionSyntax))
                {
                    // Ioc.Default.ConfigureServicesを探す
                    var member = node as MemberAccessExpressionSyntax;
                    if (node.ToString().Trim() == "Ioc.Default.ConfigureServices")
                    {
                        SyntaxNode parent = member.Parent;
                        while (!(parent is MethodDeclarationSyntax) && !(parent is ConstructorDeclarationSyntax) || parent is null)
                        {
                            parent = parent.Parent;
                        }
                        if (parent is null)
                        {
                            // あり得ないはず
                        }
                        else if (parent is ConstructorDeclarationSyntax)
                        {
                            // コンストラクタの場合
                            var con = parent as ConstructorDeclarationSyntax;
                            DiMethodName = DiClassName = con.Identifier.Text;
                        }
                        else if (parent is MethodDeclarationSyntax)
                        {
                            // メソッド定義の場合
                            var mtd = parent as MethodDeclarationSyntax;
                            var cls = mtd.Parent as ClassDeclarationSyntax;
                            DiClassName = cls.Identifier.Text;
                            DiMethodName = mtd.Identifier.Text;
                        }
                    }
                }
            }

            base.Visit(node);
        }

        /// <summary>
        /// 引数にIServiceCollectionがあるかを確認する。
        /// あればそのメソッドをDI登録メソッドとして記憶する。
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="classSyntax"></param>
        /// <param name="IdentifierText"></param>
        private void FindServiceCollectionParameter(SeparatedSyntaxList<ParameterSyntax> Parameters, ClassDeclarationSyntax classSyntax, string IdentifierText)
        {
            foreach (var parameter in Parameters)
            {
                // Type名を取得：クラスかインタフェースかは分からない
                // ソース解析だと名前しか分からないので、継承とかやっている場合は諦める。
                var typeName = parameter.Type.GetText().ToString().Trim();

                if (typeName == nameof(ServiceCollection) || typeName == nameof(IServiceCollection))
                {
                    DiClassName = classSyntax.Identifier.Text;
                    DiMethodName = IdentifierText;
                }
            }
        }

        /// <summary>
        /// DIを行なっているクラスとメソッドを見つける
        /// なかったらnullを返す
        /// </summary>
        /// <param name="node"></param>
        /// <returns>クラス名, メソッド名</returns>
        public (string, string) FindDiClass(SyntaxNode node)
        {
            Visit(node);
            return (DiClassName, DiMethodName);
        }
    }
}
