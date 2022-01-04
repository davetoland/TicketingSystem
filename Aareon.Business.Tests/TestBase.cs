using AutoFixture;

namespace Aareon.Business.Tests
{
    public class TestBase
    {
        protected Fixture Fixture;

        public virtual void Setup()
        {
            Fixture = new Fixture();
        }
    }
}