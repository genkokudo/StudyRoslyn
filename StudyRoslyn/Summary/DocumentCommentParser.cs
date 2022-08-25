using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace StudyRoslyn.Summary
{
    /// <summary>
    /// メソッドコメントのXMLをパースするクラス
    /// </summary>
    public class DocumentCommentParser
    {
        public static DocumentComment Parse(string documentComment)
        {
            if (string.IsNullOrWhiteSpace(documentComment))
            {
                // コメントなし
                return null;
            }
            // XDocumentクラスを用いてsammary要素とparam要素を取得する
            XDocument doc = XDocument.Parse(documentComment);
            var summary = doc.Descendants("summary").FirstOrDefault();
            var returns = doc.Descendants("returns").FirstOrDefault();
            var paramList = doc.Descendants("param");

            var docComment = new DocumentComment
            {
                // summary要素が取得できた場合、不要な改行や空白を取り除く
                Summary = (summary != null) ? RemoveAnySpaceChar(summary.Value) : string.Empty,
                Returns = (returns != null) ? RemoveAnySpaceChar(returns.Value) : string.Empty
            };

            foreach (var param in paramList)
            {
                // param要素はname属性が取得できたもののみ有効な要素とする
                var name = param.Attribute("name");
                if (name != null)
                {
                    var pdc = new ParamDocumentComment();
                    pdc.Name = RemoveAnySpaceChar(name.Value);
                    pdc.Comment = RemoveAnySpaceChar(param.Value);
                    docComment.Params.Add(pdc);
                }
            }
            return docComment;
        }

        // 引数の文字列から正規表現のパターン「\s」に該当する文字を削除した文字列を返す
        private static string RemoveAnySpaceChar(string str)
        {
            return Regex.Replace(str, "\\s", "");

        }
    }
}
