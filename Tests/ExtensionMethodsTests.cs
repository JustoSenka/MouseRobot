using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;

namespace Tests
{
    [TestClass]
    public class ExtensionMethodsTests
    {
        [TestMethod]
        public void IsTheCaller_WithinTheSameClass_ReturnsTrue()
        {
            var result = CallMethodFromSameClass.Call(typeof(AnyClass));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsTheCaller_WithinTheSameClass_ReturnsTrue_EvenWhenCheckingOnSelf()
        {
            var result = CallMethodFromSameClass.Call(typeof(CallMethodFromSameClass));
            Assert.IsTrue(result);
        }

        public class AnyClass { }
        public class CallMethodFromSameClass
        {
            public static bool Call(Type callerType)
            {
                return Method(callerType);
            }

            public static bool Method(Type callerType)
            {
                return callerType.IsTheCaller();
            }
        }

        [TestMethod]
        public void IsTheCaller_WithinDifferentClasses_ReturnsTrue_WhenCorrectClassIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(CallMethodFromOtherClass));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsTheCaller_WithinDifferentClasses_ReturnsFalse_WhenSelfClassIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(CallMethodFromSameClass));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsTheCaller_WithinDifferentClasses_ReturnsFalse_IfClassToFarInStackIsProvided()
        {
            var result = CallMethodFromOtherClass.Call(typeof(ExtensionMethodsTests));
            Assert.IsFalse(result);
        }

        public class CallMethodFromOtherClass
        {
            public static bool Call(Type callerType)
            {
                return CallMethodFromSameClass.Method(callerType);
            }
        }
    }
}
