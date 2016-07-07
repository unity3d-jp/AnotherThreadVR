using UnityEngine;
using System.Collections;

namespace UTJ {

public class DebrisTest : MonoBehaviour {

	public Material material_;

	void Awake()
	{
		Debris.Instance.init(material_);
	}
	
	void Update()
	{
		Debris.Instance.render(0 /* front */, Camera.main, Time.realtimeSinceStartup, 0.0f /* flow_speed */, 1.0f/60.0f);
	}
}

} // namespace UTJ {
