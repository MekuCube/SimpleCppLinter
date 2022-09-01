using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class FunctionSegment : SegmentBase
    {
        public string FunctionInner = null;
        public MacroSegment MacroSegment = null;

        public FunctionSegment(MacroSegment InMacroSegment, int InStartIndex, int InEndIndex, string InFunctionInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            FunctionInner = InFunctionInner;
            GitDiffState = InMacroSegment.GitDiffState;
        }

        public override int GetStartIndex()
        {
            if (MacroSegment != null)
                return MacroSegment.GetStartIndex();
            return base.GetStartIndex();
        }

        public static FunctionSegment Build(string InCode, int InStartIndex = 0)
        {
            MacroSegment MacroSegment = MacroSegment.Build(InCode, InStartIndex, "UFUNCTION");
            if (MacroSegment == null)
                return null;

            string[] FunctionEndStrings = { ";", "{" };
            int EndIndex = InCode.IndexOfAny(FunctionEndStrings, MacroSegment.GetEndIndex());
            if (EndIndex == -1)
                return null;

            string FunctionInner = InCode.Substring(MacroSegment.GetEndIndex(), EndIndex - MacroSegment.GetEndIndex()).Trim();
            FunctionSegment Segment = new FunctionSegment(MacroSegment, MacroSegment.GetEndIndex(), EndIndex, FunctionInner);

            if (FunctionInner.StartsWith("+"))
            {
                Segment.FunctionInner = Segment.FunctionInner.Substring(1).Trim();
                Segment.GitDiffState = SegmentBuilder.EGitDiffState.Added;
            }
            else if (FunctionInner.StartsWith("-"))
            {
                Segment.FunctionInner = Segment.FunctionInner.Substring(1).Trim();
                Segment.GitDiffState = SegmentBuilder.EGitDiffState.Removed;
            }

            return Segment;
        }

        // Report any warnings or errors
        public override bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings)
        {
            CommentSegment MyComment = SegmentBuilder.GetRelativeSegment(MyIndex, -1, typeof(CommentSegment), typeof(PropertySegment), typeof(FunctionSegment), typeof(GitDiffSegment)) as CommentSegment;
            // Missing comment
            if (MyComment == null)
            {
                Errors.Add(String.Format("Missing comment for function '{0}'", FunctionInner));
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            if (FunctionInner == null)
                return base.ToString();
            return String.Format("{0} [{1}]", FunctionInner, base.ToString());
        }
    }
}
