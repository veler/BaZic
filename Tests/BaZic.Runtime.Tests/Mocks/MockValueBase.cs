using System;

namespace BaZic.Runtime.Tests.Mocks
{
    internal class MockValueBase
    {
        public event EventHandler<EventArgs> MockEventChanged;

        public int field;

        public int Foo { get; set; }

        public int Bar { get; private set; }

        public int Boo { private get; set; }

        protected int Buzz { get; set; }

        public MockValueBase()
        {
            Foo = 1;
            Bar = 2;
            Boo = 3;
            Buzz = 4;
            field = 5;
        }

        public void RaiseEvent()
        {
            var ev = MockEventChanged;
            if (ev != null)
            {
                ev(this, new EventArgs());
            }
        }
    }
}
