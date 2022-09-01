using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class CommentSegment : SegmentBase
    {
        public string CommentInner = null;

        public CommentSegment()
        {
        }

        public static CommentSegment Build(string InCode, int InStartIndex = 0)
        {
            CommentSegment SingleLineComment = BuildSingleLine(InCode, InStartIndex);
            CommentSegment MultiLineComment = BuildMultiLine(InCode, InStartIndex);

            if (SingleLineComment != null && MultiLineComment != null)
            {
                if (SingleLineComment.StartIndex < MultiLineComment.StartIndex)
                    return SingleLineComment;
                return MultiLineComment;
            }
            else if (SingleLineComment != null)
                return SingleLineComment;
            else
                return MultiLineComment;
        }
        public static CommentSegment BuildSingleLine(string InCode, int InStartIndex = 0)
        {
            string StartString = "//";
            int StartIndex = InCode.IndexOf(StartString, InStartIndex);
            if (StartIndex == -1)
                return null;

            int EndIndex = InCode.IndexOf(Environment.NewLine, StartIndex);
            if (EndIndex == -1)
            {
                return null;
            }

            CommentSegment Segment = new CommentSegment();
            Segment.StartIndex = StartIndex;
            Segment.EndIndex = EndIndex;
            Segment.CommentInner = InCode.Substring(StartIndex, EndIndex - StartIndex);

            return Segment;
        }
        public static CommentSegment BuildMultiLine(string InCode, int InStartIndex = 0)
        {
            string StartString = "/*";
            int StartIndex = InCode.IndexOf(StartString, InStartIndex);
            if (StartIndex == -1)
                return null;

            int EndIndex;
            string CommentInner = InCode.GetNestedString(out EndIndex, "/*", "*/", StartIndex);
            if (CommentInner == null)
                return null;

            CommentSegment Segment = new CommentSegment();
            Segment.StartIndex = StartIndex;
            Segment.EndIndex = EndIndex;
            Segment.CommentInner = CommentInner;

            return Segment;
        }
        public override string ToString()
        {
            if (CommentInner == null)
                return base.ToString();
            return String.Format("{0}", CommentInner);
        }
    }
}
