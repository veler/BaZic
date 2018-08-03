using System;
using System.Text;
using System.Threading.Tasks;
using BaZic.Core.ComponentModel.Reflection;
using BaZic.Core.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Core.Tests.ComponentModel.Reflection
{
    [TestClass]
    public class FastReflectionTest
    {
        [TestMethod]
        public void FastReflectionProperties()
        {
            var fastReflection = new FastReflection();
            var testValue = new MockValue();

            Assert.AreEqual(1, fastReflection.GetProperty(testValue, nameof(MockValue.Foo)));
            Assert.AreEqual(2, fastReflection.GetProperty(testValue, nameof(MockValue.Bar)));



            try
            {
                fastReflection.GetProperty(testValue, nameof(MockValue.Boo));
                Assert.Fail();
            }
            catch (MemberAccessException)
            {
            }
            catch
            {
                Assert.Fail();
            }



            try
            {
                fastReflection.GetProperty(testValue, "Buzz");
                Assert.Fail();
            }
            catch (MemberAccessException)
            {
            }
            catch
            {
                Assert.Fail();
            }


            testValue = new MockValue();
            fastReflection.SetProperty(testValue, nameof(MockValue.Foo), 123);
            Assert.AreEqual(123, fastReflection.GetProperty(testValue, nameof(MockValue.Foo)));



            try
            {
                fastReflection.SetProperty(testValue, nameof(MockValue.Bar), 123);
                Assert.Fail();
            }
            catch (MemberAccessException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void FastReflectionPropertiesStatic()
        {
            var fastReflection = new FastReflection();

            fastReflection.SetStaticProperty(typeof(MockValue), nameof(MockValue.StaticFoo), 123);
            Assert.AreEqual(123, fastReflection.GetStaticProperty(typeof(MockValue), nameof(MockValue.StaticFoo)));
        }

        [TestMethod]
        public void FastReflectionInstantiate()
        {
            var fastReflection = new FastReflection();

            var instance1 = fastReflection.Instantiate(typeof(StringBuilder));
            Assert.IsInstanceOfType(instance1, typeof(StringBuilder));

            var instance3 = (string)fastReflection.Instantiate(typeof(string));
            Assert.IsInstanceOfType(instance3, typeof(string));

            var instance4 = (bool)fastReflection.Instantiate(typeof(bool));
            Assert.IsInstanceOfType(instance4, typeof(bool));

            var instance2 = (MockValue)fastReflection.Instantiate(typeof(MockValue));
            Assert.AreEqual(1, instance2.Foo);

            instance2 = (MockValue)fastReflection.Instantiate(typeof(MockValue), 123);
            instance2 = (MockValue)fastReflection.Instantiate(typeof(MockValue), 456);
            instance2 = (MockValue)fastReflection.Instantiate(typeof(MockValue), 789);
            Assert.AreEqual(789, instance2.Foo);

            instance2 = (MockValue)fastReflection.Instantiate(typeof(MockValue), "100");
            Assert.AreEqual(1000, instance2.Foo);

            try
            {
                fastReflection.Instantiate(typeof(MockValue), 1, 2, 3);
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            try
            {
                fastReflection.Instantiate(typeof(MockValue), true);
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void FastReflectionMethods()
        {
            var fastReflection = new FastReflection();
            var testValue = new MockValue();

            Assert.IsNull(fastReflection.InvokeMethod(testValue, nameof(MockValue.VoidMethod)));
            fastReflection.InvokeMethod(testValue, nameof(MockValue.VoidMethod)); // Must call from the cache

            Assert.AreEqual(246, fastReflection.InvokeMethod(testValue, nameof(MockValue.ReturnMethod), 123));



            try
            {
                fastReflection.InvokeMethod(testValue, nameof(MockValue.ReturnMethod), "Bad argument");
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }



            try
            {
                fastReflection.InvokeMethod(testValue, nameof(MockValue.ReturnMethod), "Bad argument", "Second bad argument");
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void FastReflectionMethodsStatic()
        {
            var fastReflection = new FastReflection();

            var instance = "123";
            var result = fastReflection.InvokeStaticMethod(typeof(int), nameof(int.Parse), instance);

            Assert.AreEqual(123, result);

            try
            {
                fastReflection.InvokeStaticMethod(typeof(MockValue), nameof(MockValue.VoidMethod));
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }


        private int eventRaiseCount = 0;
        [TestMethod]
        public async Task FastReflectionEvents()
        {
            var fastReflection = new FastReflection();
            var testValue = new MockValue();

            var action = new Action(() =>
            {
                eventRaiseCount++;
            });

            fastReflection.SubscribeEvent(testValue, nameof(MockValue.MockEventChanged), action);

            await Task.Delay(200);
            testValue.RaiseEvent();
            await Task.Delay(200);

            fastReflection.Dispose();

            await Task.Delay(200);
            testValue.RaiseEvent();
            await Task.Delay(200);

            Assert.AreEqual(1, eventRaiseCount);
        }
    }
}
