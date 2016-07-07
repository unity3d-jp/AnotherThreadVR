using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public class TestTask : Task
{
	public RigidbodyTransform rigidbody_;
	public LockTarget lock_target_;
	public GameObject owner_;

	public void calc_position(ref Vector3 position)
	{
		position = rigidbody_.transform_.position_;
	}

	public override void update(float dt, double update_time, float flow_speed)
	{
		lock_target_.update();
	}
	public override void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer)
	{
		// lock_target_.renderUpdate(front, camera);
	}
}


public class LockTargetTest : MonoBehaviour
{
	const int MAX = 128;
	private List<TestTask> list_;
	private Camera camera_;
	private Player player_;

	void Start()
	{
		TaskManager.Instance.init();
		// Sight.Instance.init();
		LockTarget.createPool();
		list_ = new List<TestTask>();
		GameObject prefab = null;
		for (var i = 0; i < MAX; ++i) {
			GameObject go;
			if (prefab == null) {
				prefab = GameObject.Find("Cube");
				go = prefab;
			} else {
				float range = 8;
				go = Instantiate(prefab, new Vector3(Random.Range(-range, range),
													 Random.Range(-range, range),
													 Random.Range(-range, range)),
								 Quaternion.identity) as GameObject;
			}
			var task = new TestTask();
			var pos = go.transform.position;
			var rot = go.transform.rotation;
			task.init();
			task.rigidbody_.init(ref pos, ref rot);
			var lock_target = LockTarget.create(task, new LockTarget.CalcPosition(task.calc_position));
			task.lock_target_ = lock_target;
			task.owner_ = go;
			list_.Add(task);
		}
		camera_ = Camera.main;

		player_ = Player.create();
	}


	void Update()
	{
		player_.rigidbody_.transform_.position_ = camera_.transform.position;
		player_.rigidbody_.transform_.rotation_ = camera_.transform.rotation;
		foreach(var t in list_) {
			t.update(1f/60f, Time.time, 0f /* flow_speed */);
		}
		LockTarget.checkAll(player_, Time.time);

		var inv_mat = player_.rigidbody_.transform_.getInverseR();
		foreach(var t in list_) {
			if (t.lock_target_.isEntering(ref inv_mat,
										  ref player_.rigidbody_.transform_.position_,
										  1f /* ratio */,
										  1f /* dist_min */,
										  500f /* dist_max */)) {
				t.owner_.GetComponent<MeshRenderer>().material.color = Color.red;
			} else {
				t.owner_.GetComponent<MeshRenderer>().material.color = Color.white;
			}
		}
	}
}

} // namespace UTJ {
