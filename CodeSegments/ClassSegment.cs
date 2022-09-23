using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver(true)]
    internal class ClassSegment : SegmentBase
    {
        public string ClassInner = null;
        public MacroSegment MacroSegment = null;

        public ClassSegment(int InStartIndex, int InEndIndex, MacroSegment InMacroSegment, string InClassInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            ClassInner = InClassInner;
            GitDiffState = InMacroSegment.GitDiffState;

            // TODO: Generalize
            if (ClassInner.StartsWith("+"))
            {
                ClassInner = ClassInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Added;
            }
            else if (ClassInner.StartsWith("-"))
            {
                ClassInner = ClassInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Removed;
            }
            ClassInner = ClassInner.TrimEnd('+').Trim();
        }

        public override int GetStartIndex()
        {
            if (MacroSegment != null)
                return MacroSegment.GetStartIndex();
            return base.GetStartIndex();
        }

        public static ClassSegment Build(string InCode, int InStartIndex = 0)
        {
            MacroSegment MacroSegment = MacroSegment.Build(InCode, InStartIndex, "UCLASS");
            if (MacroSegment == null)
                return null;

            string ClassEndString = "{";
            int EndIndex = InCode.IndexOf(ClassEndString, MacroSegment.GetEndIndex());
            if (EndIndex == -1)
                return null;

            string ClassInner = InCode.Substring(MacroSegment.GetEndIndex(), EndIndex - MacroSegment.GetEndIndex()).Trim();

            ClassSegment Segment = new ClassSegment(MacroSegment.GetEndIndex(), EndIndex, MacroSegment, ClassInner);

            return Segment;
        }

        public override string ToString()
        {
            if (ClassInner == null || MacroSegment == null)
                return base.ToString();
            return String.Format("{0} '{1}'", MacroSegment.MacroName, ClassInner);
        }
    }
}
