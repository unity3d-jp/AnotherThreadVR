using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public partial class Enemy : Task
{
	const int POOL_MAX = 2048;
	private static Enemy[] pool_;
	private static int pool_index_;
	private static Dragon dragon_pool_;

	public static void createPool()
	{
		pool_ = new Enemy[POOL_MAX];
		for (var i = 0; i < POOL_MAX; ++i) {
			var enemy = new Enemy();
			enemy.alive_ = false;
			// enemy.type_ = Type.None;
			pool_[i] = enemy;
		}
		pool_index_ = 0;

		dragon_pool_ = new Dragon();
		dragon_pool_.createPool();
	}
	

	public enum Type {
		None,
		Zako,
		Zako2,
		Dragon,
	}
	private enum Phase {
		Alive,
		Dying,
	}

	private RigidbodyTransform rigidbody_;
	private int collider_;
	// private Type type_;
	private float life_;
	private IEnumerator enumerator_;
	private double update_time_;
	private Phase phase_;
	private Vector3 target_position_;
	private LockTarget lock_target_;
	private delegate void OnUpdateFunc(float dt, float flow_speed);
	private OnUpdateFunc on_update_;
	private delegate void OnRenderUpdateFunc(int front, ref DrawBuffer draw_buffer);
	private OnRenderUpdateFunc on_render_update_;

	public static Enemy create(Type type)
	{
		Enemy enemy = Enemy.create();
		// enemy.type_ = type;
		enemy.phase_ = Phase.Alive;
		enemy.init();
		switch (type) {
			case Type.None:
				Debug.Assert(false);
				break;
			case Type.Zako:
				{
					var position = new Vector3(MyRandom.Range(-15f, 15f), MyRandom.Range(-6f, 6f), -100f);
					enemy.zako_init(ref position, ref CV.QuaternionIdentity);
				}
				break;
			case Type.Zako2:
				{
					var position = new Vector3(MyRandom.Range(-6f, 6f),
											   MyRandom.Range(-6f, 6f),
											   MyRandom.Range(194, 198f));
					enemy.zako2_init(ref position, ref CV.Quaternion180Y);
				}
				break;
			case Type.Dragon:
				{
					enemy.dragon_init();
				}
				break;
		}
		return enemy;
	}

	private static Enemy create()
	{
		int cnt = 0;
		while (pool_[pool_index_].alive_) {
			++pool_index_;
			if (pool_index_ >= POOL_MAX)
				pool_index_ = 0;
			++cnt;
			if (cnt >= POOL_MAX) {
				Debug.LogError("EXCEED Enemy POOL!");
				break;
			}
		}
		var enemy = pool_[pool_index_];
		return enemy;
	}

	private void calc_lock_position_center(ref Vector3 position)
	{
		position = rigidbody_.transform_.position_;
	}

	public override void destroy()
	{
		// type_ = Type.None;
		enumerator_ = null;
		lock_target_.destroy();
		lock_target_ = null;
		MyCollider.destroyEnemy(collider_);
		base.destroy();
	}

	public override void update(float dt, double update_time, float flow_speed)
	{
		if (phase_ == Phase.Dying) {
			destroy();
			return;
		}

		update_time_ = update_time;
		if (enumerator_ != null) {
			enumerator_.MoveNext();
		}
		if (alive_) {
			on_update_(dt, flow_speed);
		}
	}

	public override void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer)
	{
		on_render_update_(front, ref draw_buffer);
	}
}

} // namespace UTJ {
