using UnityEngine;

namespace UTJ {

public struct DrawBuffer
{
	public enum Type
	{
		None,
		Zako,
		DragonHead,
		DragonBody,
		DragonTail,
		Shutter,
	}

	public enum SE
	{
		None,
		Bullet,
		Explosion,
		Missile,
		Lockon,
		Shield,
		
		VoiceIkuyo,
		VoiceUwaa,
		VoiceSorosoro,
		VoiceOtoto,
		VoiceYoshi,
	}

	public enum BGM
	{
		Keep,
		Stop,
		Pause,
		Resume,
		Battle,
	}

	public enum Motion
	{
		Keep,
		Play,
		Pause,
		Resume,
		GoodLuck,
	}

	public const int OBJECT_MAX = 256;

	public struct ObjectBuffer
	{
		public MyTransform transform_;
		public Type type_;

		public void init()
		{
			transform_.init();
			type_ = Type.None;
		}
		public void set(ref MyTransform transform, Type type)
		{
			transform_ = transform;
			type_ = type;
		}
	}

	public MyTransform camera_transform_;
	public MyTransform player_transform_;
	public Vector3 player_arm_offset_;
	public ObjectBuffer[] object_buffer_;
	public int object_num_;
	public SE[] se_;
	public BGM bgm_;
	public Motion motion_;

	private int audio_idx_;

	public void init()
	{
		object_buffer_ = new DrawBuffer.ObjectBuffer[OBJECT_MAX];
		for (int i = 0; i < OBJECT_MAX; ++i) {
			object_buffer_[i].init();
		}
		object_num_ = 0;

		se_ = new SE[SystemManager.AUDIO_CHANNEL_MAX];
		for (var i = 0; i < SystemManager.AUDIO_CHANNEL_MAX; ++i) {
			se_[i] = SE.None;
		}
		audio_idx_ = 0;
		
		bgm_ = BGM.Keep;
	}

	public void beginRender()
	{
		object_num_ = 0;
		audio_idx_ = 0;
	}

	public void regist(ref MyTransform transform, Type type)
	{
		object_buffer_[object_num_].set(ref transform, type);
		++object_num_;
		if (object_num_ > OBJECT_MAX) {
			Debug.LogError("EXCEED Enemy POOL!");
			Debug.Assert(false);
		}
	}

	public void registCamera(ref MyTransform transform)
	{
		camera_transform_ = transform;
	}

	public void registPlayer(ref MyTransform transform, ref Vector3 arm_offset)
	{
		player_transform_ = transform;
		player_arm_offset_ = arm_offset;
	}

	public void registSound(SE se)
	{
		if (audio_idx_ >= SystemManager.AUDIO_CHANNEL_MAX) {
			Debug.Log("max audio channel is used.");
			return;
		}
		se_[audio_idx_] = se;
		++audio_idx_;
	}

	public void registBgm(BGM bgm)
	{
		bgm_ = bgm;
	}

	public void registMotion(Motion motion)
	{
		motion_ = motion;
	}
}

} // namespace UTJ {


