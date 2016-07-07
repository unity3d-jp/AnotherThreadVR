using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace UTJ {

public class PerformanceFetcher : MonoBehaviour {

	void LateUpdate()
	{
		SystemManager.Instance.endPerformanceMeter();
	}

	void OnPreCull()
	{
		SystemManager.Instance.beginPerformanceMeter();
	}

	void OnPostRender()
	{
		SystemManager.Instance.endPerformanceMeter2();
	}

#if UNITY_PS4 && !UNITY_EDITOR && DEBUG
	[DllImport("NativePluginPS4")]
	private static extern int plugin_sceRazorCpuPushMarker(string label);
	[DllImport("NativePluginPS4")]
	private static extern int plugin_sceRazorCpuPopMarker();
#else
	private static int plugin_sceRazorCpuPushMarker(string label) { return 0; }
	private static int plugin_sceRazorCpuPopMarker() { return 0; }
#endif

	public static void PushMarker(string label)
	{
		plugin_sceRazorCpuPushMarker(label);
	}
	public static void PopMarker()
	{
		plugin_sceRazorCpuPopMarker();
	}

}

} // namespace UTJ {
