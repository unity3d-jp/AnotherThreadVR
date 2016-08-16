
// #define REPEAT

using UnityEngine;
using System.Collections;

namespace UTJ {

public class TubeScroller : MonoBehaviour
{
	// singleton
	private static TubeScroller instance_;
	public static TubeScroller Instance {
		get {
			return instance_;
		}
	}
	private static void setInstance(TubeScroller ts)
	{
		instance_ = ts;
	}

	public enum TubeType {
		Type1,
		Type2,
		Type3,
		Type4,
		Type5,
		Type6,

		None,
	}
	const int NUM = 52;
	private TubeType[] pattern_tube_list_ = new TubeType[NUM] {
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type2,
		TubeType.Type3,
		TubeType.Type4,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,

		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type2,

		TubeType.Type3,
		TubeType.Type4,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,

		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type2,
		TubeType.Type5,

		TubeType.Type6,
		TubeType.Type4,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,

		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,

		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
		TubeType.Type1,
	};

	public GameObject title_prefab_;
	public GameObject[] tube_prefabs_;
	const float UNIT_LENGTH = 200f;
	const float UNIT_CENTER_OFFSET = 100f;
	const float REPEAT_LENGTH = UNIT_LENGTH * NUM;
	const float MOST_BEHIND_Z = -500f;
	private Animator animator_ = null;
	private GameObject[] tube_list_;
	private Vector3[] original_position_list_;
	private float progressed_distance_ = 0f;
	
	struct TubeCollider {
		public bool is_box_;
		public Vector2 volume_;
	}
	TubeCollider[] collider_list_;
	const float TUBE_RADIUS = 8f;

	void Awake()
	{
		tube_list_ = new GameObject[NUM];
		original_position_list_ = new Vector3[tube_list_.Length];
		collider_list_ = new TubeCollider[tube_list_.Length];
		progressed_distance_ = 0f;
		int p_idx = 0;
		for (var i = 0; i < tube_list_.Length; ++i) {
			var pattern_tube = pattern_tube_list_[p_idx];
			var pos = new Vector3(0f, 0f, UNIT_LENGTH*i + MOST_BEHIND_Z);
			tube_list_[i] = Instantiate(tube_prefabs_[(int)pattern_tube],
										pos,
										CV.Quaternion180Y) as GameObject;
			if (i == 3) {
				if (title_prefab_) {
					var go = Instantiate(title_prefab_) as GameObject;
					go.transform.SetParent(tube_list_[i].transform);
				}
			}
			original_position_list_[i] = pos;
			switch (pattern_tube) {
				case TubeType.Type1:
					collider_list_[i].is_box_ = false;
					collider_list_[i].volume_.x = TUBE_RADIUS;
					collider_list_[i].volume_.y = TUBE_RADIUS;
					break;
				case TubeType.Type2:
				case TubeType.Type3:
				case TubeType.Type4:
				case TubeType.Type5:
				case TubeType.Type6:
					collider_list_[i].is_box_ = true;
					collider_list_[i].volume_.x = 8*5f;
					collider_list_[i].volume_.y = 8*3f;
					break;
			}

			++p_idx;
			if (p_idx >= pattern_tube_list_.Length) {
				p_idx = 0;
			}
		}
		setInstance(this);
	}

	public void restart()
	{
		progressed_distance_ = 0f;
	}

	public void update(float dt, float flow_speed)
	{
		progressed_distance_ += flow_speed * dt;
#if REPEAT
		if (progressed_distance_ > REPEAT_LENGTH) {
			progressed_distance_ -= REPEAT_LENGTH;
		}
#endif
	}

	public float getDistance()
	{
		return -progressed_distance_;
	}

	private float get_travelled_distance()
	{
#if REPEAT
		float div = progressed_distance_ / REPEAT_LENGTH;
		float frac = div - Mathf.Floor(div);
		float distance = REPEAT_LENGTH * frac;
#else
		float distance = progressed_distance_;
#endif
		return distance;
	}

	private int get_block_index(float z)
	{
		float distance = get_travelled_distance();
		z -= distance;
		z -= MOST_BEHIND_Z;
		z += UNIT_CENTER_OFFSET;
		z /= UNIT_LENGTH;
#if REPEAT
		int index = (int)Mathf.Repeat(z, NUM);
#else
		int index = (int)z;
		if (index >= NUM)
			index = -1;
#endif
		return index;
	}

	public TubeType getTubeType(float z)
	{
		int index = get_block_index(z);
		if (index < 0) {
		    return TubeType.None;
 	    }
		return pattern_tube_list_[index];
	}

	public bool checkIntersectionWithSphere(ref Vector3 pos, float radius)
	{
		int index = get_block_index(pos.z);
		if (index < 0) {
		    return false;
	    }
		if (collider_list_[index].is_box_) {
			if (pos.x < -collider_list_[index].volume_.x + radius)
				return true;
			else if (pos.x > collider_list_[index].volume_.x - radius)
				return true;
			else if (pos.y < -collider_list_[index].volume_.y + radius)
				return true;
			else if (pos.y > collider_list_[index].volume_.y - radius)
				return true;
			else
				return false;
		} else {
			var r2 = pos.x * pos.x + pos.y * pos.y;
			if (r2 > (TUBE_RADIUS-radius)*(TUBE_RADIUS-radius))
				return true;
			else
				return false;
		}
	}

	public void setPause(bool flg)
	{
		if (animator_ != null)
			animator_.speed = (flg ? 0f : 1f);
	}

	public void setOperatorMotionGoodLuck()
	{
		if (animator_ != null)
			animator_.SetBool("goodluck", true);
	}

	public void render()
	{
		float distance = get_travelled_distance();
		for (var i = 0; i < tube_list_.Length; ++i) {
			var pos = original_position_list_[i];
			pos.z += distance;
#if REPEAT
			if (pos.z < MOST_BEHIND_Z) {
				pos.z += REPEAT_LENGTH;
			}
			if (pos.z > MOST_BEHIND_Z + REPEAT_LENGTH) {
				pos.z -= REPEAT_LENGTH;
			}
#endif
			tube_list_[i].transform.position = pos;
		}
	}
}

} // namespace UTJ {
