using System.Collections;
using UnityEngine;

namespace UTJ {

public class HUD {

	// singleton
	static HUD instance_;
	public static HUD Instance { get { return instance_ ?? (instance_ = new HUD()); } }

 	public void init()
	{
	}

	public void setDamagePoint(ref Vector3 position)
	{
	}

 	public void update(float dt, double update_time)
	{
	}

 	public void renderUpdate(int front)
	{
	}
}

} // namespace UTJ {
