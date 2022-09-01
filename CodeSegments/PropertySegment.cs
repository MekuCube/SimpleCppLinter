using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [SegmentAttribute]
    internal class PropertySegment : SegmentBase
    {
        public string PropertyInner = null;
        public MacroSegment MacroSegment = null;

        public PropertySegment(int InStartIndex, int InEndIndex, MacroSegment InMacroSegment, string InPropertyInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            PropertyInner = InPropertyInner;
            GitDiffState = InMacroSegment.GitDiffState;
        }

        public override int GetStartIndex()
        {
            if (MacroSegment != null)
                return MacroSegment.GetStartIndex();
            return base.GetStartIndex();
        }

        public static PropertySegment Build(string InCode, int InStartIndex = 0)
        {
            MacroSegment MacroSegment = MacroSegment.Build(InCode, InStartIndex, "UPROPERTY");
            if (MacroSegment == null)
                return null;

            string PropertyEndString = ";";
            int EndIndex = InCode.IndexOf(PropertyEndString, MacroSegment.GetEndIndex());
            if (EndIndex == -1)
                return null;

            string PropertyInner = InCode.Substring(MacroSegment.GetEndIndex(), EndIndex - MacroSegment.GetEndIndex()).Trim();

            PropertySegment Segment = new PropertySegment(MacroSegment.GetEndIndex(), EndIndex, MacroSegment, PropertyInner);

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
