using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return GetNestedString(InCode, out OutEndIndex, OpenNest, CloseNest, StartIndex);
        }

        public static string GetNestedString(this string InCode, out int OutEndIndex, string OpenNest = "(", string CloseNest = ")", int StartIndex = 0)
        {
            OutEndIndex = -1;
            int OpenIndex = InCode.IndexOf(OpenNest, StartIndex);
            if (OpenIndex == -1)
                return null;

            int NumOpen = 1;
            int CurrentIndex = OpenIndex + OpenNest.Length;
            int EndIndex = -1;
            while (true)
            {
                EndIndex = InCode.IndexOfAny(new string[] { OpenNest, CloseNest }, CurrentIndex);
                if (EndIndex == -1)
                    break;

                if (InCode.Substring(EndIndex, OpenNest.Length) == OpenNest)
                {
                    EndIndex++;
                    NumOpen++;
                }
                else if (InCode.Substring(EndIndex, CloseNest.Length) == CloseNest)
                {
                    EndIndex++;
                    NumOpen--;
                    if (NumOpen <= 0)
                        break;
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
            string InnerSegment = InCode.Substring(OpenIndex, EndIndex - OpenIndex);

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
    }
}