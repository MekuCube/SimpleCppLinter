using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    internal class MacroSegment : SegmentBase
    {
        public string MacroName = null;
        public string MacroInner = null;

        public MacroSegment(int InStartIndex, int InEndIndex, string InMacroName, string InMacroInner) : base(InStartIndex, InEndIndex)
        {
            MacroName = InMacroName;
            MacroInner = InMacroInner;
        }
        public static MacroSegment Build(string InCode, int InStartIndex = 0, string InMacroName = "UPROPERTY")
        {
            string StartString = string.Format("{0}", InMacroName);
            int StartIndex = InCode.IndexOf(StartString, InStartIndex);
            if (StartIndex == -1)
                return null;

            int EndIndex = -1;
            string MacroInner = InCode.GetNestedString(out EndIndex, "(", ")", StartIndex);

            MacroSegment Segment = new MacroSegment(StartIndex, EndIndex, InMacroName, MacroInner);

            string FullLine = InCode.GetLineByIndex(StartIndex);
            if (FullLine.StartsWith("+"))
                Segment.GitDiffState = SegmentBuilder.EGitDiffState.Added;
            else if (FullLine.StartsWith("-"))
                Segment.GitDiffState = SegmentBuilder.EGitDiffState.Removed;

            return Segment;
        }
        public override string ToString()
        {
            if (MacroName == null || MacroInner == null)
                return base.ToString();
            return String.Format("{0}{1}", MacroName, MacroInner);
        }
    }
}
