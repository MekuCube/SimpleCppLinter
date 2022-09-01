using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class PropertySegment : MacroSegment
    {
        public string PropertyInner = null;

        public PropertySegment(MacroSegment InMacroSegment, int InEndIndex, string InPropertyInner) : base(InMacroSegment.StartIndex, InEndIndex, InMacroSegment.MacroName, InMacroSegment.MacroInner)
        {
            PropertyInner = InPropertyInner;
            GitDiffState = InMacroSegment.GitDiffState;
        }
        public PropertySegment(int InStartIndex, int InEndIndex, string InMacroName, string InMacroInner, string InPropertyInner) : base(InStartIndex, InEndIndex, InMacroName, InMacroInner)
        {
            PropertyInner = InPropertyInner;
        }

        public static PropertySegment Build(string InCode, int InStartIndex = 0)
        {
            var MacroSegment = Build(InCode, InStartIndex, "UPROPERTY");
            if (MacroSegment == null)
                return null;

            string PropertyEndString = ";";
            int EndIndex = InCode.IndexOf(PropertyEndString, MacroSegment.EndIndex);
            if (EndIndex == -1)
                return null;
            EndIndex += PropertyEndString.Length;

            string PropertyInner = InCode.Substring(MacroSegment.EndIndex, EndIndex - MacroSegment.EndIndex).Trim();

            PropertySegment Segment = new PropertySegment(MacroSegment, EndIndex, PropertyInner);

            if (PropertyInner.StartsWith("+"))
            {
                Segment.PropertyInner = Segment.PropertyInner.Substring(1).Trim();
                Segment.GitDiffState = SegmentBuilder.EGitDiffState.Added;
            }
            else if (PropertyInner.StartsWith("-"))
            {
                Segment.PropertyInner = Segment.PropertyInner.Substring(1).Trim();
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
                Errors.Add(String.Format("Missing comment for property '{0}'", PropertyInner));
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            if (PropertyInner == null)
                return base.ToString();
            return String.Format("{0} [{1}]", PropertyInner, base.ToString());
        }
    }
}
