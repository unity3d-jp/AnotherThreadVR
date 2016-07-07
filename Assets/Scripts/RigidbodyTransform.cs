using UnityEngine;

namespace UTJ {

public struct RigidbodyTransform
{
	public MyTransform transform_;
	public Vector3 velocity_;
	private Vector3 acceleration_;
	public float damper_;
	public Vector3 r_velocity_;
	public Vector3 r_acceleration_;
	public float r_damper_;

	public void init()
	{
		init(ref CV.Vector3Zero, ref CV.QuaternionIdentity); 
	}
	public void init(ref Vector3 position, ref Quaternion rotation)
	{
		transform_.init(ref position, ref rotation);
		velocity_ = CV.Vector3Zero;
		acceleration_ = CV.Vector3Zero;
		damper_ = 0f;
		r_velocity_ = CV.Vector3Zero;
		r_acceleration_ = CV.Vector3Zero;
		r_damper_ = 0f;
	}

	public void setPosition(float x, float y, float z)
	{
		transform_.position_.x = x;
		transform_.position_.y = y;
		transform_.position_.z = z;
		velocity_ = CV.Vector3Zero;
		acceleration_ = CV.Vector3Zero;
		r_velocity_ = CV.Vector3Zero;
		r_acceleration_ = CV.Vector3Zero;
	}

	public void setDamper(float damper)
	{
		damper_ = damper;
	}

	public void setRotateDamper(float damper)
	{
		r_damper_ = damper;
	}

	public void addForce(ref Vector3 v)
	{
		acceleration_.x += v.x;
		acceleration_.y += v.y;
		acceleration_.z += v.z;
	}

	public void addForceX(float v)
	{
		acceleration_.x += v;
	}
	public void addForceY(float v)
	{
		acceleration_.y += v;
	}
	public void addForceZ(float v)
	{
		acceleration_.z += v;
	}
	public void addForceXY(float x, float y)
	{
		acceleration_.x += x;
		acceleration_.y += y;
	}

	public void addForwardForce(float power)
	{
		var force = transform_.rotation_ * new Vector3(0f, 0f, power);
		addForce(ref force);
	}

	public void addForwardForce(ref Vector3 power)
	{
		var force = transform_.rotation_ * power;
		addForce(ref force);
	}

	public void setAcceleration(ref Vector3 a)
	{
		acceleration_ = a;
	}

	public void setVelocity(ref Vector3 v)
	{
		velocity_ = v;
	}
	public void setVelocity(float x, float y, float z)
	{
		velocity_.x = x;
		velocity_.y = y;
		velocity_.z = z;
	}
	public void setVelocity(Vector3 velocity)
	{
		velocity_ = velocity;
	}

	public void addTorque(ref Vector3 torque)
	{
		r_acceleration_.x += torque.x;
		r_acceleration_.y += torque.y;
		r_acceleration_.z += torque.z;
	}

	public void addTorque(float x, float y, float z)
	{
		r_acceleration_.x += x;
		r_acceleration_.y += y;
		r_acceleration_.z += z;
	}

	public void addTorqueX(float torque)
	{
		r_acceleration_.x += torque;
	}

	public void addTorqueY(float torque)
	{
		r_acceleration_.y += torque;
	}

	public void addTorqueZ(float torque)
	{
		r_acceleration_.z += torque;
	}

	public void addLocalTorqueZ(float torque)
	{
		var t = transform_.rotation_ * new Vector3(0f, 0f, torque);
		r_acceleration_ += t;
	}

	public void solveForXYTube(float radius, float dt)
	{
		float predicted_x = transform_.position_.x + ((velocity_.x + (acceleration_.x * dt)) * dt);
		float predicted_y = transform_.position_.y + ((velocity_.y + (acceleration_.y * dt)) * dt);
		float predicted_rad2 = predicted_x * predicted_x + predicted_y * predicted_y;
		if (predicted_rad2 < radius * radius)
			return;

		// adjust
		float rrad = radius / Mathf.Sqrt(predicted_rad2);
		predicted_x *= rrad;
		predicted_y *= rrad;
		velocity_.x = (predicted_x - transform_.position_.x)/dt;
		velocity_.y = (predicted_y - transform_.position_.y)/dt;

		// tangent
		float tangent_x = -transform_.position_.y;
		float tangent_y = transform_.position_.x;
		float rlen = 1.0f / Mathf.Sqrt(tangent_x * tangent_x + tangent_y * tangent_y);
		tangent_x *= rlen;
		tangent_y *= rlen;

		float dot = tangent_x * acceleration_.x + tangent_y * acceleration_.y;
		acceleration_.x = tangent_x * dot;
		acceleration_.y = tangent_y * dot;
	}

	public void solveForGround(float ground, float dt)
	{
		float predicted_y = transform_.position_.y + ((velocity_.y + (acceleration_.y * dt)) * dt);
		if (predicted_y > ground)
			return;
		velocity_.y = (ground - transform_.position_.y)/dt;
		acceleration_.y = 0f;
	}

