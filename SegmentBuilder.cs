using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SimpleCppLinter
{
    public class SegmentBuilder
    {
        List<SegmentBase> AllSegments = new List<SegmentBase>();
        bool bIgnoreGitDiffRemoved = true;

        // If parsing Git diff text, indicate whether this segment was added, removed or neither
        public enum EGitDiffState
        {
            None,
            Added,
            Removed
        };

        public SegmentBuilder(string CppText)
        {
            List<Type> SegmentTypes = SegmentAttribute.GetAllTypes();
            foreach (Type TypeIt in SegmentTypes)
            {
                int CurrentIndex = 0;
                while (true)
                {
                    object[] Parameters = new object[] { CppText, CurrentIndex };
                    MethodInfo BuildMethod = TypeIt.GetMethod("Build");
                    SegmentBase Segment = BuildMethod.Invoke(null, Parameters) as SegmentBase;
                    if (Segment == null)
                        break;
                    if (Segment.GetEndIndex() < 0)
                        throw new Exception("Invalid EndIndex");
                    AllSegments.Add(Segment);
                    CurrentIndex = Segment.GetStartIndex() + 1;
                }
            }

            // Sort by StartIndex
            AllSegments = AllSegments.OrderBy(o => o.GetStartIndex()).ToList();

            // Delete removed lines
            if (bIgnoreGitDiffRemoved)
            {
                for (int i = AllSegments.Count - 1; i >= 0; i--)
                {
                    if (AllSegments[i].GitDiffState != SegmentBuilder.EGitDiffState.Removed)
                        continue;
                    AllSegments.RemoveAt(i);
                }
            }

            // Git Diff Segment are exclusive, and any segments that overlap within are invalid
            for (int i = AllSegments.Count-1; i >= 0; i--)
            {
                if (!IsInHigherExclusivitySegment(AllSegments[i]))
                    continue;

                AllSegments.RemoveAt(i);
            }

            // Calculate start lines
            int CurrentLine = -1;
            int PreviousStartIndex = -1;
            for (int i = 0; i < AllSegments.Count; i++)
            {
                SegmentBase Segment = AllSegments[i];
                int SegmentStartLine = Segment.GetStartLine();
                int SegmentStartIndex = Segment.GetStartIndexForLine();
                if (SegmentStartLine >= 0)
                {
                    CurrentLine = SegmentStartLine;
                    PreviousStartIndex = SegmentStartIndex;
                    continue;
                }
                string TextBetweenPreviousAndCurrentSegment = null;
                if (CurrentLine < 0)
                {
                    // First line is 1
                    CurrentLine = 1;
                    TextBetweenPreviousAndCurrentSegment = CppText.Substring(0, SegmentStartIndex);
                }
                else
                {
                    TextBetweenPreviousAndCurrentSegment = CppText.Substring(PreviousStartIndex, SegmentStartIndex - PreviousStartIndex);
                }
                SegmentStartLine = TextBetweenPreviousAndCurrentSegment.Count(Environment.NewLine);

                CurrentLine += SegmentStartLine;
                Segment.SetStartLine(CurrentLine);
                PreviousStartIndex = SegmentStartIndex;
            }
        }

        // Returns true if this segment has another segment within that is higher exclusivity
        internal bool IsInHigherExclusivitySegment(SegmentBase InSegment)
        {
            // Git Diff Segment are exclusive, and any segments that overlap within are invalid
            for (int i = 0; i < AllSegments.Count; i++)
            {
                if (InSegment == AllSegments[i])
                    continue;
                SegmentBase OtherSegment = AllSegments[i];
                if (OtherSegment == null)
                    continue;
                if (OtherSegment.Exclusive <= InSegment.Exclusive)
                    continue;

                if (OtherSegment.GetStartIndex() < InSegment.GetStartIndex())
                    continue;
                if (OtherSegment.GetEndIndex() > InSegment.GetEndIndex())
                    continue;
                return true;
            }
            return false;
        }

        internal SegmentBase GetRelativeSegment(int CurrentIndex, int LookOffset, params Type[] TypesToReturn)
        {
            int IndexIt = CurrentIndex + LookOffset;
            while (IndexIt >= 0 && IndexIt < AllSegments.Count)
            {
                SegmentBase Segment = AllSegments[IndexIt];
                IndexIt += LookOffset;
                if (Segment == null)
                    continue;
                if (!Segment.GetType().IsSubclassOf(TypesToReturn))
                    continue;
                return Segment;
            }
            return null;
        }

        public List<SegmentBase> Validate(List<string> Errors, List<string> Warnings, EGitDiffState OnlyDiffState = EGitDiffState.None)
        {
            List<SegmentBase> SegmentsWithErrors = new List<SegmentBase>();

            for (int i = 0; i < AllSegments.Count; i++)
            {
                SegmentBase SegmentIt = AllSegments[i];
                if (SegmentIt == null)
                    throw new Exception("Got null SegmentBase");
                if (OnlyDiffState != EGitDiffState.None && SegmentIt.GitDiffState != OnlyDiffState)
                    continue;

                if (!SegmentIt.OnValidate(this, i, Errors, Warnings))
                    SegmentsWithErrors.Add(SegmentIt);
            }

            return SegmentsWithErrors;
        }
    }
}
