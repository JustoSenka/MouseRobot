using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Robot
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns true if current method was called from that class or self.
        /// 
        /// Usage:
        ///    typeof(MyClass).IsTheCaller()
        ///    myClassInstance.IsTheCaller()
        ///    
        /// Cannot be used on static types.
        /// </summary>
        public static bool IsTheCaller<ExpectedCallerClass>(this ExpectedCallerClass expectedCaller) where ExpectedCallerClass : class
        {
            var checkerType = new StackFrame(1).GetMethod().DeclaringType;
            var actualCallerType = new StackFrame(2).GetMethod().DeclaringType;

            if (checkerType == actualCallerType)
                return true;

            if (expectedCaller is Type)
                return (expectedCaller as Type) == actualCallerType;
            
            return actualCallerType == typeof(ExpectedCallerClass);
        }

        public static void MoveAfter<T>(this IList<T> list, int indexToMove, int indexOfTarget)
        {
            var elem = list[indexToMove];
            list.RemoveAt(indexToMove);

            // If taken from beginning at putting to the edn, index will be smaller, since after removal they all get shifted
            indexOfTarget += (indexToMove <= indexOfTarget) ? 0 : 1;
            list.Insert(indexOfTarget, elem);
        }
        
        public static void MoveBefore<T>(this IList<T> list, int indexToMove, int indexOfTarget)
        {
            list.MoveAfter(indexToMove, indexOfTarget - 1);
        }

        public static T ClearMethods<T>(this T commands) where T : ICollection<Command>
        {
            foreach(var c in commands)
                c.ClearMethod();
            return commands;
        }

    }
}
