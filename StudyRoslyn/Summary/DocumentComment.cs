using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.Summary
{
    /// <summary>
    /// XMLドキュメントコメントを格納するクラス
    /// </summary>
    public class DocumentComment
    {
        public string Summary { get; set; }
        public List<ParamDocumentComment> Params { get; set; }
        public string Returns { get; set; }

        public DocumentComment()
        {
            Params = new List<ParamDocumentComment>();
        }
    }

    public class ParamDocumentComment
    {
        public string Name { get; set; }
        public string Comment { get; set; }
    }
}
