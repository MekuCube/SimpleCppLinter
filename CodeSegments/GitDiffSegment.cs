using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver]
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
            // @@ -132,7 +132,7 @@ public:

            string StartString = "@@";
            string EndString = ":";
            int StartIndex = -1;
            int EndIndex = -1;
            while (InStartIndex < InCode.Length)
            {
                StartIndex = InCode.IndexOf(StartString, InStartIndex);
                if (StartIndex == -1)
                {
                    // Failed to find start string
                    break;
                }

                int MidIndex = InCode.IndexOf(StartString, StartIndex + StartString.Length);
                if (MidIndex == -1)
                {
                    InStartIndex += StartString.Length;
                    continue;
                }

                MidIndex += StartString.Length;

                // @@ -132,7 +132,7 @@
                string StartSection = InCode.Substring(StartIndex, MidIndex - StartIndex).Trim();
                if (StartSection.Contains(Environment.NewLine))
                {
                    InStartIndex += StartString.Length;
                    continue;
                }

                EndIndex = InCode.IndexOf(EndString, MidIndex + StartString.Length);
                if (EndIndex == -1)
                {
                    InStartIndex += StartString.Length;
                    continue;
                }

                EndIndex += EndString.Length;
                string EndSection = InCode.Substring(MidIndex, EndIndex - MidIndex).Trim();
                // public:
                if (EndSection.Contains(Environment.NewLine))
                {
                    InStartIndex += StartString.Length;
                    EndIndex = -1;
                    continue;
                }
                break;
            }
            if (EndIndex == -1)
            {
                return null;
            }

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
