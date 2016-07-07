using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

class DragonNode {
	public RigidbodyTransform rigidbody_;
	public int collider_;
	public LockTarget lock_target_;
	
	private int idx_;
	private Vector3 locator_;
	private DrawBuffer.Type draw_type_; 

	private void calc_lock_position(ref Vector3 position)
	{
		position = rigidbody_.transform_.position_;
	}

	public void init(Task task,
					 int idx,
					 ref Vector3 position,
					 ref Quaternion rotation,
					 DrawBuffer.Type draw_type)
	{
		rigidbody_.init(ref position, ref rotation);
		rigidbody_.setRotateDamper(8);
		collider_ = MyCollider.createEnemy();
		MyCollider.initSphereEnemy(collider_, ref position, 1f /* radius */);
		idx_ = idx;
		lock_target_ = LockTarget.create(task, new LockTarget.CalcPosition(this.calc_lock_position));
		locator_ = (idx_ == 0 ?
					new Vector3(0f, 0f, 0f) :
					new Vector3(0f, 0f, -3f));
		draw_type_ = draw_type;
	}

	public void reset(ref MyTransform parent_transform)
	{
		var pos = parent_transform.transformPosition(ref locator_);
		rigidbody_.transform_.position_ = pos;
		rigidbody_.transform_.rotation_ = parent_transform.rotation_;
		rigidbody_.velocity_ = CV.Vector3Zero;
		rigidbody_.r_velocity_ = CV.Vector3Zero;
	}

	public void update(float dt, double update_time, ref MyTransform parent_transform)
	{
		if (MyCollider.getHitOpponentForEnemy(collider_) != MyCollider.Type.None) {
			rigidbody_.addTorque(MyRandom.Range(-20f, 20f),
								 MyRandom.Range(-20f, 20f),
								 MyRandom.Range(-20f, 20f));
		}
		if (lock_target_.isHitted()) {
			rigidbody_.addTorque(MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f),
								 MyRandom.Range(-200f, 200f));
			lock_target_.clearLock();
			Explosion.Instance.spawn(ref lock_target_.updated_position_, update_time);
			Hahen.Instance.spawn(ref lock_target_.updated_position_, update_time);
			SystemManager.Instance.registSound(DrawBuffer.SE.Explosion);
		}
		rigidbody_.addSpringTorque(ref parent_transform.rotation_, 30f /* torque_level */);
		var pos = parent_transform.transformPosition(ref locator_);
		rigidbody_.transform_.position_ = pos;
		rigidbody_.update(dt);
		MyCollider.updateEnemy(collider_, ref rigidbody_.transform_.position_);
		lock_target_.update();
	}

	public void renderUpdate(int front, ref DrawBuffer draw_buffer)
	{
		lock_target_.renderUpdate(front);
		draw_buffer.regist(ref rigidbody_.transform_, draw_type_);
	}

	public void addTubeTorque(float power)
	{
		var diff = new Vector3(-rigidbody_.transform_.position_.x,
							   -rigidbody_.transform_.position_.y,
							   0f);
		var torque = diff.normalized * power;
		rigidbody_.addTorque(ref torque);
	}
}

public class Dragon {
	public enum Mode {
		Attack,
		Chase,
		Farewell,
		LastAttack,
	}

	const int NODE_NUM = 8;
	private DragonNode[] nodes_;
	public Mode mode_;
	public LightBall lightball_;
	public bool is_charging_ = false;

	public void createPool()
	{
		nodes_ = new DragonNode[NODE_NUM];
		for (var i = 0; i < nodes_.Length; ++i) {
			nodes_[i] = new DragonNode();
		}		
	}

	public void init(Task task,
					 ref Vector3 position,
					 ref Quaternion rotation)
	{
		for (var i = 0; i < nodes_.Length; ++i) {
			var pos = position + rotation * new Vector3(0f, 0f, -3f*i);
			DrawBuffer.Type draw_type;
			if (i != nodes_.Length - 1) {
				draw_type = DrawBuffer.Type.DragonBody;
			} else {
				draw_type = DrawBuffer.Type.DragonTail;
			}
			nodes_[i].init(task, i, ref pos, ref rotation, draw_type);
		}
		mode_ = Mode.Attack;
		lightball_.init();
		is_charging_ = false;
	}

	public void reset(ref MyTransform head_transform)
	{
		for (var i = 0; i < nodes_.Length; ++i) {
			if (i == 0) {
				nodes_[i].reset(ref head_transform);
			} else {
				nodes_[i].reset(ref nodes_[i-1].rigidbody_.transform_);
			}
		}
	}

	public void update(float dt, double update_time, ref MyTransform head_transform)
	{
		for (var i = 0; i < nodes_.Length; ++i) {
			if (i == 0) {
				nodes_[i].update(dt, update_time, ref head_transform);
			} else {
				nodes_[i].update(dt, update_time, ref nodes_[i-1].rigidbody_.transform_);
			}
		}
		lightball_.update(dt);
	}

