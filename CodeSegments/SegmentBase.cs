using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    public abstract class SegmentBase
    {
        // Start index in CppText
        public int StartIndex = -1;
        // End index in CppText
        public int EndIndex = -1;
        // If set, will delete any segment its within that has lesser exclusivity
        public int Exclusive = 0;

        // If parsing Git diff text, indicate whether this segment was added, removed or neither
        public SegmentBuilder.EGitDiffState GitDiffState = SegmentBuilder.EGitDiffState.None;

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
        public virtual bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings) { return true; }
    }

    public class SegmentAttribute : Attribute
    {
        public SegmentAttribute()
        {

        }
    }
}
