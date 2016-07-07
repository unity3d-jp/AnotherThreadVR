
using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class SightRenderer : MonoBehaviour {
	// singleton
	static SightRenderer instance_;
	public static SightRenderer Instance { get { return instance_; } }

	private MeshFilter mf_;
	private MeshRenderer mr_;
	private UnityEngine.Rendering.CommandBuffer command_buffer_;

	void Awake()
	{
		instance_ = this;
	}

	public void init(Camera camera)
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		mr_.enabled = false;
		mf_.sharedMesh = Sight.Instance.getMesh();
		mr_.sharedMaterial = Sight.Instance.getMaterial();
		command_buffer_ = new UnityEngine.Rendering.CommandBuffer();
		command_buffer_.DrawRenderer(mr_, Sight.Instance.getMaterial());
		camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterImageEffects, command_buffer_);
	}
}

} // namespace UTJ {