	public void update(float dt)
	{
		// apply dampler
		acceleration_.x -= velocity_.x * damper_;
		acceleration_.y -= velocity_.y * damper_;
		acceleration_.z -= velocity_.z * damper_;
		// update velocity
		velocity_.x += acceleration_.x * dt;
		velocity_.y += acceleration_.y * dt;
		velocity_.z += acceleration_.z * dt;
		acceleration_.x = acceleration_.y = acceleration_.z = 0f; // clear acceleration
		// update position
		transform_.position_.x += velocity_.x * dt;
		transform_.position_.y += velocity_.y * dt;
		transform_.position_.z += velocity_.z * dt;

		/*
		 * for rotation
		 */
		// apply dampler
		r_acceleration_.x -= r_velocity_.x * r_damper_;
		r_acceleration_.y -= r_velocity_.y * r_damper_;
		r_acceleration_.z -= r_velocity_.z * r_damper_;
		// update velocity
		r_velocity_.x += r_acceleration_.x * dt;
		r_velocity_.y += r_acceleration_.y * dt;
		r_velocity_.z += r_acceleration_.z * dt;
		r_acceleration_.x = r_acceleration_.y = r_acceleration_.z = 0f; // clear acceleration
		// update rotation
		var nx = r_velocity_.x * dt;
		var ny = r_velocity_.y * dt;
		var nz = r_velocity_.z * dt;
		var len2 = nx*nx + ny*ny + nz*nz; // sin^2
		var w = Mathf.Sqrt(1f - len2); // (sin^2 + cos^2) = 1
		var q = new Quaternion(nx, ny, nz, w);
		transform_.rotation_ = q * transform_.rotation_;
		// normalize
		var v2 = transform_.rotation_.x * transform_.rotation_.x;
		v2 += transform_.rotation_.y * transform_.rotation_.y;
		v2 += transform_.rotation_.z * transform_.rotation_.z;
		v2 += transform_.rotation_.w * transform_.rotation_.w;
		float inv = 1.0f / Mathf.Sqrt(v2);
		transform_.rotation_.x *= inv;
		transform_.rotation_.y *= inv;
		transform_.rotation_.z *= inv;
		transform_.rotation_.w *= inv;
	}

	public void cancelUpdateForTube(float dt)
	{
		// cancel
		transform_.position_.x -= velocity_.x * dt;
		transform_.position_.y -= velocity_.y * dt;
		transform_.position_.z -= velocity_.z * dt;
		// recalculate
		float len2 = (transform_.position_.x * transform_.position_.x +
					  transform_.position_.y * transform_.position_.y);
		float len = Mathf.Sqrt(len2);
		float rlen = 1f / len;
		float dx = -transform_.position_.y * rlen;
		float dy = transform_.position_.x * rlen;
		float norm = dx * velocity_.x + dy * velocity_.y;
		velocity_.x = dx * norm;
		velocity_.y = dy * norm;
		transform_.position_.x += velocity_.x * dt;
		transform_.position_.y += velocity_.y * dt;
		float PLAYER_WIDTH2 = 1f;
		float offset = rlen * (Tube.RADIUS - PLAYER_WIDTH2);
		transform_.position_.x *= offset;
		transform_.position_.y *= offset;
	}

	public void addSpringForceX(float target_x, float ratio)
	{
		addForceX((target_x - transform_.position_.x) * ratio);
	}

	public void addSpringForceZ(float target_z, float ratio)
	{
		addForceZ((target_z - transform_.position_.z) * ratio);
	}

	public void addSpringForceXY(float target_x, float target_y, float ratio)
	{
		addForceX((target_x - transform_.position_.x) * ratio);
		addForceY((target_y - transform_.position_.y) * ratio);
	}

	public void addTargetTorque(ref Vector3 target, float torque_level)
	{
		addTargetTorque(ref target, torque_level, -1f /* max_level */);
	}
	public void addTargetTorque(ref Vector3 target, float torque_level, float max_level)
	{
		var diff = target - transform_.position_;
		diff.Normalize();
		addOrientTorque(ref diff, torque_level, max_level);
	}

	public void addOrientTorque(ref Vector3 dir, float torque_level)
	{
		addOrientTorque(ref dir, torque_level, -1f /* max level */);
	}
	public void addOrientTorque(ref Vector3 dir, float torque_level, float max_level)
	{
		var forward = transform_.rotation_ * new Vector3(0f, 0f, 1f);
		var torque = Vector3.Cross(forward, dir);
		torque.x *= torque_level;
		torque.y *= torque_level;
		torque.z *= torque_level;
		if (max_level > 0f) {
			var level = torque.magnitude;
			if (max_level < level) {
				float r = max_level/level;
				torque *= r;
			}
		}
		addTorque(ref torque);
	}

	public void addSpringTorque(ref Quaternion target, float torque_level)
	{
		var q = target * Quaternion.Inverse(transform_.rotation_);
		var torque = new Vector3(q.x, q.y, q.z) * torque_level;
		addTorque(ref torque);
	}
}

} // namespace UTJ {
