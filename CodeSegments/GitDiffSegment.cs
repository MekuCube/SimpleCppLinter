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
        public int NewStartLine = 0;

        private static readonly string StartString = "@@";
        private static readonly string EndString = ":";

        public GitDiffSegment(int InStartIndex, int InEndIndex, string InDiffSegmentInner) : base(InStartIndex, InEndIndex)
        {
            Exclusive = 1;
            DiffSegmentInner = InDiffSegmentInner;

            string LineCodes = DiffSegmentInner.GetNestedString(StartString, StartString);
            if (LineCodes != null)
            {
                LineCodes = LineCodes.Trim('@').Trim();
                string[] SplitLineCodes = LineCodes.Split(' ');
                if (SplitLineCodes != null && SplitLineCodes.Length > 1)
                {
                    string PrevCode = SplitLineCodes[0];
                    string NewCode = SplitLineCodes[1];

                    string[] NewCodeSegments = NewCode.Split(',');
                    // If NewStartLine is the line below, then our line has to be NewStartLine-1
                    if (int.TryParse(NewCodeSegments[0], out NewStartLine))
                        StartLine = NewStartLine - 1;
                }
            }
        }

        public static GitDiffSegment Build(string InCode, int InStartIndex = 0)
        {
            // @@ -132,7 +132,7 @@ public:

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
