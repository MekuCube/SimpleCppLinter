using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver(true)]
    internal class StructSegment : SegmentBase
    {
        public string StructInner = null;
        public MacroSegment MacroSegment = null;

        public StructSegment(int InStartIndex, int InEndIndex, MacroSegment InMacroSegment, string InStructInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            StructInner = InStructInner;
            GitDiffState = InMacroSegment.GitDiffState;

            // TODO: Generalize
            if (StructInner.StartsWith("+"))
            {
                StructInner = StructInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Added;
            }
            else if (StructInner.StartsWith("-"))
            {
                StructInner = StructInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Removed;
            }
            StructInner = StructInner.TrimEnd('+').Trim();
        }

        public override int GetStartIndex()
        {
            if (MacroSegment != null)
                return MacroSegment.GetStartIndex();
            return base.GetStartIndex();
        }

        public static StructSegment Build(string InCode, int InStartIndex = 0)
        {
            MacroSegment MacroSegment = MacroSegment.Build(InCode, InStartIndex, "USTRUCT");
            if (MacroSegment == null)
                return null;

            string StructEndString = "{";
            int EndIndex = InCode.IndexOf(StructEndString, MacroSegment.GetEndIndex());
            if (EndIndex == -1)
                return null;

            string StructInner = InCode.Substring(MacroSegment.GetEndIndex(), EndIndex - MacroSegment.GetEndIndex()).Trim();

            StructSegment Segment = new StructSegment(MacroSegment.GetEndIndex(), EndIndex, MacroSegment, StructInner);

            return Segment;
        }

        public override string ToString()
        {
            if (StructInner == null || MacroSegment == null)
                return base.ToString();
            return String.Format("{0} '{1}'", MacroSegment.MacroName, StructInner);
        }
    }
}
