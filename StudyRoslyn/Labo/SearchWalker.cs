using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StudyRoslyn.Labo
{
    /// <summary>
    /// ノード単位で辿り、特定のメソッドを探すクラス
    /// </summary>
    public class SearchWalker : SyntaxWalker
    {
        // 変更対象のクラス名
        private string _className;

        // 変更対象のメソッド名
        private string _methodName;

        /// <summary>
        /// 変更対象のノード
        /// </summary>
        public SyntaxNode TargetNode { get; set; }

        /// <summary>
        /// 各ノードを辿る
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            // メソッドやコンストラクタのノードの時、親ノードがクラスなので
            // そのクラス名が今回探す対象のものかをチェックする。
            bool checkParentClass(SyntaxNode nd)
            {
                var cls = nd.Parent as ClassDeclarationSyntax;
                return cls != null && cls.Identifier.Text == _className;
            }

            if (string.IsNullOrWhiteSpace(_className) || string.IsNullOrWhiteSpace(_methodName) || node == null)
                return;

            if (_className == _methodName)
            {
                // コンストラクタから探す
                var constructor = node as ConstructorDeclarationSyntax;
                if (constructor != null && constructor.Identifier.Text == _className)
                {
                    if (checkParentClass(node))
                    {
                        // 対象のノードを特定した
                        TargetNode = node;
                    }
                }
            }
            else
            {
                // メソッドから探す
                var method = node as MethodDeclarationSyntax;
                if (method != null && method.Identifier.Text == _methodName)
                {
                    if (checkParentClass(node))
                    {
                        // 対象のノードを特定した
                        TargetNode = node;
                    }
                }


            }

            base.Visit(node);
        }

        /// <summary>
        /// 指定されたメソッドブロックを探す
        /// </summary>
        /// <param name="node"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        public SyntaxNode SearchMethod(SyntaxNode node, string className, string methodName)
        {
            _className = className;
            _methodName = methodName;
            Visit(node);

            return TargetNode;
        }
    }

}
