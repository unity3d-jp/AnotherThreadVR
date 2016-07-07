using UnityEngine;
using System.Collections;

namespace UTJ {

public class SightTest : MonoBehaviour {

	public GameObject prefab_;
	public Material material_;
	private SightUnitTest[] unit_list_;

	void Awake()
	{
		Sight.Instance.init(material_);
	}
	
	void Start()
	{
		unit_list_ = new SightUnitTest[32];
		for (var i = 0; i < unit_list_.Length; ++i) {
			var pos = new Vector3(Random.Range(-10f, 10f),
								  Random.Range(-10f, 10f),
								  Random.Range(-10f, 10f));
			var go = Instantiate(prefab_, pos, Quaternion.identity) as GameObject;
			unit_list_[i] = go.AddComponent<SightUnitTest>();
			unit_list_[i].init(Time.time);
		}
	}

	void Update()
	{
		Sight.Instance.begin();
		for (var i = 0; i < unit_list_.Length; ++i) {
			unit_list_[i].renderUpdate(Time.time);
		}
		Sight.Instance.end(0 /* front */);

		Sight.Instance.render(0 /* front */, Camera.main, Time.time);
	}
}

} // namespace UTJ {
