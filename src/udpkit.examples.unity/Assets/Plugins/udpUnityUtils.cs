using UdpKit;

public static class UdpKitUnityUtils {
    public static UdpSocket CreatePlatformSpecificSocket<TSerializer> () where TSerializer : UdpSerializer, new() {
        return CreatePlatformSpecificSocket<TSerializer>(new UdpConfig());
    }

    public static UdpSocket CreatePlatformSpecificSocket<TSerializer> (UdpConfig config) where TSerializer : UdpSerializer, new() {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        return UdpSocket.Create<UdpPlatformManaged, TSerializer>(config);
#elif UNITY_IPHONE
		return UdpSocket.Create<UdpPlatformIOS, TSerializer>(config);
#elif UNITY_ANDROID
		return UdpSocket.Create<UdpPlatformAndroid, TSerializer>(config);
#else
		throw new System.NotImplementedException ("UdpKit doesn't support the current platform");
#endif
    }
}
