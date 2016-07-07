using UnityEngine;

namespace UTJ {

public abstract class Task
{
	public bool alive_;
	public abstract void update(float dt, double update_time, float flow_speed);
	public abstract void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer);

	public virtual void init()
	{
		alive_ = true;
		TaskManager.Instance.add(this);
	}

	public virtual void destroy()
	{
		TaskManager.Instance.remove(this);
		alive_ = false;
	}
}

} // namespace UTJ {