	public void renderUpdate(int front, ref MyTransform head_transform, ref DrawBuffer draw_buffer)
	{
		for (var i = 0; i < nodes_.Length; ++i) {
			nodes_[i].renderUpdate(front, ref draw_buffer);
		}
		if (is_charging_) {
			var offset = new Vector3(0, 0, 2.5f);
			lightball_.renderUpdate(front, ref head_transform, ref offset);
		}
	}

	public void addTubeTorque(float power)
	{
		for (var i = 0; i < nodes_.Length; ++i) {
			nodes_[i].addTubeTorque(power);
		}
	}
}

public partial class Enemy : Task
{
	private Dragon dragon_;
	private Vector3 lock_point_offset_ = new Vector3(0f, 0f, 2.5f);
	private Vector3 MUZZLE_OFFSET = new Vector3(0f, 0f, 2.5f);
	private Vector3 HEAD_OFFSET = new Vector3(0f, 0f, -3f);

	public void setMode(Dragon.Mode mode)
	{
		dragon_.mode_ = mode;
	}

	private void calc_lock_position_dragon(ref Vector3 position)
	{
		position = rigidbody_.transform_.transformPosition(ref lock_point_offset_);
	}

	public void dragon_init()
	{
		var position = new Vector3(-10f, -5f, 10f);
		rigidbody_.init(ref position, ref CV.QuaternionIdentity);
		rigidbody_.setDamper(2f);
		rigidbody_.setRotateDamper(20f);
		collider_ = MyCollider.createEnemy();
		MyCollider.initSphereEnemy(collider_, ref position, 1f /* radius */);
		lock_target_ = LockTarget.create(this, new LockTarget.CalcPosition(calc_lock_position_dragon));
		enumerator_ = dragon_act(); // この瞬間は実行されない
		on_update_ = new OnUpdateFunc(dragon_update);
		on_render_update_ = new OnRenderUpdateFunc(dragon_render_update);
		life_ = 10000000f;
		dragon_ = dragon_pool_;
		dragon_.init(this, ref rigidbody_.transform_.position_, ref rigidbody_.transform_.rotation_);
	}

	private void limit_target(ref Vector3 position, float limit)
	{
		float len = Mathf.Sqrt(position.x*position.x + position.y*position.y);
		if (len > limit) {
			float r = limit/len;
			position.x *= r;
			position.y *= r;
		}
	}

	private void reset(ref Vector3 position, ref Quaternion rotation)
	{
		rigidbody_.transform_.position_ = position;
		rigidbody_.velocity_ = CV.Vector3Zero;
		rigidbody_.transform_.rotation_ = rotation;
		rigidbody_.r_velocity_ = CV.Vector3Zero;
		dragon_.reset(ref rigidbody_.transform_);
	}

