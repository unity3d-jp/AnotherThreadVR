using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class ExplosionTest : MonoBehaviour {

	public Camera camera_;
	public Material material_;
	private bool ready_ = false;
	private float time_ = 0f;

	IEnumerator loop()
	{
		ready_ = true;

		GetComponent<MeshRenderer>().sharedMaterial = material_;
		var range = 2.5f;
		for (;;) {
			Explosion.Instance.begin();
			var pos = new Vector3(Random.Range(-range, range),
								  Random.Range(-range, range),
								  Random.Range(-range, range));
			Explosion.Instance.spawn(ref pos, time_);
			Explosion.Instance.end(0 /* front */);
			yield return new WaitForSeconds(1f);
		}
	}

	void Awake()
	{
		Explosion.Instance.init(material_);
	}

	void Start()
	{
		StartCoroutine(loop());
	}
	
	void Update()
	{
		if (!ready_) {
			return;
		}
		Explosion.Instance.render(0 /* front */, camera_, time_, -1f /* flow_speed */);
		var mesh = Explosion.Instance.getMesh();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		var material = Explosion.Instance.getMaterial();
		GetComponent<MeshRenderer>().material = material;

		time_ += 0.001f;
	}
}

} // namespace UTJ {
