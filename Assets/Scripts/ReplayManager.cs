using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public class ReplayManager
{
	const int MAX_FRAMES = 120*60*10; // 10 minutes for 120fps
	struct RecordedFrame
	{
		public double game_time_;
		public MyTransform player_transform_;
		public bool is_fire_button_pressed_;
	}

	private RecordedFrame[] frames_ = new RecordedFrame[MAX_FRAMES]; // 2.5MiB
	private int frame_index_;
	private int recorded_frame_number_;
	private double start_time_;
	private bool is_recording_;
	private bool is_playing_;

	public void init()
	{
		is_recording_ = false;
		is_playing_ = false;
	}

	public void startRecording(double update_time)
	{
		Debug.Assert(frames_.Length == MAX_FRAMES);
		frame_index_ = 0;
		recorded_frame_number_ = 0;
		start_time_ = update_time;
		is_recording_ = true;
		is_playing_ = false;
	}

	public void stopRecording()
	{
		if (is_recording_) {
			is_recording_ = false;
			recorded_frame_number_ = frame_index_;
		}
	}

	public bool hasRecorded()
	{
		return (recorded_frame_number_ > 0);
	}

	public void startPlaying(double update_time, Player player)
	{
		frame_index_ = 0;
		start_time_ = update_time;
		is_recording_ = false;
		is_playing_ = true;
		if (player != null) {
			player.setReplay(this);
		}
	}

	public void stopPlaying(Player player)
	{
		if (is_playing_) {
			is_playing_ = false;
			if (player != null) {
				player.setReplay(null);
			}
		}
	}

	public bool isPlaying()
	{
		return is_playing_;
	}

	public void update(double update_time,
					   ref MyTransform player_transform,
					   bool is_fire_button_pressed)
	{
		if (is_recording_) {
			if (frame_index_ > 0 &&
				frames_[frame_index_-1].game_time_ == update_time) { // time seems to stop.
				return;
			}

			frames_[frame_index_].game_time_ = update_time - start_time_;
			frames_[frame_index_].player_transform_.position_ = player_transform.position_;
			frames_[frame_index_].player_transform_.rotation_ = player_transform.rotation_;
			frames_[frame_index_].is_fire_button_pressed_ = is_fire_button_pressed;
			// Debug.LogFormat("record:{0},{1}",
			// 				frames_[frame_index_].game_time_,
			// 				frames_[frame_index_].player_transform_);
			++frame_index_;
			if (frame_index_ >= MAX_FRAMES) {
				Debug.LogError("exceeded replay buffer");
				is_recording_ = false;
			}
		}
	}

	public void update(double update_time, Player player)
	{
		if (is_recording_) {
			update(update_time,
				   ref player.rigidbody_.transform_,
				   player.isFireButtonPressed());
		}
	}
	
	private int search(double game_time, int head_index, int tail_index)
	{
		if (head_index == tail_index) {
			return head_index;
		}
		if (head_index+1 == tail_index) {
			return head_index;
		}

		int center_index = (head_index + tail_index)/2;
		if (frames_[center_index].game_time_ < game_time) {
			return search(game_time, center_index, tail_index);
		} else {
			return search(game_time, head_index, center_index);
		}
	}

	/**
	 * @return whether next frame is available. 'false' means you should stop the replay at this frame.
	 */
	public bool getFrameData(double update_time, ref MyTransform transform, ref bool is_fire_button_pressed)
	{
		double game_time = update_time - start_time_;
		int max = recorded_frame_number_;
		int index = search(game_time, 0, max);
		int index_a = index;
		if (index_a >= max) {
			index_a = max - 1;
		}
		int index_b = index+1;
		if (index_b >= max) {
			index_b = max - 1;
		}

		double game_time_a = frames_[index_a].game_time_;
		double game_time_b = frames_[index_b].game_time_;
		double diff = game_time_b - game_time_a;
		float ratio = (float)((game_time - game_time_a)/diff);
		transform.position_ = Vector3.Lerp(frames_[index_a].player_transform_.position_,
										   frames_[index_b].player_transform_.position_,
										   ratio);
		transform.rotation_ = Quaternion.Slerp(frames_[index_a].player_transform_.rotation_,
											   frames_[index_b].player_transform_.rotation_,
											   ratio);

		bool is_fire_button_pressed_a = frames_[index_a].is_fire_button_pressed_;
		bool is_fire_button_pressed_b = frames_[index_b].is_fire_button_pressed_;
		is_fire_button_pressed = ratio < 0.5f ? is_fire_button_pressed_a : is_fire_button_pressed_b;
		return (index+1 < max);
	}
}

} // namespace UTJ {
