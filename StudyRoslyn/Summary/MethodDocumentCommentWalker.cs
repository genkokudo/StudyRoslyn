using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.Summary
{
    /// <summary>
    /// CSharpSyntaxWalkerを継承したメソッドコメント収集クラス
    /// </summary>
    public class MethodDocumentCommentWalker : CSharpSyntaxWalker
    {
        private SemanticModel m_semanticModel; // ドキュメントコメントを取得するにはセマンティックモデルが必要
        private Dictionary<MethodDeclarationSyntax, DocumentComment> m_documentComments = new Dictionary<MethodDeclarationSyntax, DocumentComment>();

        // 取得したドキュメントコメントのリストを返すプロパティ
        public IReadOnlyDictionary<MethodDeclarationSyntax, DocumentComment> DocumentComments
        {
            get { return m_documentComments; }
        }

        public MethodDocumentCommentWalker(SyntaxTree tree)
        {
            // コンストラクタでセマンティックモデルを作成しておく
            var compilation = CSharpCompilation.Create("tmpcompilation", syntaxTrees: new[] { tree });
            m_semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees[0], true);
        }

        // VisitMethodDeclaration()をオーバーライドし、各メソッド宣言からドキュメントコメントを取得する
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            base.VisitMethodDeclaration(node);

            // IMethodSymbol.GetDocumentationCommentXml()でメソッドコメントのXML文字列を取得し、
            // パーサークラスを用いてパースする
            IMethodSymbol symbol = m_semanticModel.GetDeclaredSymbol(node);
            var parse = DocumentCommentParser.Parse(symbol.GetDocumentationCommentXml());
            if (parse == null)
            {
                return;
            }
            m_documentComments[node] = parse;
        }
    }
}
