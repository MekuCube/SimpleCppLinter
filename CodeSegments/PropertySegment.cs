using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver(true)]
    internal class PropertySegment : SegmentBase
    {
        public string PropertyInner = null;
        public string PropertyType = null;
        public string PropertyName = null;
        public MacroSegment MacroSegment = null;

        // TODO: Expose as config
        private static Dictionary<string, string> TypeRequiresPrefix = new Dictionary<string, string>
        {
            { "bool", "b" }
        };

        public PropertySegment(int InStartIndex, int InEndIndex, MacroSegment InMacroSegment, string InPropertyInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            PropertyInner = InPropertyInner;
            GitDiffState = InMacroSegment.GitDiffState;

            // TODO: Generalize
            if (GitDiffState == SegmentBuilder.EGitDiffState.None)
            {
                if (PropertyInner.StartsWith("+"))
                {
                    PropertyInner = PropertyInner.Substring(1).Trim();
                    GitDiffState = SegmentBuilder.EGitDiffState.Added;
                }
                else if (PropertyInner.StartsWith("-"))
                {
                    PropertyInner = PropertyInner.Substring(1).Trim();
                    GitDiffState = SegmentBuilder.EGitDiffState.Removed;
                }
            }

            // Parse PropertyInner
            int CurrentIndex = 0;

            // Variable type
            PropertyType = PropertyInner.Split(' ')[0];
            CurrentIndex += PropertyType.Length;

            PropertyName = PropertyInner.Split(' ')[1];
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
            return new PropertySegment(MacroSegment.GetEndIndex(), EndIndex, MacroSegment, PropertyInner);
        }

        // Report any warnings or errors
        public override bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings)
        {
            bool bSuccess = true;
            // Verify specific naming for properties
            if (PropertyType != null && PropertyName != null && TypeRequiresPrefix.ContainsKey(PropertyType))
            {
                string RequiredPrefix = TypeRequiresPrefix[PropertyType];
                if (!PropertyName.StartsWith(RequiredPrefix))
                {
                    Errors.Add(String.Format("{0} of type '{1}' is required to start with '{2}' ({3})", ToString(), PropertyType, RequiredPrefix, RequiredPrefix + PropertyName));
                    bSuccess = false;
                }
            }
            if (!base.OnValidate(SegmentBuilder, MyIndex, Errors, Warnings))
                bSuccess = false;
            return bSuccess;
        }

        public override string ToString()
        {
            if (PropertyName != null && MacroSegment != null)
                return String.Format("{0} '{1}'", MacroSegment.MacroName, PropertyName);
            else if (PropertyInner != null && MacroSegment != null)
                return String.Format("{0} '{1}'", MacroSegment.MacroName, PropertyInner);
            else
                return base.ToString();
        }
    }
}
