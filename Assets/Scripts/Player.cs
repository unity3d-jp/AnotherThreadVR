using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

// public class Player : Task
public class Player
{
	private static Player instance_;
	public static Player Instance { get { return instance_; } }

	public const float PLAYER_WIDTH2 = 0.5f;
	public static readonly Vector3 AIMING_OFFSET = new Vector3(0f, 0.5f, 0f);
	private const float PLAYER_Z = 5f;

	public RigidbodyTransform rigidbody_;
	private int collider_;

	private Vector3 l_bullet_locator_ = new Vector3(-0.35f, 0.1f, 4f);
	private Vector3 r_bullet_locator_ = new Vector3( 0.35f, 0.1f, 4f);
	private double fire_time_;
	private double can_fire_time_;
	private bool now_locking_ = false;
	private bool prev_fire_button_ = false;

	private int l_trail_;
	private int r_trail_;
	private int t_trail_;
	private Vector3 l_trail_locator_ = new Vector3(-0.4f, 0f, -1f);
	private Vector3 r_trail_locator_ = new Vector3( 0.4f, 0f, -1f);
	private float arm_offset_;

	private enum Phase {
		Title,
		Start,
		Battle,
	}
	private Phase phase_ = Phase.Title;

	public void setPhaseTitle() { phase_ = Phase.Title; }
	public void setPhaseStart() { phase_ = Phase.Start; }
	public void setPhaseBattle() { phase_ = Phase.Battle; }

	public bool isNowLocking() { return now_locking_; }

	public static Player create()
	{
		var player = new Player();
		Debug.Assert(instance_ == null);
		instance_ = player;
		player.init();
		return player;
	}

	private void init()
	{
		rigidbody_.init();
		rigidbody_.transform_.position_.z = PLAYER_Z;
		rigidbody_.setDamper(16f);
		rigidbody_.setRotateDamper(32f);
		collider_ = MyCollider.createPlayer();
		MyCollider.initSpherePlayer(collider_, ref rigidbody_.transform_.position_,
									2f /* radius */);
		fire_time_ = 0f;
		can_fire_time_ = 0f;

		float width = 0.3f;
		var lpos = rigidbody_.transform_.transformPosition(ref l_trail_locator_);
		l_trail_ = Trail.Instance.spawn(ref lpos, width, Trail.Type.Player);
		var rpos = rigidbody_.transform_.transformPosition(ref r_trail_locator_);
		r_trail_ = Trail.Instance.spawn(ref rpos, width, Trail.Type.Player);
	}

	private int getButton(InputManager.Button button)
	{
		return InputManager.Instance.getButton(button);
	}

	public void destroy()
	{
		Trail.Instance.destroy(l_trail_);
		l_trail_ = -1;
		Trail.Instance.destroy(r_trail_);
		r_trail_ = -1;
		MyCollider.destroyPlayer(collider_);
		collider_ = -1;
	}

	public void setPositionXY(float x, float y)
	{
		rigidbody_.setPosition(x, y, PLAYER_Z);
		var lpos = rigidbody_.transform_.transformPosition(ref l_trail_locator_);
		Trail.Instance.resetPosition(l_trail_, ref lpos);
		var rpos = rigidbody_.transform_.transformPosition(ref r_trail_locator_);
		Trail.Instance.resetPosition(r_trail_, ref rpos);
	}

	public void update(float dt, double update_time, float flow_speed)
	{
		update_posture(dt);
		switch (phase_) {
			case Phase.Title:
				rigidbody_.setDamper(2f);
				float valx = Mathf.PerlinNoise((float)update_time*0.2f, 0f) - 0.5f;
				float valy = Mathf.PerlinNoise((float)update_time*0.3f, 0.5f) - 0.5f;
				rigidbody_.addForceX(valx*2f);
				rigidbody_.addForceY(valy*2.5f);
				rigidbody_.addSpringForceXY(0f, -27f, 4f);
				rigidbody_.update(dt);
				break;

			case Phase.Start:
				update_attack(update_time);
				rigidbody_.setDamper(2f);
				rigidbody_.addForceY(10f);
				rigidbody_.addSpringForceX(0f, 4f);
				rigidbody_.update(dt);
				break;

			case Phase.Battle:
				update_attack(update_time);
				rigidbody_.setDamper(16f);
				update_battle(dt, update_time);
				break;
		}
		// trail
	    {
			var lpos = rigidbody_.transform_.transformPosition(ref l_trail_locator_);
			Trail.Instance.update(l_trail_, ref lpos, dt, flow_speed, update_time);
			var rpos = rigidbody_.transform_.transformPosition(ref r_trail_locator_);
			Trail.Instance.update(r_trail_, ref rpos, dt, flow_speed, update_time);
		}
	}

