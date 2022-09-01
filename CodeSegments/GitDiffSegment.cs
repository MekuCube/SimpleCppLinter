using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class GitDiffSegment : SegmentBase
    {
        public string DiffSegmentInner = null;

        public GitDiffSegment(int InStartIndex, int InEndIndex, string InDiffSegmentInner) : base(InStartIndex, InEndIndex)
        {
            Exclusive = 1;
            DiffSegmentInner = InDiffSegmentInner;
        }

        public static GitDiffSegment Build(string InCode, int InStartIndex = 0)
        {
            string StartString = "@@";
            int StartIndex = InCode.IndexOf(StartString, InStartIndex);
            if (StartIndex == -1)
                return null;

            int MidIndex = InCode.IndexOf(StartString, StartIndex + StartString.Length);
            if (MidIndex == -1)
                return null;

            string EndString = ":";
            int EndIndex = InCode.IndexOf(EndString, MidIndex + StartString.Length);
            if (EndIndex == -1)
                return null;
            EndIndex += EndString.Length;

            string Inner = InCode.Substring(StartIndex, EndIndex - StartIndex);
            GitDiffSegment Segment = new GitDiffSegment(StartIndex, EndIndex, Inner);
            return Segment;
        }
        public override string ToString()
        {
            if (DiffSegmentInner == null)
                return base.ToString();
            return String.Format("{0}", DiffSegmentInner);
        }
    }
}
