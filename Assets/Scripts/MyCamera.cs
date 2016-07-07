using UnityEngine;

namespace UTJ {

public class MyCamera
{
	const float SCREEN_WIDTH2 = 480f;
	const float SCREEN_HEIGHT2 = 272;
	private Matrix4x4 screen_matrix_;
	private RigidbodyTransform rigidbody_;

	public static MyCamera create()
	{
		var camera = new MyCamera();
		camera.init();
		return camera;
	}

	private void init()
	{
		rigidbody_.init();
		rigidbody_.transform_.position_ = new Vector3(0, 0, -8);
	}

	public Vector3 getScreenPoint(ref Vector3 world_position)
	{
		var v = screen_matrix_.MultiplyPoint(world_position);
		return new Vector3(v.x * (-SCREEN_WIDTH2), v.y * (-SCREEN_HEIGHT2), v.z);
	}

	public void renderUpdate(int front, ref DrawBuffer draw_buffer)
	{
		draw_buffer.registCamera(ref rigidbody_.transform_);
	}
}

} // namespace UTJ {