	public IEnumerator dragon_act()
	{
		double charge_start = update_time_;
		rigidbody_.setDamper(2f);
		while (dragon_.mode_ == Dragon.Mode.Attack) {
			// for (var i = new Utility.WaitForSeconds(30f, update_time_); !i.end(update_time_);) {
			{
				{
					var target = Player.Instance.rigidbody_.transform_.position_ + new Vector3(0f, 0f, 20f);
					limit_target(ref target, 4f);
					rigidbody_.addTargetTorque(ref target, 50f /* torque_level */, -1f /* max_level */);
					rigidbody_.addForwardForce(40f);
					rigidbody_.addLocalTorqueZ(60f);
				}
				if (update_time_ - charge_start < 4f) {
					dragon_.is_charging_ = true;
				} else {
					// dragon_.is_charging_ = false;
					double fired_time = 0;
					for (var w = new Utility.WaitForSeconds(4f, update_time_); !w.end(update_time_);) {
						var target = Player.Instance.rigidbody_.transform_.position_;
						rigidbody_.addTargetTorque(ref target, 50f /* torque_level */, -1f /* max_level */);
						if (update_time_ - fired_time > 0.2f) {
							var muzzle_pos = rigidbody_.transform_.transformPosition(ref MUZZLE_OFFSET);
							SystemManager.Instance.registSound(DrawBuffer.SE.Missile);
							EnemyBullet.create(ref muzzle_pos,
											   ref rigidbody_.transform_.rotation_,
											   50f /* speed */,
											   2f /* width */,
											   20f /* length */,
											   update_time_);
							fired_time = update_time_;
						}
						yield return null;
					}
					charge_start = update_time_;
				}
				yield return null;
			}

			yield return null;
		}

		for (var i = new Utility.WaitForSeconds(2f, update_time_); !i.end(update_time_);) {
			rigidbody_.addOrientTorque(ref CV.Vector3Forward, 50f /* torque_level */);
			rigidbody_.addLocalTorqueZ(60f);
			rigidbody_.addSpringForceZ(-9f, 0.5f /* ratio */);
			yield return null;
		}
		rigidbody_.setDamper(8f);
		{
			double back_and_force_time = update_time_ + 5f;
			double fire_time = update_time_ + 1f;
			while (dragon_.mode_ == Dragon.Mode.Chase) {
				rigidbody_.addOrientTorque(ref CV.Vector3Forward, 50f /* torque_level */);
				float phase = Mathf.Repeat((float)update_time_*1f, Mathf.PI*2f);
				rigidbody_.addLocalTorqueZ(-7f);
				rigidbody_.addSpringForceXY(Mathf.Cos(phase) * 8f,
											Mathf.Sin(phase) * 8f,
											8f /* ratio */);
				float target_z = -5f;
				if (update_time_ - back_and_force_time > 0f) {
					float p = -40f + Mathf.PerlinNoise(Mathf.Repeat((float)update_time_*0.1f, 1f), 0f) * 120f;
					target_z += p;
				}
				rigidbody_.addSpringForceZ(target_z, 20f /* ratio */);
				if (update_time_ - fire_time > 0f) {
					if (Player.Instance.rigidbody_.transform_.position_.z >
						rigidbody_.transform_.position_.z) {
						SystemManager.Instance.registSound(DrawBuffer.SE.Missile);
						var muzzle_pos = rigidbody_.transform_.transformPosition(ref MUZZLE_OFFSET);
						EnemyBullet.create(ref muzzle_pos,
										   ref CV.QuaternionIdentity,
										   50f /* speed */,
										   2f /* width */,
										   20f /* length */,
										   update_time_);
					}
					fire_time = update_time_ + 0.2f + Mathf.PerlinNoise(Mathf.Repeat((float)update_time_, 1f), 0f) * 0.5f;
				}

				dragon_.addTubeTorque(1f /* power */);

				yield return null;
			}
		}
		while (dragon_.mode_ == Dragon.Mode.Farewell) {
			rigidbody_.addForceZ(-100f);
			yield return null;
		}
		{
			for (var w = new Utility.WaitForSeconds(2f, update_time_); !w.end(update_time_);) { yield return null; }
			var pos = Player.Instance.rigidbody_.transform_.position_ + new Vector3(0f, 20f, 10f);
			var rot = Quaternion.Euler(90f, 0f, 0f);
			reset(ref pos, ref rot);
			yield return null;
			for (var w = new Utility.WaitForSeconds(0.5f, update_time_); !w.end(update_time_);) {
				rigidbody_.addForwardForce(100f /* force */);
				rigidbody_.addLocalTorqueZ(10f);
				yield return null;
			}
			SystemManager.Instance.registBgm(DrawBuffer.BGM.Stop);
			SystemManager.Instance.setBulletTime(true);
			for (var w = new Utility.WaitForSeconds(0.5f, update_time_); !w.end(update_time_);) {
				rigidbody_.addForwardForce(100f /* force */);
				rigidbody_.addLocalTorqueZ(10f);
				yield return null;
			}
			for (var w = new Utility.WaitForSeconds(3f, update_time_); !w.end(update_time_);) {
				var target = new Vector3(0f, 0f, 15f + Player.Instance.rigidbody_.transform_.position_.z);
				rigidbody_.addTargetTorque(ref target, 25f /* torque_level */, -1f /* max_level */);
				rigidbody_.addLocalTorqueZ(-40f);
				rigidbody_.addForwardForce(200f /* force */);
				yield return null;
			}
			SystemManager.Instance.setBulletTime(false);
			for (var w = new Utility.WaitForSeconds(30f, update_time_); !w.end(update_time_);) {
				rigidbody_.addForwardForce(100f /* force */);
				rigidbody_.addForceZ(-300f /* force */);
				yield return null;
			}
		}
		for (;;) {
			yield return null;
		}
	}

	private void dragon_update(float dt, float flow_speed)
	{
		if (MyCollider.getHitOpponentForEnemy(collider_) != MyCollider.Type.None) {
			rigidbody_.addTorque(MyRandom.Range(-20f, 20f),
								 MyRandom.Range(-20f, 20f),
								 MyRandom.Range(-20f, 20f));
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
			SystemManager.Instance.registSound(DrawBuffer.SE.Explosion);
			MyCollider.disableForEnemy(collider_);
			lock_target_.disable();
			phase_ = Phase.Dying;
		}
		
		rigidbody_.update(dt);
		MyCollider.updateEnemy(collider_, ref rigidbody_.transform_.position_);
		lock_target_.update();
		var head_transform = rigidbody_.transform_.add(ref HEAD_OFFSET);
		dragon_.update(dt, update_time_, ref head_transform);
	}

	private void dragon_render_update(int front, ref DrawBuffer draw_buffer)
	{
		lock_target_.renderUpdate(front);
		draw_buffer.regist(ref rigidbody_.transform_, DrawBuffer.Type.DragonHead);
		dragon_.renderUpdate(front, ref rigidbody_.transform_, ref draw_buffer);
	}

}

} // namespace UTJ {
