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
        private string _className;
        private string _methodName;

        /// <summary>
        /// 各ノードを辿る
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            if (string.IsNullOrWhiteSpace(_className) || string.IsNullOrWhiteSpace(_methodName) || node == null)
                return;

            if (_className == _methodName)
            {
                if (node.GetType().Name == nameof(ConstructorDeclarationSyntax))
                {
                    Console.WriteLine($"[Node  - Type: {node.GetType().Name}, Kind: {node.RawKind}]\n{node}\n");
                }
            }
            else
            {
                if (node.GetType().Name == nameof(MethodDeclarationSyntax))
                {
                    Console.WriteLine($"[Node  - Type: {node.GetType().Name}, Kind: {node.RawKind}]\n{node}\n");
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
        public void SearchMethod(SyntaxNode node, string className, string methodName)
        {
            _className = className;
            _methodName = methodName;
            Visit(node);
        }
    }

}
