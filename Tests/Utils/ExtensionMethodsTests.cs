using System;
using Robot;
using NUnit.Framework;
using RobotRuntime;
using System.Runtime.CompilerServices;

namespace Tests.Utils
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void IsTheCaller_WithinTheSameClass_ReturnsTrue()
        {
            var result = CallMethodFromSameClass.Call(typeof(AnyClass));
            Assert.IsTrue(result);
        }

        [Test]
        public void IsTheCaller_WithinTheSameClass_ReturnsTrue_EvenWhenCheckingOnSelf()
        {
            var result = CallMethodFromSameClass.Call(typeof(CallMethodFromSameClass));
            Assert.IsTrue(result);
        }

        public class AnyClass { }
        public class CallMethodFromSameClass
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Call(Type callerType)
            {
                return Method(callerType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Method(Type callerType)
            {
                return callerType.IsTheCaller();
            }
        }

        [Test]
        public void IsTheCaller_WithinDifferentClasses_ReturnsTrue_WhenCorrectClassIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(CallMethodFromOtherClass));
            Assert.IsTrue(result);
        }

        [Test]
        public void IsTheCaller_WithinDifferentClasses_ReturnsFalse_WhenSelfClassIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(CallMethodFromSameClass));
            Assert.IsFalse(result);
        }

        [Test]
        public void IsTheCaller_WithinDifferentClasses_ReturnsFalse_IfClassToFarInStackIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(ExtensionMethodsTests));
            Assert.IsFalse(result);
        }

        public class CallMethodFromOtherClass
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Call(Type callerType)
            {
                return CallMethodFromSameClass.Method(callerType);
            }
        }
    }
}
