using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.Labo
{
    // SyntaxWalkerはVisitorパターンという造りとなっており
    // ソースコードを解析するのに使う。
    /// <summary>
    /// ノード単位で辿る解析クラス
    /// </summary>
    public class NodeWalker : SyntaxWalker
    {
        /// <summary>
        /// 各ノードを辿る
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            if (node != null)
                Console.WriteLine($"[Node  - Type: {node.GetType().Name}, Kind: {node.RawKind}]\n{node}\n");

            base.Visit(node);
        }
    }

    /// <summary>
    /// トークン単位で辿る解析クラス
    /// 入れ子では解析せず、単に最初から1つずつのトークンに分解して列挙
    /// </summary>
    public class TokenWalker : SyntaxWalker // Visitor パターンでソースコードを解析
    {
        public TokenWalker() : base(depth: SyntaxWalkerDepth.Token) // トークンの深さまで Visit
        { }

        protected override void VisitToken(SyntaxToken token) // 各トークンを Visit
        {
            if (token != null)
                Console.WriteLine($"[Token - Type: {token.GetType().Name}, Kind: {token.RawKind}]\n{token}\n");

            base.VisitToken(token);
        }
    }
}
