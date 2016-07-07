
using UnityEngine;
using System.Collections;

namespace UTJ {

public class SightUnitTest : MonoBehaviour {

	private int sight_id_;

	public void init(float time)
	{
		sight_id_ = Sight.Instance.spawn(time);
	}

	public void renderUpdate(float time)
	{
		var pos = transform.position;
		Sight.Instance.renderUpdate(sight_id_, ref pos);
	}

}

} // namespace UTJ {
