using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FlangeConnection.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void findEffectWidthOfSeal_10_10returned()
        {
            int b_p = 10;
            int expected = 10;

            Calc calc = new Calc();
            float actual = calc.findEffectWidthOfSeal(b_p);

            Assert.AreEqual(expected, actual);
        }
    }
}
