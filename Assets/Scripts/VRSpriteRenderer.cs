using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VRSpriteRenderer : MonoBehaviour {
	// singleton
	static VRSpriteRenderer instance_;
	public static VRSpriteRenderer Instance { get { return instance_; } }

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
		mf_.sharedMesh = VRSprite.Instance.getMesh();
		mr_.sharedMaterial = VRSprite.Instance.getMaterial();
		mr_.SetPropertyBlock(VRSprite.Instance.getMaterialPropertyBlock());
		command_buffer_ = new UnityEngine.Rendering.CommandBuffer();
		command_buffer_.DrawRenderer(mr_, VRSprite.Instance.getMaterial());
		camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterImageEffects, command_buffer_);
	}
}

} // namespace UTJ {
