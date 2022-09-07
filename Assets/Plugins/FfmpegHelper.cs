using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class FfmpegHelper
{

	/* Interface to native implementation */

	[DllImport("__Internal")]
	private static extern void _Excute(string command);

	/* Public interface for use inside C# / JS code */

	// Starts lookup for some bonjour registered service inside specified domain
	public static void Excute(string command)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
	AndroidJavaClass javaClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit");
	AndroidJavaObject session = javaClass.CallStatic<AndroidJavaObject>("execute", new object[] {command});

	//AndroidJavaObject returnCode = session.Call<AndroidJavaObject>("getReturnCode", new object[] {});
	//int rc = returnCode.Call<int>("getValue", new object[] {});
#elif !UNITY_EDITOR && UNITY_IOS
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor)
			_Excute(command);
#endif
	}
}