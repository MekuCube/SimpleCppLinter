using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver(true)]
    internal class FunctionSegment : SegmentBase
    {
        public string FunctionInner = null;
        public string FunctionReturnType = null;
        public string FunctionName = null;
        public string FunctionParameters = null;
        public MacroSegment MacroSegment = null;

        // TODO: Expose as config
        private static readonly string[] RequireUnderscore = { "Multicast" };

        public FunctionSegment(MacroSegment InMacroSegment, int InStartIndex, int InEndIndex, string InFunctionInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            FunctionInner = InFunctionInner;
            GitDiffState = InMacroSegment.GitDiffState;

            // TODO: Generalize
            if (GitDiffState == SegmentBuilder.EGitDiffState.None)
            {
                if (FunctionInner.StartsWith("+"))
                {
                    FunctionInner = FunctionInner.Substring(1).Trim();
                    GitDiffState = SegmentBuilder.EGitDiffState.Added;
                }
                else if (FunctionInner.StartsWith("-"))
                {
                    FunctionInner = FunctionInner.Substring(1).Trim();
                    GitDiffState = SegmentBuilder.EGitDiffState.Removed;
                }
            }

            // Parse FunctionInner
            int CurrentIndex = 0;

            // Return type
            FunctionReturnType = FunctionInner.Split(' ')[0];
            CurrentIndex += FunctionReturnType.Length;
            
            // Parameters
            int StartIndex;
            int EndIndex;
            FunctionParameters = FunctionInner.GetNestedString(out StartIndex, out EndIndex, "(", ")", CurrentIndex);
            
            // Function name
            CurrentIndex = EndIndex;
            FunctionName = FunctionInner.Substring(FunctionReturnType.Length, StartIndex - FunctionReturnType.Length).Trim();
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
            return new FunctionSegment(MacroSegment, MacroSegment.GetEndIndex(), EndIndex, FunctionInner);
        }

        // Report any warnings or errors
        public override bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings)
        {
            bool bSuccess = true;
            // Verify specific naming for functions
            if (FunctionName != null)
            {
                // Underscore required after certain start tags
                foreach (string RequireUnderscoreIt in RequireUnderscore)
                {
                    if (!FunctionName.StartsWith(RequireUnderscoreIt, StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (FunctionName.StartsWith(RequireUnderscoreIt + "_", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    Errors.Add(String.Format("{0} requires underscore (_) after '{1}'", ToString(), RequireUnderscoreIt));
                    bSuccess = false;
                }
            }
            if (!base.OnValidate(SegmentBuilder, MyIndex, Errors, Warnings))
                bSuccess = false;
            return bSuccess;
        }

        public override string ToString()
        {
            if (MacroSegment != null && FunctionName != null)
                return String.Format("{0} '{1}'", MacroSegment.MacroName, FunctionName);
            else if (MacroSegment != null && FunctionInner != null)
                return String.Format("{0} '{1}'", MacroSegment.MacroName, FunctionInner);
            else
                return base.ToString();
        }
    }
}
