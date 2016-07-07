using UnityEngine;
using System.Collections;

namespace UTJ {

public class TrailTest : MonoBehaviour {

	public Material material_;

	IEnumerator loop()
	{
		yield return null;
		var pos0 = Vector3.zero;
		var id0 = Trail.Instance.spawn(ref pos0, 0.1f /* width */, Trail.Type.Player);
		// var id1 = Trail.Instance.spawn(ref pos0, 0.1f /* width */, Trail.Type.Player);

		float update_time = 0f;
		for (;;) {
			float dt = (1f/60f) * 0.1f;
			float phase = Mathf.Repeat(update_time*4f, 1f) * Mathf.PI * 2f;
			update_time += dt;
		    {
				var pos = new Vector3(Mathf.Cos(phase), Mathf.Sin(phase), 0f) * 0.5f;
				Trail.Instance.update(id0, ref pos, dt, 10f /* flow_speed */, update_time);
			}
		    // {
			// 	var pos = new Vector3(-Mathf.Cos(phase), -Mathf.Sin(phase), 0f) * 0.5f;
			// 	Trail.Instance.update(id1, ref pos, dt, 0.1f /* flow_speed */, update_time);
			// }
			Trail.Instance.begin(0 /* front */);
			Trail.Instance.renderUpdate(0 /* front */, id0);
			// Trail.Instance.renderUpdate(0 /* front */, id1);
			Trail.Instance.end();

			yield return null;
		}
	}

	void Awake()
	{
		Trail.Instance.init(material_);
	}
	void Start()
	{
		StartCoroutine(loop());
	}
	
	void Update()
	{
		Trail.Instance.render(0 /* front */);
	}
}

} // namespace UTJ {
