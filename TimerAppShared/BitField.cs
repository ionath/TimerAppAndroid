using System;
using System.Collections.Generic;
using System.Text;

namespace TimerAppShared
{
    public struct BitField
    {
        int bitfield;

        public const int TRUE = 1;
        public const int FALSE = 0;
        
        public BitField(int initialValue)
        {
            bitfield = initialValue;
        }

        public void SetBit(int flag, bool value)
        {
            if (value)
            {
                bitfield |= flag;
            }
            else
            {
                bitfield &= ~flag;
            }
        }

        public void ChangeBits(int flags, int value)
        {
            bitfield ^= (-value ^ bitfield) & flags;
        }

        public void SetBits(int flags)
        {
            bitfield |= flags;
        }

        public void ClearBits(int flags)
        {
            bitfield &= ~flags;
        }

        public void ToggleBits(int flags)
        {
            bitfield ^= flags;
        }

        public static explicit operator int(BitField v)
        {
            return v.bitfield;
        }

        public bool GetBit(int flag)
        {
            return (bitfield & flag) != 0;
        }

        internal int ToInt()
        {
            return bitfield;
        }
    }
}
