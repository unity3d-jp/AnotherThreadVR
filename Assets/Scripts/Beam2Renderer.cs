using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class Beam2Renderer : MonoBehaviour {

	private MeshFilter mf_;
	private MeshRenderer mr_;

	void Start()
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		mf_.sharedMesh = Beam2.Instance.getMesh();
		mr_.sharedMaterial = Beam2.Instance.getMaterial();
		mr_.SetPropertyBlock(Beam2.Instance.getMaterialPropertyBlock());
	}
}

} // namespace UTJ {
