using UdpKit;

public class demoSerializer : UdpSerializer {
    public override bool Pack (ref UdpBitStream stream, ref object o) {
        throw new System.NotImplementedException();
    }

    public override bool Unpack (ref UdpBitStream stream, ref object o) {
        throw new System.NotImplementedException();
    }
}
