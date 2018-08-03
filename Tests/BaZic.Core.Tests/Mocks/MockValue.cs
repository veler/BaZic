namespace BaZic.Core.Tests.Mocks
{
    public class MockValue : MockValueBase
    {
        public static int StaticFoo { get; set; }

        public MockValue()
        {
        }

        public MockValue(int foo)
        {
            Foo = foo;
        }

        public MockValue(string foo)
        {
            Foo = int.Parse(foo) * 10;
        }

        public void VoidMethod()
        {
        }

        public int ReturnMethod(int value)
        {
            return value * 2;
        }
    }
}
