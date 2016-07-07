using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public class Notice : Task
{
	const int POOL_MAX = 4;
	private static Notice[] pool_;
	private static int pool_index_;
	
	public static void createPool()
	{
		pool_ = new Notice[POOL_MAX];
		for (var i = 0; i < POOL_MAX; ++i) {
			var obj = new Notice();
			obj.alive_ = false;
			pool_[i] = obj;
		}
		pool_index_ = 0;
	}

	public static Notice create(float x, float y,
								MySprite.Kind kind,
								MySprite.Type type,
								bool blink)
	{
		return create(x, y, -1f /* disappear_time */, kind, type, blink);
	}
	public static Notice create(float x, float y,
								double disappear_time,
								MySprite.Kind kind,
								MySprite.Type type,
								bool blink)
	{
		int cnt = 0;
		while (pool_[pool_index_].alive_) {
			++pool_index_;
			if (pool_index_ >= POOL_MAX)
				pool_index_ = 0;
			++cnt;
			if (cnt >= POOL_MAX) {
				Debug.LogError("EXCEED Notice POOL!");
				break;
			}
		}
		var obj = pool_[pool_index_];
		obj.init(x, y, disappear_time, kind, type, blink);
		return obj;
	}


	private float x_;
	private float y_;
	private MySprite.Kind kind_;
	private MySprite.Type type_;
	private double disappear_time_;
	private bool blink_;
	private double blink_time_;
	private int blink_rest_;
	private bool display_;

	private void init(float x, float y,
					  double disappear_time,
					  MySprite.Kind kind,
					  MySprite.Type type,
					  bool blink)
	{
		base.init();
		x_ = x;
		y_ = y;
		disappear_time_ = disappear_time;
		kind_ = kind;
		type_ = type;
		blink_ = blink;
		blink_time_ = 0f;
		blink_rest_ = 10;
		display_ = true;
	}

	public override void destroy()
	{
		base.destroy();
	}

	public override void update(float dt, double update_time, float flow_speed)
	{
		if (disappear_time_ >= 0 && update_time > disappear_time_) {
			destroy();
			return;
		}

		if (blink_ && blink_rest_ > 0) {
			if (blink_time_ < update_time) {
				display_ = !display_;
				blink_time_ = update_time + 0.15f;
				--blink_rest_;
			}
		}
	}

	public override void renderUpdate(int front, MyCamera camera, ref DrawBuffer draw_buffer)
	{
		if (display_)
			MySprite.Instance.put(front, x_, y_, 1f /* magnify_ratio */, kind_, type_);
	}
}

} // namespace UTJ {
