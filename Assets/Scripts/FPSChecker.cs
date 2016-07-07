using UnityEngine;
using System.Collections;

public class FPSChecker : MonoBehaviour {

	private float prev_time_;
	private float fps_display_ = 0f;

	void Start () {
		prev_time_ = Time.time;
	}
	
	void Update () {
		float elapsed = Time.time - prev_time_;
		if (elapsed == 0f)
			return;
		prev_time_ = Time.time;
		float fps = 1f/elapsed;
		fps_display_ = Mathf.Lerp(fps_display_, fps, 0.01f);
		GetComponent<TextMesh>().text = string.Format("fps:{0}", fps_display_);
	}
}
