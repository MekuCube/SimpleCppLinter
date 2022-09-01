using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class FunctionSegment : MacroSegment
    {
        public string FunctionInner = null;

        public FunctionSegment(MacroSegment InMacroSegment, int InEndIndex, string InFunctionInner) : base(InMacroSegment.StartIndex, InEndIndex, InMacroSegment.MacroName, InMacroSegment.MacroInner)
        {
            FunctionInner = InFunctionInner;
            GitDiffState = InMacroSegment.GitDiffState;
        }
        public FunctionSegment(int InStartIndex, int InEndIndex, string InMacroName, string InMacroInner, string InFunctionInner) : base(InStartIndex, InEndIndex, InMacroName, InMacroInner)
        {
            FunctionInner = InFunctionInner;
        }

        public static FunctionSegment Build(string InCode, int InStartIndex = 0)
        {
            var MacroSegment = Build(InCode, InStartIndex, "UFUNCTION");
            if (MacroSegment == null)
                return null;

            string PropertyEndString = ";";
            int EndIndex = InCode.IndexOf(PropertyEndString, MacroSegment.EndIndex);
            if (EndIndex == -1)
                return null;
            EndIndex += PropertyEndString.Length;

            string FunctionInner = InCode.Substring(MacroSegment.EndIndex, EndIndex - MacroSegment.EndIndex).Trim();
            FunctionSegment Segment = new FunctionSegment(MacroSegment, EndIndex, FunctionInner);

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
