using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class TypeExtensionMethods
    {
        static public bool IsSubclassOf(this Type InType, params Type[] Types)
        {
            foreach (Type TypeIt in Types)
            {
                if (InType == TypeIt || InType.IsSubclassOf(TypeIt))
                    return true;
            }
            return false;
        }
    }
}