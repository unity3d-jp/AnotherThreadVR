using UnityEngine;
using System.Collections;

namespace UTJ {

class CV {
	public static Vector3 Vector3Zero = Vector3.zero;
	public static Vector3 Vector3One = Vector3.one;
	public static Vector3 Vector3Forward = Vector3.forward;
	public static Vector3 Vector3Back = Vector3.back;
	public static Vector3 Vector3Left = Vector3.left;
	public static Vector3 Vector3Right = Vector3.right;
	public static Vector3 Vector3Up = Vector3.up;
	public static Vector3 Vector3Down = Vector3.down;
	public static Quaternion QuaternionIdentity = Quaternion.identity;
	public static Quaternion Quaternion180Y = Quaternion.Euler(0f, 180f, 0f);
}

} // namespace UTJ {
