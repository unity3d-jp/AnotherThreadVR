using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class ShieldRenderer : MonoBehaviour {
	// singleton
	static ShieldRenderer instance_;
	public static ShieldRenderer Instance { 
		get {
			if (instance_ == null) {
				Debug.Assert(false);
			}
			return instance_;
		}
	}
	public static void setInstance(ShieldRenderer sr)
	{
		instance_ = sr;
	}

	private MeshFilter mf_;
	private MeshRenderer mr_;

	void Start()
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		mf_.sharedMesh = Shield.Instance.getMesh();
		mr_.sharedMaterial = Shield.Instance.getMaterial();
		mr_.SetPropertyBlock(Shield.Instance.getMaterialPropertyBlock());
	}
}

} // namespace UTJ {
