using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SimpleCppLinter
{
    public abstract class SegmentBase
    {
        // Start index in CppText
        protected int StartIndex = -1;
        // End index in CppText
        protected int EndIndex = -1;
        // If set, will delete any segment its within that has lesser exclusivity
        public int Exclusive = 0;
        // Start line in CppText
        protected int StartLine = -1;

        // If parsing Git diff text, indicate whether this segment was added, removed or neither
        public SegmentBuilder.EGitDiffState GitDiffState = SegmentBuilder.EGitDiffState.None;

        virtual public int GetStartIndex()
        {
            return StartIndex;
        }
        virtual public int GetEndIndex()
        {
            return EndIndex;
        }
        virtual public int GetStartIndexForLine()
        {
            return StartIndex;
        }
        virtual public int GetStartLine()
        {
            return StartLine;
        }
        public void SetStartLine(int NewStartLine)
        {
            StartLine = NewStartLine;
        }

        public int Length()
        {
            return EndIndex - StartIndex;
        }

        public SegmentBase()
        {
        }
        public SegmentBase(int InStartIndex, int InEndIndex)
        {
            StartIndex = InStartIndex;
            EndIndex = InEndIndex;
        }
        // Report any warnings or errors
        public virtual bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings)
        {
            // Verify comment if required
            CommentObserverAttribute CommentAttribute = GetType().GetCustomAttribute<CommentObserverAttribute>(true);
            bool bSuccess = true;
            if (CommentAttribute != null && CommentAttribute.bRequiresComment)
            {
                // Look for the code segment above us, and verify that it is a comment
                List<Type> CommentObservingTypes = CommentObserverAttribute.GetAllTypes();
                CommentObservingTypes.Add(typeof(CommentSegment));
                SegmentBase PreviousSegment = SegmentBuilder.GetRelativeSegment(MyIndex, -1, CommentObservingTypes.ToArray());
                CommentSegment MyComment = PreviousSegment as CommentSegment;
                GitDiffSegment PreviousGitDiff = PreviousSegment as GitDiffSegment;
                // Missing comment (and not cut off by diff)
                if (MyComment == null && PreviousGitDiff == null)
                {
                    Errors.Add(String.Format("Missing required comment for {0} [line {1}]", ToString(), GetStartLine()));
                    bSuccess = false;
                }
            }
            return bSuccess;
        }
    }
}
