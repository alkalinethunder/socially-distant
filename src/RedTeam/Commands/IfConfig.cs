using RedTeam.Net;

namespace RedTeam.Commands
{
    public class IfConfig : Command
    {
        public override string Name => "ifconfig";

        protected override void Main(string[] args)
        {
            Console.WriteLine($@"eth0: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet {Network.LocalAddress}  netmask {Network.SubnetMask}
        ether 0a:1b:2c:3d:4e:5f  txqueuelen 1000  (Ethernet)
        RX packets 56972954  bytes 50786519373 (50.7 GB)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 50655402  bytes 42644755644 (42.6 GB)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0");
        }
    }
}