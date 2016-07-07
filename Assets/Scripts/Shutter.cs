using UnityEngine;
using System.Collections.Generic;

namespace UTJ {

public class Shutter : Task
{
	const int POOL_MAX = 16;
	private static Shutter[] pool_;
	private static int pool_index_;

	public static void createPool()
	{
		pool_ = new Shutter[POOL_MAX];
		for (var i = 0; i < POOL_MAX; ++i) {
			var task = new Shutter();
			task.alive_ = false;
			pool_[i] = task;
		}
		pool_index_ = 0;
	}

	public static void create(float rot, float flow_speed, double update_time)
	{
		int cnt = 0;
		while (pool_[pool_index_].alive_) {
			++pool_index_;
			if (pool_index_ >= POOL_MAX)
				pool_index_ = 0;
			++cnt;
			if (cnt >= POOL_MAX) {
				Debug.LogError("EXCEED Shutter POOL!");
				break;
			}
		}
		var task = pool_[pool_index_];

		var rotation = Quaternion.Euler(0f, 0f, rot);
		var offset = 12f;
		var far = 40f;
		var z = 700f;
		var margin = 20f;
		var duration = (z+margin)/Mathf.Abs(flow_speed);
		var stop_time = update_time + duration;
		var pos = rotation * CV.Vector3Down;
		var position = new Vector3(pos.x * far, pos.y * far, z);
		var speed = (far - offset)/duration;
		task.init(ref position, ref rotation, speed, stop_time);
	}

	private RigidbodyTransform rigidbody_;
	private int collider_;
	private double stop_time_;

	public void init(ref Vector3 position, ref Quaternion rotation, float speed, double stop_time)
	{
		base.init();
		rigidbody_.init(ref position, ref rotation);
		var dir = rotation * CV.Vector3Up;
		var velocity = dir * speed;
		rigidbody_.setVelocity(velocity);
		collider_ = MyCollider.createEnemyBullet();
		MyCollider.initSphereEnemyBullet(collider_, ref position, 0.5f /* radius */);
		stop_time_ = stop_time;
	}

	public override void destroy()
	{
		MyCollider.destroyEnemyBullet(collider_);
		base.destroy();
	}

	public override void update(float dt, double update_time, float flow_speed)
	{
		if (update_time > stop_time_) {
			rigidbody_.velocity_.x = 0f;
			rigidbody_.velocity_.y = 0f;
		}
		rigidbody_.velocity_.z = flow_speed;
		rigidbody_.update(dt);
		if (rigidbody_.transform_.position_.z < -400f) {
			destroy();
			return;
		}
		MyCollider.updateEnemyBullet(collider_, ref rigidbody_.transform_.position_);
	}

	public override void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer)
	{
		draw_buffer.regist(ref rigidbody_.transform_, DrawBuffer.Type.Shutter);
	}

}

} // namespace UTJ {
