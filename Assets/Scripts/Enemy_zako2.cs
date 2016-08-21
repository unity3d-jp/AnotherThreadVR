using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public partial class Enemy : Task
{
	public void zako2_init(ref Vector3 position, ref Quaternion rotation)
	{
		rigidbody_.init(ref position, ref rotation);
		collider_ = MyCollider.createEnemy();
		MyCollider.initSphereEnemy(collider_, ref position, 1f /* radius */);
		lock_target_ = LockTarget.create(this, new LockTarget.CalcPosition(calc_lock_position_center));
		enumerator_ = zako2_act(); // この瞬間は実行されない
		on_update_ = new OnUpdateFunc(zako2_update);
		on_render_update_ = new OnRenderUpdateFunc(zako2_render_update);
		life_ = 100f;
	}

	public IEnumerator zako2_act()
	{
		rigidbody_.setDamper(2f);
		rigidbody_.setRotateDamper(20f);
		target_position_.x = MyRandom.Range(-5f, 5f);
		target_position_.y = MyRandom.Range(-5f, 5f);

		yield return null;
		for (var i = new Utility.WaitForSeconds(2.2f, update_time_); !i.end(update_time_);) {
			rigidbody_.addForceZ(-160f);
			rigidbody_.addSpringForceXY(target_position_.x, target_position_.y, 2f);
			yield return null;
		}

		var sec = MyRandom.Range(3f, 5f);
		var fired_time = update_time_;
		var move_force = new Vector3(-target_position_.x, -target_position_.y, 0f);
		move_force.Normalize();
		move_force = Quaternion.Euler(0f, 0f, MyRandom.Range(-45f, 45f)) * move_force * 4f;
		for (var i = new Utility.WaitForSeconds(sec, update_time_); !i.end(update_time_);) {
			rigidbody_.addTargetTorque(ref Player.Instance.rigidbody_.transform_.position_,
									   100f);
			rigidbody_.addForce(ref move_force);
			var target = Player.Instance.rigidbody_.transform_.position_;
			if (update_time_ - fired_time > 0.4f) {
				EnemyBullet.create(ref rigidbody_.transform_.position_,
								   ref target,
								   50f /* speed */,
								   update_time_);
				fired_time = update_time_;
			}
			yield return null;
		}

		for (var i = new Utility.WaitForSeconds(4f, update_time_); !i.end(update_time_);) {
			rigidbody_.addOrientTorque(ref CV.Vector3Back, 100f);
			rigidbody_.addForceZ(-80f);
			yield return null;
		}

		destroy();
	}

	private void zako2_update(float dt, float flow_speed)
	{
		if (MyCollider.getHitOpponentForEnemy(collider_) != MyCollider.Type.None) {
			rigidbody_.addTorque(MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f));
			life_ -= 20f;
		}
		if (lock_target_.isHitted()) {
			rigidbody_.addTorque(MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f));
			lock_target_.clearLock();
			life_ -= 100f;
		}
		if (life_ <= 0f && phase_ == Phase.Alive) {
			Explosion.Instance.spawn(ref lock_target_.updated_position_, update_time_);
			Hahen.Instance.spawn(ref lock_target_.updated_position_, update_time_);
			Shockwave.Instance.spawn(ref lock_target_.updated_position_, update_time_);
			SystemManager.Instance.registSound(DrawBuffer.SE.Explosion);
			MyCollider.disableForEnemy(collider_);
			lock_target_.disable();
			phase_ = Phase.Dying;
		}
		
		rigidbody_.update(dt);
		MyCollider.updateEnemy(collider_, ref rigidbody_.transform_.position_);
		lock_target_.update();
	}

	private void zako2_render_update(int front, ref DrawBuffer draw_buffer)
	{
		lock_target_.renderUpdate(front);
		draw_buffer.regist(ref rigidbody_.transform_, DrawBuffer.Type.Zako);
	}
}

} // namespace UTJ {
