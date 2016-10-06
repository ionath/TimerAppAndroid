using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimerAppShared;

namespace TimerUnitTest
{
    [TestClass]
    public class BitfieldUnitTest
    {
        const int FLAG0 = 1 << 0;
        const int FLAG1 = 1 << 1;
        const int FLAG2 = 1 << 2;
        const int FLAG3 = 1 << 3;
        const int FLAG4 = 1 << 4;
        const int FLAG5 = 1 << 5;
        const int FLAG6 = 1 << 6;
        const int FLAG7 = 1 << 7;
        const int FLAG8 = 1 << 8;
        const int FLAG9 = 1 << 9;
        const int FLAG10 = 1 << 10;
        const int FLAG11 = 1 << 11;
        const int FLAG12 = 1 << 12;
        const int FLAG13 = 1 << 13;
        const int FLAG14 = 1 << 14;
        const int FLAG15 = 1 << 15;
        const int FLAG16 = 1 << 16;
        const int FLAG17 = 1 << 17;
        const int FLAG18 = 1 << 18;
        const int FLAG19 = 1 << 19;
        const int FLAG20 = 1 << 20;
        const int FLAG21 = 1 << 21;
        const int FLAG22 = 1 << 22;
        const int FLAG23 = 1 << 23;
        const int FLAG24 = 1 << 24;
        const int FLAG25 = 1 << 25;
        const int FLAG26 = 1 << 26;
        const int FLAG27 = 1 << 27;
        const int FLAG28 = 1 << 28;
        const int FLAG29 = 1 << 29;
        const int FLAG30 = 1 << 30;
        const int FLAG31 = 1 << 31;

        [TestMethod]
        public void TestSetBits1()
        {
            BitField bitfield = new BitField();
            bitfield.SetBits(FLAG0);
            bool testFlag0 = bitfield.GetBit(FLAG0);
            bool testFlag1 = bitfield.GetBit(FLAG1);
            bool testFlag15 = bitfield.GetBit(FLAG15);
            bool testFlag31 = bitfield.GetBit(FLAG31);
            Assert.AreEqual(testFlag0, true);
            Assert.AreEqual(testFlag1, false);
            Assert.AreEqual(testFlag15, false);
            Assert.AreEqual(testFlag31, false);
        }

        [TestMethod]
        public void TestSetBits2()
        {
            BitField bitfield = new BitField();
            bitfield.SetBits(FLAG0|FLAG2|FLAG10|FLAG31);
            bool testFlag0 = bitfield.GetBit(FLAG0);
            bool testFlag1 = bitfield.GetBit(FLAG1);
            bool testFlag2 = bitfield.GetBit(FLAG2);
            bool testFlag10 = bitfield.GetBit(FLAG10);
            bool testFlag15 = bitfield.GetBit(FLAG15);
            bool testFlag30 = bitfield.GetBit(FLAG30);
            bool testFlag31 = bitfield.GetBit(FLAG31);
            Assert.AreEqual(testFlag0, true);
            Assert.AreEqual(testFlag1, false);
            Assert.AreEqual(testFlag2, true);
            Assert.AreEqual(testFlag10, true);
            Assert.AreEqual(testFlag15, false);
            Assert.AreEqual(testFlag30, false);
            Assert.AreEqual(testFlag31, true);
        }

        [TestMethod]
        public void TestChangeBits()
        {
            BitField bitfield = new BitField();
            bitfield.ChangeBits(FLAG0 | FLAG4 | FLAG10 | FLAG31, BitField.TRUE);
            bool testFlag0 = bitfield.GetBit(FLAG0);
            bool testFlag1 = bitfield.GetBit(FLAG1);
            bool testFlag4 = bitfield.GetBit(FLAG4);
            bool testFlag10 = bitfield.GetBit(FLAG10);
            bool testFlag17 = bitfield.GetBit(FLAG17);
            bool testFlag30 = bitfield.GetBit(FLAG30);
            bool testFlag31 = bitfield.GetBit(FLAG31);
            Assert.AreEqual(testFlag0, true);
            Assert.AreEqual(testFlag1, false);
            Assert.AreEqual(testFlag4, true);
            Assert.AreEqual(testFlag10, true);
            Assert.AreEqual(testFlag17, false);
            Assert.AreEqual(testFlag30, false);
            Assert.AreEqual(testFlag31, true);
        }

        [TestMethod]
        public void TestClearBits()
        {
            BitField bitfield = new BitField();
            bitfield.SetBits(-1);
            Assert.AreEqual(bitfield.GetBit(FLAG0), true);
            Assert.AreEqual(bitfield.GetBit(FLAG1), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), true);
            bitfield.ClearBits(FLAG0);
            Assert.AreEqual(bitfield.GetBit(FLAG0), false);
            Assert.AreEqual(bitfield.GetBit(FLAG1), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), true);
            bitfield.ClearBits(FLAG31);
            Assert.AreEqual(bitfield.GetBit(FLAG0), false);
            Assert.AreEqual(bitfield.GetBit(FLAG1), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), false);
            bitfield.ClearBits(FLAG1|FLAG2);
            Assert.AreEqual(bitfield.GetBit(FLAG1), false);
            Assert.AreEqual(bitfield.GetBit(FLAG0), false);
            Assert.AreEqual(bitfield.GetBit(FLAG2), false);
            Assert.AreEqual(bitfield.GetBit(FLAG3), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), false);
        }

        [TestMethod]
        public void TestToggleBits()
        {
            BitField bitfield = new BitField();
            bitfield.ToggleBits(FLAG0|FLAG2|FLAG16|FLAG31);
            Assert.AreEqual(bitfield.GetBit(FLAG0), true);
            Assert.AreEqual(bitfield.GetBit(FLAG1), false);
            Assert.AreEqual(bitfield.GetBit(FLAG2), true);
            Assert.AreEqual(bitfield.GetBit(FLAG16), true);
            Assert.AreEqual(bitfield.GetBit(FLAG30), false);
            Assert.AreEqual(bitfield.GetBit(FLAG31), true);
            bitfield.ToggleBits(FLAG1 | FLAG2 | FLAG17 | FLAG30);
            Assert.AreEqual(bitfield.GetBit(FLAG0), true);
            Assert.AreEqual(bitfield.GetBit(FLAG1), true);
            Assert.AreEqual(bitfield.GetBit(FLAG2), false);
            Assert.AreEqual(bitfield.GetBit(FLAG16), true);
            Assert.AreEqual(bitfield.GetBit(FLAG17), true);
            Assert.AreEqual(bitfield.GetBit(FLAG30), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), true);
            bitfield.ToggleBits(FLAG0 | FLAG2 | FLAG17 | FLAG31);
            Assert.AreEqual(bitfield.GetBit(FLAG0), false);
            Assert.AreEqual(bitfield.GetBit(FLAG1), true);
            Assert.AreEqual(bitfield.GetBit(FLAG2), true);
            Assert.AreEqual(bitfield.GetBit(FLAG16), true);
            Assert.AreEqual(bitfield.GetBit(FLAG17), false);
            Assert.AreEqual(bitfield.GetBit(FLAG30), true);
            Assert.AreEqual(bitfield.GetBit(FLAG31), false);
        }
    }
}
