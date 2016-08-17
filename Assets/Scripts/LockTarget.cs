using UnityEngine;

namespace UTJ {

public class LockTarget
{
	const int POOL_MAX = 128;
	private static LockTarget[] pool_;
	private static int pool_index_;
	private static int lock_max_ = 16;
	private static int fired_num_ = 0;
	private static int lock_num_ = 0;
	public delegate void CalcPosition(ref Vector3 result);

	public static int getCurrentLockNum() { return lock_num_; }
	public static int getCurrentFiredNum() { return fired_num_; }

	public static void createPool()
	{
		pool_ = new LockTarget[POOL_MAX];
		for (var i = 0; i < POOL_MAX; ++i) {
			var obj = new LockTarget();
			obj.alive_ = false;
			pool_[i] = obj;
		}
		pool_index_ = 0;
	}

	public static LockTarget create(Task owner_task, CalcPosition calc_position_func)
	{
		int cnt = 0;
		while (pool_[pool_index_].alive_) {
			++pool_index_;
			if (pool_index_ >= POOL_MAX)
				pool_index_ = 0;
			++cnt;
			if (cnt >= POOL_MAX) {
				Debug.LogError("EXCEED LockTarget POOL!");
				break;
			}
		}
		var obj = pool_[pool_index_];
		obj.init(owner_task, calc_position_func);
		return obj;
	}

	public static void restart()
	{
		for (var i = 0; i < pool_.Length; ++i) {
			pool_[i].alive_ = false;
			pool_[i].disabled_ = false;
		}
	}

	public static bool checkAll(Player player, double update_time)
	{
		if (lock_num_ >= lock_max_) {
			return false;
		}

		bool locked = false;
		// var mat = player.rigidbody_.transform_.getTRS();
		// var inv_mat = mat.inverse;
		var inv_mat = player.rigidbody_.transform_.getInverseR();
		var aiming_point = player.rigidbody_.transform_.position_ + Player.AIMING_OFFSET;

		for (var i = 0; i < pool_.Length; ++i) {
			var lock_target = pool_[i];
			if (lock_target.alive_ &&
				!lock_target.disabled_ &&
				!lock_target.fired_ &&
				!lock_target.locked_ &&
				!lock_target.hitted_) {
				if (lock_target.isEntering(ref inv_mat,
										   ref aiming_point,
										   (12.5f/200f) /* ratio */,
										   1f /* dist_min */,
										   100f /* dist_max */)) {
					lock_target.setLock(update_time);
					locked = true;
					++lock_num_;
					if (lock_num_ >= lock_max_) {
						break;
					}
				}
			}
		}
		return locked;
	}

	public static bool fireMissiles(Player player)
	{
		bool fired = false;
		if (lock_num_ > 0) {
			for (var i = 0; i < pool_.Length; ++i) {
				var lock_target = pool_[i];
				if (lock_target.alive_ && !lock_target.disabled_) {
					if (lock_target.locked_ && !lock_target.fired_) {
						Missile.create(ref player.rigidbody_.transform_.position_,
									   ref player.rigidbody_.transform_.rotation_,
									   lock_target);
						lock_target.fired_ = true;
						++fired_num_;
						fired = true;
					}
				}
			}
		}
		return fired;
	}

	public bool alive_;
	public bool disabled_;
	private Task owner_task_;
	private CalcPosition calc_position_func_;
	public Vector3 updated_position_;
	public bool locked_;
	public bool fired_;
	public bool hitted_;
	private int sight_id_;
	
	private void init(Task owner_task, CalcPosition calc_position_func)
	{
		alive_ = true;
		disabled_ = false;
		owner_task_ = owner_task;
		calc_position_func_ = calc_position_func;
		calc_position_func_(ref updated_position_);
		locked_ = false;
		fired_ = false;
		hitted_ = false;
		sight_id_ = -1;
	}
	
	public void destroy()
	{
		alive_ = false;
		if (fired_) {
			--fired_num_;
			fired_ = false;
		}
		if (locked_) {
			--lock_num_;
			locked_ = false;
		}
		if (sight_id_ >= 0) {
			Sight.Instance.destroy(sight_id_);
			sight_id_ = -1;
		}
	}

	public void disable()
	{
		disabled_ = true;
		if (fired_) {
			--fired_num_;
			fired_ = false;
		}
		if (locked_) {
			--lock_num_;
			locked_ = false;
		}
		if (sight_id_ >= 0) {
			Sight.Instance.destroy(sight_id_);
			sight_id_ = -1;
		}
	}

	public void update()
	{
		if (owner_task_.alive_) {
			calc_position_func_(ref updated_position_);
		} else {
			destroy();
		}
	}

	public bool isHitted()
	{
		return hitted_;
	}

	public void hit()
	{
		hitted_ = true;
	}

	public void setLock(double update_time)
	{
		locked_ = true;
		if (sight_id_ >= 0) {
			Sight.Instance.destroy(sight_id_);
		}
		sight_id_ = Sight.Instance.spawn(update_time);
	}

	public void clearLock()
	{
		if (locked_) {
			--lock_num_;
			locked_ = false;
		}
		if (fired_) {
			--fired_num_;
			fired_ = false;
		}
		hitted_ = false;
		if (sight_id_ >= 0) {
			Sight.Instance.destroy(sight_id_);
		}
		sight_id_ = -1;
	}

	public bool isEntering(ref Matrix4x4 inv_mat,
						   ref Vector3 pos,
						   float ratio,
						   float dist_min,
						   float dist_max)
	{
		var point = updated_position_ - pos;
		point = inv_mat.MultiplyVector(point);
		if (dist_min < point.z && point.z < dist_max) {
			var dist_xy2 = point.x*point.x + point.y*point.y;
			var dist_xy = Mathf.Sqrt(dist_xy2);
			var grad = dist_xy / point.z;
			if (grad < ratio) {
				return true;
			}
		}
		return false;
	}

	public void renderUpdate(int front)
	{ 
		if (!disabled_ && (locked_ || fired_)) {
			if (sight_id_ >= 0) {
				Sight.Instance.renderUpdate(sight_id_,
											ref updated_position_);
				if (fired_) {
					var size = new Vector2(8f, 8f);
					VRSprite.Instance.renderUpdate(front,
												   ref updated_position_,
												   ref size,
												   MySprite.Kind.Target,
												   VRSprite.Type.LockFired);
				} else {
					var size = new Vector2(7.5f, 7.5f);
					VRSprite.Instance.renderUpdate(front,
												   ref updated_position_,
												   ref size,
												   MySprite.Kind.Target,
												   VRSprite.Type.Locked);
				}
			}
		}
	}

}

} // namespace UTJ {