	private void update_posture(float dt)
	{
		int hori = getButton(InputManager.Button.Horizontal);
		int vert = getButton(InputManager.Button.Vertical);

		rigidbody_.addTorqueZ(-(float)hori * 0.002f);
		rigidbody_.addTorqueY((float)hori * 0.01f);
		rigidbody_.addTorqueX(-(float)vert * 0.01f);

	    {
			var v0 = rigidbody_.transform_.rotation_ * CV.Vector3Forward;
			var v1 = Vector3.Cross(v0, CV.Vector3Forward) * 100f;
			rigidbody_.addTorque(ref v1);
		}
	    {
			var v0 = rigidbody_.transform_.rotation_ * CV.Vector3Left;
			var v1 = Vector3.Cross(v0, CV.Vector3Left) * 10f;
			rigidbody_.addTorque(ref v1);
		}
	    {
			var v0 = rigidbody_.transform_.rotation_ * CV.Vector3Right;
			var v1 = Vector3.Cross(v0, CV.Vector3Right) * 10f;
			rigidbody_.addTorque(ref v1);
		}

		arm_offset_ -= 8f * dt;
		arm_offset_ = Mathf.Clamp(arm_offset_, 0f, 1f);
	}

	private void update_attack(double update_time)
	{
		bool fire_button = getButton(InputManager.Button.Fire) > 0;
		bool fire_button_released = (!fire_button && prev_fire_button_);
		prev_fire_button_ = fire_button;

		// fire bullets
		if (fire_button) {
			if (can_fire_time_ - update_time > 0f && update_time - fire_time_ > 0f) {
				var lpos = rigidbody_.transform_.transformPosition(ref l_bullet_locator_);
				Bullet.create(ref lpos, ref rigidbody_.transform_.rotation_, 120f /* speed */, update_time);
				var rpos = rigidbody_.transform_.transformPosition(ref r_bullet_locator_);
				Bullet.create(ref rpos, ref rigidbody_.transform_.rotation_, 120f /* speed */, update_time);
				SystemManager.Instance.registSound(DrawBuffer.SE.Bullet);
				fire_time_ = update_time + 0.08f;
				arm_offset_ = 1f;
			}
		} else {
			can_fire_time_ = update_time + 2f;
		}

		// lockonrange display
		LockonRange.Instance.setOn(fire_button);
		now_locking_ = fire_button;

		// fire missiles
		if (fire_button_released) {
			bool fired = LockTarget.fireMissiles(this);
			if (fired) {
				SystemManager.Instance.registSound(DrawBuffer.SE.Missile);
			}
		}

	}

	private void update_battle(float dt, double update_time)
	{
	    {
			var move_vector = rigidbody_.transform_.rotation_ * CV.Vector3Forward;
			const float FORCE = 500f;
			rigidbody_.addForceX(move_vector.x * FORCE);
			rigidbody_.addForceY(move_vector.y * FORCE);
		}

	    {
			rigidbody_.solveForXYTube(Tube.RADIUS, dt);
			rigidbody_.update(dt);
		}

		// shield
		{
			Vector3 intersect_point = new Vector3(0f, 0f, 0f);
			if (MyCollider.getHitOpponentForPlayer(collider_, ref intersect_point) != MyCollider.Type.None) {
				SystemManager.Instance.registSound(DrawBuffer.SE.Shield);
				Shield.Instance.spawn(ref intersect_point,
									  ref rigidbody_.transform_.position_,
									  update_time,
									  Shield.Type.Green);
			}
		}

		// collider
		MyCollider.updatePlayer(collider_, ref rigidbody_.transform_.position_);
	}

	public void renderUpdate(int front, ref DrawBuffer draw_buffer)
	{
		// trail
		if (l_trail_ >= 0) {
			Trail.Instance.renderUpdate(front, l_trail_);
		}
		if (r_trail_ >= 0) {
			Trail.Instance.renderUpdate(front, r_trail_);
		}
					   
		var arm_offset = new Vector3(0f, 0f, -arm_offset_ * 0.1f);
		draw_buffer.registPlayer(ref rigidbody_.transform_, ref arm_offset);
	}
}

} // namespace UTJ {
