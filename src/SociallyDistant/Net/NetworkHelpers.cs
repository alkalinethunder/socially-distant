namespace SociallyDistant.Net
{
    public static class NetworkHelpers
    {
        public const uint RegionSubnetMask = 0xff000000;
        public const uint IspSubnetMask = 0xffff0000;
        public const uint NetworkSubnetMask = 0xffffff00;
        public const uint LoopbackAddress = 0x7f000001;
        
        public static readonly uint[] LocalNets = new uint[]
        {
            0x0a000000, // 10.0.0.0/24,
            0xc0a80000, // 192.168.0.0/24
            0xc0a80100, // 192.168.1.0/24
        };
        
        public static string ToIPv4String(uint address)
        {
            var oct1 = (byte) address;
            var oct2 = (byte) (address >> 8);
            var oct3 = (byte) (address >> 16);
            var oct4 = (byte) (address >> 24);

            return $"{oct4}.{oct3}.{oct2}.{oct1}";
        }

        public static string ToCidrMask(uint subnet, uint mask)
        {
            var maskBits = CountBits(mask);
            var subnetAddress = ToIPv4String(subnet);

            return subnetAddress + "/" + maskBits.ToString();
        }
        
        public static int CountBits(uint value)
        {
            var result = 0;
            while (value > 0)
            {
                if ((value & 1) == 1)
                    result++;
                value = value >> 1;
            }
            return result;
        }

        public static bool TryParseIp(string host, out uint ipAddress)
        {
            var result = false;
            ipAddress = 0;

            var split = host.Split('.');
            if (split.Length == 4)
            {
                var seg1 = split[0];
                var seg2 = split[1];
                var seg3 = split[2];
                var seg4 = split[3];

                if (byte.TryParse(seg1, out byte oct1) && byte.TryParse(seg2, out byte oct2) &&
                    byte.TryParse(seg3, out byte oct3) && byte.TryParse(seg4, out byte oct4))
                {
                    ipAddress = ((uint) oct1 << 24) | ((uint) oct2 << 16) | ((uint) oct3 << 8) | oct4;
                    result = true;
                }
            }
            
            return result;
        }
    }
}