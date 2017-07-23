using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

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

            // If taken from beginning at putting to the end, index will be smaller, since after removal they all get shifted
            indexOfTarget += (indexToMove <= indexOfTarget) ? 0 : 1;
            list.Insert(indexOfTarget, elem);
        }
        
        public static void MoveBefore<T>(this IList<T> list, int indexToMove, int indexOfTarget)
        {
            list.MoveAfter(indexToMove, indexOfTarget - 1);
        }

        public static int IndexOf<T, K>(this T collection, K element) where T : IReadOnlyCollection<K> where K : class
        {
            var index = collection.TakeWhile((s) => s != element).Count();
            if (collection.ElementAt(index) != element)
                throw new Exception("Index not found for: " + element.ToString());
            return index;
        }

        public static void ForEach<T, K>(this T source, Action<K> action) where T : IEnumerable where K : class
        {
            foreach (var item in source)
                action(item as K);
        }

        public static T ClearMethods<T>(this T commands) where T : ICollection<Command>
        {
            foreach(var c in commands)
                c.ClearMethod();
            return commands;
        }

    }
}
