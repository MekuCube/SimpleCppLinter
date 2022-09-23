using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtensionMethods
    {
        static public int IndexOfAny(this string InString, string[] anyOf, int StartIndex = 0)
        {
            int Index = -1;

            foreach (string value in anyOf)
            {
                if (value == null || value.Length == 0)
                    continue;
                int IndexIt = InString.IndexOf(value, StartIndex);
                if (IndexIt < 0)
                    continue;
                if (Index >= 0 && IndexIt >= Index)
                    continue;
                Index = IndexIt;
            }
            return Index;
        }
        public static string GetNestedString(this string InCode, string OpenNest = "(", string CloseNest = ")", int StartIndex = 0)
        {
            int OutEndIndex = -1;
            return InCode.GetNestedString(out OutEndIndex, OpenNest, CloseNest, StartIndex);
        }

        public static string GetNestedString(this string InCode, out int OutEndIndex, string OpenNest = "(", string CloseNest = ")", int StartIndex = 0)
        {
            int OutStartIndex = -1;
            return InCode.GetNestedString(out OutStartIndex, out OutEndIndex, OpenNest, CloseNest, StartIndex);
        }

        public static string GetNestedString(this string InCode, out int OutStartIndex, out int OutEndIndex, string OpenNest = "(", string CloseNest = ")", int StartIndex = 0)
        {
            OutEndIndex = -1;
            OutStartIndex = InCode.IndexOf(OpenNest, StartIndex);
            if (OutStartIndex == -1)
                return null;

            int NumOpen = 1;
            int CurrentIndex = OutStartIndex + OpenNest.Length;
            int EndIndex = -1;
            while (true)
            {
                EndIndex = InCode.IndexOfAny(new string[] { OpenNest, CloseNest }, CurrentIndex);
                if (EndIndex == -1)
                    break;

                if (InCode.Substring(EndIndex, CloseNest.Length) == CloseNest)
                {
                    EndIndex += CloseNest.Length;
                    NumOpen--;
                    if (NumOpen <= 0)
                        break;
                }
                else if (InCode.Substring(EndIndex, OpenNest.Length) == OpenNest)
                {
                    EndIndex += OpenNest.Length;
                    NumOpen++;
                }
                else
                {
                    throw new Exception("Invalid character");
                }

                CurrentIndex = EndIndex;
            }
            if (EndIndex == -1)
                return null;

            OutEndIndex = EndIndex;
            string InnerSegment = InCode.Substring(OutStartIndex, EndIndex - OutStartIndex);

            return InnerSegment;
        }

        public static string GetLineByIndex(this string InString, int InIndex)
        {
            int OutLineNumber;
            return InString.GetLineByIndex(InIndex, out OutLineNumber);
        }
        public static string GetLineByIndex(this string InString, int InIndex, out int OutLineNumber)
        {
            string[] Lines = InString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToArray();
            int CurrentIndex = 0;
            for (OutLineNumber = 0; OutLineNumber < Lines.Length; OutLineNumber++)
            {
                string Line = Lines[OutLineNumber];
                if (OutLineNumber > 0)
                    CurrentIndex += Environment.NewLine.Length;
                CurrentIndex += Line.Length;

                if (InIndex < CurrentIndex)
                    return Line;
            }
            throw new Exception("Failed to find index");
        }
        public static int Count(this string InString, string SubString)
        {
            return Regex.Matches(InString, SubString).Count;
        }
    }
}