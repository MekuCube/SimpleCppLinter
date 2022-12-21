using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCppLinter
{
    [Segment]
    [CommentObserver(true)]
    internal class PropertySegment : SegmentBase
    {
        public string PropertyInner = null;
        public string PropertyType = null;
        public string PropertyName = null;
        public string PropertyDefaultValue = null;
        public int PropertySize = 0;
        public MacroSegment MacroSegment = null;

        // TODO: Expose as config
        private static Dictionary<string, string> TypeRequiresPrefix = new Dictionary<string, string>
        {
            { "bool", "b" }
            , { "uint8:1", "b" }
        };

        // TODO: Expose as config
        private static Dictionary<string, string> DeprecatedTypes = new Dictionary<string, string>
        {
            { "bool", "uint8" }
        };

        public PropertySegment(int InStartIndex, int InEndIndex, MacroSegment InMacroSegment, string InPropertyInner) : base(InStartIndex, InEndIndex)
        {
            MacroSegment = InMacroSegment;
            PropertyInner = InPropertyInner;
            GitDiffState = InMacroSegment.GitDiffState;

            // TODO: Generalize
            if (PropertyInner.StartsWith("+"))
            {
                PropertyInner = PropertyInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Added;
            }
            else if (PropertyInner.StartsWith("-"))
            {
                PropertyInner = PropertyInner.Substring(1).Trim();
                if (GitDiffState == SegmentBuilder.EGitDiffState.None)
                    GitDiffState = SegmentBuilder.EGitDiffState.Removed;
            }

            string PropertyParseString = PropertyInner;

            // Property default value
            int PropertyDefaultValueSpecifierIndex = PropertyParseString.LastIndexOf('=');
            if (PropertyDefaultValueSpecifierIndex >= 0)
            {
                PropertyDefaultValue = PropertyParseString.Substring(PropertyDefaultValueSpecifierIndex + 1).Trim();
                if (PropertyDefaultValue != null && PropertyDefaultValue.Length > 0)
                {
                    PropertyParseString = PropertyParseString.Substring(0, PropertyDefaultValueSpecifierIndex).Trim();
                }
                else
                {
                    PropertyDefaultValue = null;
                }
            }

            // Property size
            int PropertySizeSpecifierIndex = PropertyParseString.LastIndexOf(':');
            if (PropertySizeSpecifierIndex >= 0)
            {
                string PropertySizeString = PropertyParseString.Substring(PropertySizeSpecifierIndex + 1).Trim();
                if (int.TryParse(PropertySizeString, out PropertySize))
                {
                    PropertyParseString = PropertyParseString.Substring(0, PropertySizeSpecifierIndex).Trim();
                }
            }

            // Parse PropertyInner by whitespaces
            List<string> SplitPropertyInner = PropertyParseString.Split(' ').ToList();

            // Variable name
            if (SplitPropertyInner.Count > 0)
            {
                int Index = SplitPropertyInner.Count - 1;
                PropertyName = SplitPropertyInner[Index];
                SplitPropertyInner.RemoveAt(Index);
            }

            // Variable type
            if (SplitPropertyInner.Count > 0)
            {
                PropertyType = String.Join(" ", SplitPropertyInner);
            }
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

            int PropertyEndStartIndex = MacroSegment.GetEndIndex();
            int EndIndex = 0;
            string PropertyInner = null;
            while (PropertyEndStartIndex < InCode.Length)
            {
                string PropertyEndString = ";";
                EndIndex = InCode.IndexOf(PropertyEndString, PropertyEndStartIndex);
                if (EndIndex == -1)
                    return null;

                PropertyInner = InCode.Substring(PropertyEndStartIndex, EndIndex - PropertyEndStartIndex);

                // Trim
                int PrevLength = PropertyInner.Length;
                PropertyInner = PropertyInner.TrimStart();
                int AddedLength = -(PropertyInner.Length - PrevLength);
                PropertyEndStartIndex += AddedLength;
                PropertyInner.Trim();

                if (PropertyInner.Length <= 0)
                {
                    PropertyEndStartIndex++;
                    PropertyInner = null;
                    continue;
                }
                break;
            }
            if (PropertyInner == null)
                return null;
            return new PropertySegment(PropertyEndStartIndex, EndIndex, MacroSegment, PropertyInner);
        }

        // Report any warnings or errors
        public override bool OnValidate(SegmentBuilder SegmentBuilder, int MyIndex, List<string> Errors, List<string> Warnings)
        {
            bool bSuccess = true;
            // Verify specific naming for properties
            if (PropertyType != null && PropertyName != null)
            {
                string Key = PropertyType;
                if (PropertySize > 0)
                {
                    string NewKey = Key + ":" + PropertySize.ToString();
                    if (TypeRequiresPrefix.ContainsKey(NewKey))
                        Key = NewKey;
                }

                if (TypeRequiresPrefix.ContainsKey(Key))
                    {
                        string RequiredPrefix = TypeRequiresPrefix[Key];
                        if (!PropertyName.StartsWith(RequiredPrefix))
                        {
                            Errors.Add(String.Format("{0} of type '{1}' is required to start with '{2}' ({3}) [line {4}]", ToString(), Key, RequiredPrefix, RequiredPrefix + PropertyName, GetStartLine()));
                            bSuccess = false;
                        }
                    }
            }
            // Verify deprecated types aren't used
            if (PropertyType != null && PropertyName != null && DeprecatedTypes.ContainsKey(PropertyType))
            {
                string RequiredType = DeprecatedTypes[PropertyType];

                Errors.Add(String.Format("{0} is of type '{1}', please use type '{2}' instead ({2} {3}) [line {4}]", ToString(), PropertyType, RequiredType, PropertyName, GetStartLine()));
                bSuccess = false;

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
