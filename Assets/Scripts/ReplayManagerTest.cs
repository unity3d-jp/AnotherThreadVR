using UnityEngine;
using System.Collections;

namespace UTJ {

public class ReplayManagerTest : MonoBehaviour {

	ReplayManager replay_manager_ = new ReplayManager();

	IEnumerator loop()
	{
		replay_manager_.startRecording((double)Time.time);
		for (var i = 0; i < 7; ++i) {
			MyTransform tfm = new MyTransform();
			tfm.position_ = new Vector3(i, 0f, 0f);
			try {
				replay_manager_.update((double)i, ref tfm, false /* fired_bullet */, false /* fired_missile */);
			} catch(System.Exception e) {
				Debug.LogError(e);
			}
		}
		replay_manager_.stopRecording();

		yield return null;

		replay_manager_.startPlaying((double)Time.time, null /* player */);
		for (var i = 0; i < 24; ++i) {
			MyTransform tfm = new MyTransform();
			try {
				replay_manager_.getFrameData((double)(i-2) * 0.33333, ref tfm);
			} catch(System.Exception e) {
				Debug.LogError(e);
			}
			Debug.LogFormat("{0}:pos_x:{1}", (double)(i-2)*0.33333, tfm.position_.x);
		}
		replay_manager_.stopPlaying(null /* player */);
		yield return null;
	}

	void Start()
	{
		StartCoroutine(loop());
	}
}

} // namespace UTJ {
