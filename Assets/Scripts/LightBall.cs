using UnityEngine;
using System.Collections.Generic;

namespace UTJ {

public struct LightBall
{
	const int LIGHTBALL_MAX = 16;

	private float[] phase_list_;
	private Vector3[] normal_list_;
	private int beam_id_;
	private int[] beam_id_list_;

	public void init()
	{
		normal_list_ = new Vector3[LIGHTBALL_MAX];
		for (var i = 0; i < normal_list_.Length; ++i) {
			float x = MyRandom.Range(-1f, 1f);
			float y = MyRandom.Range(-1f, 1f);
			float z = MyRandom.Range(0.75f, 1f);
			float len2 = x*x + y*y + z*z;
			float rlen = 1.0f / Mathf.Sqrt(len2);
			normal_list_[i].x = x * rlen;
			normal_list_[i].y = y * rlen;
			normal_list_[i].z = z * rlen;
		}
		phase_list_ = new float[LIGHTBALL_MAX];
		for (var i = 0; i < phase_list_.Length; ++i) {
			phase_list_[i] = MyRandom.Range(0f, 1f);
		}

		beam_id_ = Beam.Instance.spawn(0.8f /* width */, Beam.Type.LightBall);
		beam_id_list_ = new int[LIGHTBALL_MAX];
		for (var i = 0; i < beam_id_list_.Length; ++i) {
			beam_id_list_[i] = Beam.Instance.spawn(0.1f /* width */, Beam.Type.LightBall);
		}
	}

	public void destroy()
	{
		Beam.Instance.destroy(beam_id_);
		beam_id_ = -1;
		for (var i = 0; i < beam_id_list_.Length; ++i) {
			Beam.Instance.destroy(beam_id_list_[i]);
			beam_id_list_[i] = -1;
		}
	}

	public void update(float dt)
	{
		for (var i = 0; i < phase_list_.Length; ++i) {
			phase_list_[i] += (-2f * dt);
			phase_list_[i] = Mathf.Repeat(phase_list_[i], 1f);
		}
	}

	public void renderUpdate(int front, ref MyTransform transform, ref Vector3 offset)
	{
		{
			var pos = transform.transformPosition(ref offset);
			Beam.Instance.renderUpdate(front,
									   beam_id_,
									   ref pos,
									   ref pos);
		}
		for (var i = 0; i < beam_id_list_.Length; ++i) {
			var dir0 = offset + normal_list_[i] * phase_list_[i] * 2f;
			var pos0 = transform.transformPosition(ref dir0);
			var dir1 = offset + normal_list_[i] * (phase_list_[i] + 0.1f) * 2f;
			var pos1 = transform.transformPosition(ref dir1);
			Beam.Instance.renderUpdate(front,
									   beam_id_list_[i],
									   ref pos0,
									   ref pos1);
		}
	}

}

} // namespace UTJ {
