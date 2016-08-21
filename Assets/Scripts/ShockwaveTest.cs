using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class ShockwaveTest : MonoBehaviour {

	public Material material_;
	private bool ready_ = false;
	private int cnt_ = 0;

	IEnumerator loop()
	{
		ready_ = true;

		GetComponent<MeshRenderer>().sharedMaterial = material_;
		var range = 2.5f;
		for (;;) {
			var pos = new Vector3(Random.Range(-range, range),
								  Random.Range(-range, range),
								  Random.Range(-range, range));
			Shockwave.Instance.begin();
			--cnt_;
			if (cnt_ <= 0) {
				Shockwave.Instance.spawn(ref pos, Time.time);
				cnt_ = 30;
			}
			Shockwave.Instance.end(0 /* front */);
			yield return null;
		}
	}

	void Start()
	{
		Shockwave.Instance.init(material_);
		StartCoroutine(loop());
	}
	
	void Update()
	{
		if (!ready_) {
			return;
		}
		Shockwave.Instance.render(0 /* front */, Camera.main, Time.time);
		var mesh = Shockwave.Instance.getMesh();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		var material = Shockwave.Instance.getMaterial();
		GetComponent<MeshRenderer>().material = material;
	}
}

} // namespace UTJ {
