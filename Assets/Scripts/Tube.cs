using UnityEngine;
using System.Collections;

namespace UTJ {

public class Tube : MonoBehaviour
{
	public const float RADIUS = 8f;
	public const float RADIUS_SQR = RADIUS * RADIUS;
	public static float GetRadiusSqr(float x, float y)
	{
		float phase = x == 0f ? 0f : Mathf.Atan(y / x); 
		phase += Mathf.PI * 0.125f;
		phase = Mathf.Repeat(phase, 2 * Mathf.PI);
		phase = phase * 4f / Mathf.PI;
		bool is_deep = ((int)phase) % 2 == 1;
		return is_deep ? Tube.RADIUS_SQR*4f : Tube.RADIUS_SQR;
	}
}

} // namespace UTJ {
