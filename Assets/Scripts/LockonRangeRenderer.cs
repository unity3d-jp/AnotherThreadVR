using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class LockonRangeRenderer : MonoBehaviour {
	// singleton
	static LockonRangeRenderer instance_;
	public static LockonRangeRenderer Instance { get { return instance_; } }
	private MeshFilter mf_;
	private MeshRenderer mr_;
	private UnityEngine.Rendering.CommandBuffer command_buffer_;

	public static void setInstance(LockonRangeRenderer lr)
	{
		instance_ = lr;
	}

	public void init(Camera camera)
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		mr_.enabled = false;
		mf_.sharedMesh = LockonRange.Instance.getMesh();
		mr_.sharedMaterial = LockonRange.Instance.getMaterial();
		command_buffer_ = new UnityEngine.Rendering.CommandBuffer();
		command_buffer_.DrawRenderer(mr_, LockonRange.Instance.getMaterial());
		camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterImageEffects, command_buffer_);
	}

	public void render(double render_time, float dt)
	{
		var val = Mathf.PerlinNoise((float)render_time*5f, 0f)*2f - 1f;
		transform.Rotate(new Vector3(0f, 0f, 1000f*val*dt), Space.Self);
	}
}

} // namespace UTJ {
