using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SimpleCppLinter
{
    // This class can be evaluated as a code segment
    internal class SegmentAttribute : Attribute
    {
        internal SegmentAttribute()
        {
        }
        // Returns all SegmentBase types with SegmentAttribute
        static internal List<Type> GetAllTypes()
        {
            var type = typeof(SegmentBase);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            List<Type> Result = new List<Type>();

            foreach (var mytype in types)
            {
                if (mytype.IsAbstract) continue;
                if (mytype.GetCustomAttribute<SegmentAttribute>(true) == null) continue;
                Result.Add(mytype);
            }
            return Result;
        }
    }

    // This class can be described with a comment above it
    internal class CommentObserverAttribute : Attribute
    {
        internal bool bRequiresComment = false;

        internal CommentObserverAttribute(bool bInRequiresComment = false)
        {
            bRequiresComment = bInRequiresComment;
        }
        // Returns all SegmentBase types with CommentObserverAttribute
        static internal List<Type> GetAllTypes()
        {
            Type BaseType = typeof(SegmentBase);
            IEnumerable<Type> AllTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => BaseType.IsAssignableFrom(p));

            List<Type> Result = new List<Type>();

            foreach (Type TypeIt in AllTypes)
            {
                if (TypeIt.IsAbstract) continue;
                if (TypeIt.GetCustomAttribute<CommentObserverAttribute>(true) == null) continue;
                Result.Add(TypeIt);
            }
            return Result;
        }
    }
}
