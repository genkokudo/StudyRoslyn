using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.Labo
{
    // SyntaxWalkerはVisitorパターンという造りとなっており
    // ソースコードを解析するのに使う。
    /// <summary>
    /// 
    /// </summary>
    class Walker : SyntaxWalker
    {
        /// <summary>
        /// 各ノードを辿る
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode node)
        {
            if (node != null)
                Console.WriteLine("[Node  - Type: {0}, Kind: {1}]\n{2}\n", node.GetType().Name, node.RawKind, node);

            base.Visit(node);
        }
    }
}
