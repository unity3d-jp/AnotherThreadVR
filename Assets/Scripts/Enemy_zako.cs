using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public partial class Enemy : Task
{
	public void zako_init(ref Vector3 position, ref Quaternion rotation)
	{
		rigidbody_.init(ref position, ref rotation);
		collider_ = MyCollider.createEnemy();
		MyCollider.initSphereEnemy(collider_, ref position, 1f /* radius */);
		lock_target_ = LockTarget.create(this, new LockTarget.CalcPosition(calc_lock_position_center));
		enumerator_ = zako_act(); // この瞬間は実行されない
		on_update_ = new OnUpdateFunc(zako_update);
		on_render_update_ = new OnRenderUpdateFunc(zako_render_update);
		life_ = 1f;
	}

	public IEnumerator zako_act()
	{
		rigidbody_.setDamper(2f);
		rigidbody_.setRotateDamper(2f);
		target_position_.x = MyRandom.Range(-5f, 5f);
		target_position_.y = MyRandom.Range(-5f, 5f);
		rigidbody_.addForceZ(40f);
		yield return null;
		for (var i = new Utility.WaitForSeconds(10f, update_time_); !i.end(update_time_);) {
			rigidbody_.addForceX(target_position_.x - rigidbody_.transform_.position_.x * 2f);
			rigidbody_.addForceY(target_position_.y - rigidbody_.transform_.position_.y * 2f);
			rigidbody_.addForceZ(50f);
			rigidbody_.addTorqueZ(-rigidbody_.velocity_.x * 1f);

			if (MyRandom.ProbabilityForSecond(1.5f, SystemManager.Instance.getDT())) {
				var pos = Player.Instance.rigidbody_.transform_.position_;
				pos.z += MyRandom.Range(-10f, 10f);
				EnemyBullet.create(ref rigidbody_.transform_.position_,
								   ref pos,
								   50f /* speed */,
								   update_time_);
			}

			if (phase_ == Phase.Dying) {
				// for (var j = Utility.WaitForSeconds(0.1f, update_time_); j.MoveNext();) {
				for (var j = new Utility.WaitForSeconds(0.1f, update_time_); j.end(update_time_);) {
					yield return null;
				}
				break;
			}

			yield return null;
		}
		destroy();
	}

	private void zako_update(float dt, float flow_speed)
	{
		if (MyCollider.getHitOpponentForEnemy(collider_) != MyCollider.Type.None) {
			rigidbody_.addTorque(MyRandom.Range(-100f, 100f),
								 MyRandom.Range(-100f, 100f),
								 MyRandom.Range(-100f, 100f));
			life_ -= 20f;
		}
		if (lock_target_.isHitted()) {
			rigidbody_.addTorque(MyRandom.Range(-100f, 100f),
								 MyRandom.Range(-100f, 100f),
								 MyRandom.Range(-100f, 100f));
			rigidbody_.addForceZ(-10000f);
			life_ -= 100f;
		}
		if (life_ <= 0f && phase_ == Phase.Alive) {
			Explosion.Instance.spawn(ref lock_target_.updated_position_, update_time_);
			Hahen.Instance.spawn(ref lock_target_.updated_position_, update_time_);
			SystemManager.Instance.registSound(DrawBuffer.SE.Explosion);
			MyCollider.disableForEnemy(collider_);
			lock_target_.disable();
			phase_ = Phase.Dying;
		}
		
		rigidbody_.update(dt);
		MyCollider.updateEnemy(collider_, ref rigidbody_.transform_.position_);
		lock_target_.update();
	}

	private void zako_render_update(int front, ref DrawBuffer draw_buffer)
	{
		lock_target_.renderUpdate(front);
		draw_buffer.regist(ref rigidbody_.transform_, DrawBuffer.Type.Zako);
	}
}

} // namespace UTJ {
