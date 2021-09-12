namespace SociallyDistant.Utilities
{
    public static class IpUtils
    {
        public const uint Loopback = 0x7f000001u;
        public const uint Zero = 0u;
        public const uint Broadcast = 0xffffffffu;

        public static uint AddressFromOctets(byte o1, byte o2, byte o3, byte o4)
        {
            var address = 0u;

            address |= o4;
            address |= (uint) (o3 << 8);
            address |= (uint) (o2 << 16);
            address |= (uint) (o1 << 24);
            
            return address;
        }

        public static void GetOctets(uint address, out byte o1, out byte o2, out byte o3, out byte o4)
        {
            o1 = (byte) (address >> 24);
            o2 = (byte) (address >> 16);
            o3 = (byte) (address >> 8);
            o4 = (byte) (address);
        }

        public static string IpToString(uint address)
        {
            GetOctets(address, out var b1, out var b2, out var b3, out var b4);

            return $"{b1}.{b2}.{b3}.{b4}";
        }

        public static bool IsLoopback(uint address)
        {
            return Loopback == address;
        }
    }
}