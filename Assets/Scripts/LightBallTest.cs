using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class LightBallTest : MonoBehaviour {

	public Material material_;
	private MeshFilter mf_;
	private MeshRenderer mr_;
	
	private LightBall lightball_;

	IEnumerator loop()
	{
		MyTransform transform = new MyTransform();
		transform.init();
		transform.position_ = new Vector3(1, 1, 1);
		var offset = new Vector3(0, 0, 5);
		for (;;) {
			lightball_.update(1f/60f /* dt */);
			lightball_.renderUpdate(0 /* front */, ref transform, ref offset);
			yield return null;
		}
	}

	void Start()
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		Beam.Instance.init(material_);
		var mesh = Beam.Instance.getMesh();
		mf_.sharedMesh = mesh;
		mr_.sharedMaterial = Beam.Instance.getMaterial();
		mr_.SetPropertyBlock(Beam.Instance.getMaterialPropertyBlock());
		lightball_.init();
		StartCoroutine(loop());
	}
	
	void Update()
	{
		Beam.Instance.render(0 /* front */);
	}
}

} // namespace UTJ {
