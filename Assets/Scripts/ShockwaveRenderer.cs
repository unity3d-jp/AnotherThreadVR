using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class ShockwaveRenderer : MonoBehaviour {

	private MeshFilter mf_;
	private MeshRenderer mr_;

	void Start()
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		mf_.sharedMesh = Shockwave.Instance.getMesh();
		mr_.sharedMaterial = Shockwave.Instance.getMaterial();
	}
}

} // namespace UTJ {
