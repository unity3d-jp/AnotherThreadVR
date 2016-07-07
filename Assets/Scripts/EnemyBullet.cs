using UnityEngine;
using System.Collections.Generic;

namespace UTJ {

public class EnemyBullet : Task
{
	const int POOL_MAX = 4096;
	private static EnemyBullet[] pool_;
	private static int pool_index_;

	public static void createPool()
	{
		pool_ = new EnemyBullet[POOL_MAX];
		for (var i = 0; i < POOL_MAX; ++i) {
			var task = new EnemyBullet();
			task.alive_ = false;
			pool_[i] = task;
		}
		pool_index_ = 0;
	}

	private static EnemyBullet create()
	{
		int cnt = 0;
		while (pool_[pool_index_].alive_) {
			++pool_index_;
			if (pool_index_ >= POOL_MAX)
				pool_index_ = 0;
			++cnt;
			if (cnt >= POOL_MAX) {
				Debug.LogError("EXCEED EnemyBullet POOL!");
				break;
			}
		}
		var eb = pool_[pool_index_];
		return eb;
	}

	public static void create(ref Vector3 position, ref Vector3 target, float speed, double update_time)
	{
		var eb = create();
		eb.init(ref position, ref target, speed, update_time);
	}

	public static void create(ref Vector3 position, ref Quaternion rotation, float speed, double update_time)
	{
		var eb = create();
		eb.init(ref position, ref rotation, speed, update_time);
	}

	public static void create(ref Vector3 position, ref Quaternion rotation, float speed,
							  float width, float length, double update_time)
	{
		var eb = create();
		eb.init(ref position, ref rotation, speed, width, length, update_time);
	}

	private RigidbodyTransform rigidbody_;
	private int collider_;
	private int beam_id_;
	private float length_;
	private double start_;

	public void init(ref Vector3 position, ref Vector3 target, float speed, double update_time)
	{
		var rotation = Quaternion.LookRotation(target - position);
		init(ref position, ref rotation, speed, 0.5f /* width */, 5f /* length */, update_time);
	}
	public void init(ref Vector3 position, ref Quaternion rotation, float speed, double update_time)
	{
		init(ref position, ref rotation, speed, 0.5f /* width */, 5f /* length */, update_time);
	}
	public void init(ref Vector3 position,
					 ref Quaternion rotation,
					 float speed,
					 float width,
					 float length,
					 double update_time)
	{
		base.init();
		rigidbody_.init(ref position, ref rotation);
		var dir = rotation * CV.Vector3Forward;
		var velocity = dir * speed;
		rigidbody_.setVelocity(velocity);
		collider_ = MyCollider.createEnemyBullet();
		MyCollider.initSphereEnemyBullet(collider_, ref position, width);
		start_ = update_time;
		length_ = length;
		beam_id_ = Beam2.Instance.spawn(width, Beam2.Type.EnemyBullet);
	}

	public override void destroy()
	{
		Beam2.Instance.destroy(beam_id_);
		beam_id_ = -1;
		MyCollider.destroyEnemyBullet(collider_);
		base.destroy();
	}

	public override void update(float dt, double update_time, float flow_speed)
	{
		if (MyCollider.getHitOpponentForEnemyBullet(collider_) != MyCollider.Type.None) {
			Spark.Instance.spawn(ref rigidbody_.transform_.position_, Spark.Type.EnemyBullet, update_time);
			destroy();
			return;
		}

		rigidbody_.update(dt);
		MyCollider.updateEnemyBullet(collider_, ref rigidbody_.transform_.position_);

		if (TubeScroller.Instance.checkIntersectionWithSphere(ref rigidbody_.transform_.position_,
															  0.5f /* radius */)) {
			Spark.Instance.spawn(ref rigidbody_.transform_.position_, Spark.Type.EnemyBullet, update_time);
			destroy();
			return;
		}

		if (update_time - start_ > 3f) { // 寿命
			destroy();
			return;
		}
	}

	public override void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer)
	{
		var tail = rigidbody_.transform_.position_ - (rigidbody_.velocity_.normalized * length_);
		Beam2.Instance.renderUpdate(front,
									beam_id_,
									ref rigidbody_.transform_.position_,
									ref tail);
	}

}

} // namespace UTJ {
