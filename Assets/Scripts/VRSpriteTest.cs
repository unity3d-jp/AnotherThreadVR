using UnityEngine;
using System.Collections;

namespace UTJ {

public class VRSpriteTest : MonoBehaviour {

	public Sprite[] sprites_;
	public Material material_;

	IEnumerator loop()
	{
		yield return null;
	}

	void Start()
	{
		VRSprite.Instance.init(sprites_, material_);
		StartCoroutine(loop());
	}
	
	void Update()
	{
		VRSprite.Instance.begin();
	    {
			Vector3 pos = new Vector3(0f, 0f, 100f);
			Vector2 size = new Vector2(10f, 10f);
			VRSprite.Instance.renderUpdate(0 /* front */,
										   ref pos,
										   ref size,
										   MySprite.Kind.Square,
										   VRSprite.Type.Blue);
		}
	    {
			Vector3 pos = new Vector3(0f, 10f, 100f);
			Vector2 size = new Vector2(10f, 10f);
			VRSprite.Instance.renderUpdate(0 /* front */,
										   ref pos,
										   ref size,
										   MySprite.Kind.Target,
										   VRSprite.Type.Full);
		}
		VRSprite.Instance.end(0 /* front */);

		VRSprite.Instance.render(0 /* front */, Camera.main);
	}
}

} // namespace UTJ {
