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
                CommentSegment MyComment = SegmentBuilder.GetRelativeSegment(MyIndex, -1, CommentObservingTypes.ToArray()) as CommentSegment;
                // Missing comment
                if (MyComment == null)
                {
                    Errors.Add(String.Format("Missing required comment for {0}", ToString()));
                    bSuccess = false;
                }
            }
            return bSuccess;
        }
    }
}
