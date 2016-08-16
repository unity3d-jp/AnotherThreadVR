using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public class GameManager
{
	// singleton
	static GameManager instance_;
	public static GameManager Instance { get { return instance_ ?? (instance_ = new GameManager()); } }

	private enum GamePhase {
		Title,
		Game,
	}
	private GamePhase game_phase_ = GamePhase.Title;
	private IEnumerator enumerator_;
	private double update_time_;
	private ReplayManager replay_manager_;

	public void init()
	{
		enumerator_ = act();	// この瞬間は実行されない
		replay_manager_ = new ReplayManager();
		replay_manager_.init();
	}

	public void update(float dt, double update_time)
	{
		update_time_ = update_time;
		enumerator_.MoveNext();
		replay_manager_.update(update_time, Player.Instance);
	}

	public void restart()
	{
		replay_manager_.stopRecording();
		replay_manager_.stopPlaying(Player.Instance);
		enumerator_ = null;
		enumerator_ = act();
	}

	private IEnumerator act()
	{
		game_phase_ = GamePhase.Title;
		Player.Instance.setPhaseTitle();
		Player.Instance.setPositionXY(0f, -27f);
		SystemManager.Instance.registBgm(DrawBuffer.BGM.Stop);
		SystemManager.Instance.registMotion(DrawBuffer.Motion.Play);
		SystemManager.Instance.setFlowSpeed(0f);
		SystemManager.Instance.setSubjective(true);

		for (var w = new Utility.WaitForSeconds(5f, update_time_); !w.end(update_time_);) {yield return null; }
		Notice notice;
		{
			notice = Notice.create(-400f, 400f,
								   MySprite.Kind.GamePadPress,
								   MySprite.Type.Full,
								   false /* blink */);
		}
		while (game_phase_ == GamePhase.Title) {
			if (InputManager.Instance.getButton(InputManager.Button.Fire) > 0) {
				game_phase_ = GamePhase.Game;
				SystemManager.Instance.registSound(DrawBuffer.SE.Missile);
				SystemManager.Instance.registMotion(DrawBuffer.Motion.GoodLuck);
				replay_manager_.startRecording(update_time_);
			} else if (replay_manager_.hasRecorded()) {
				game_phase_ = GamePhase.Game;
				SystemManager.Instance.registSound(DrawBuffer.SE.Missile);
				SystemManager.Instance.registMotion(DrawBuffer.Motion.GoodLuck);
				SystemManager.Instance.setSubjective(false);
				replay_manager_.startPlaying(update_time_, Player.Instance);
			}
			yield return null;
		}
		notice.destroy();

		for (var w = new Utility.WaitForSeconds(1.5f, update_time_); !w.end(update_time_);) {yield return null; }
		Player.Instance.setPhaseStart();
		SystemManager.Instance.registBgm(DrawBuffer.BGM.Battle);
		for (var w = new Utility.WaitForSeconds(4f, update_time_); !w.end(update_time_);) {	yield return null; }
		Notice.create(0f, 0f,
					  update_time_ + 3f,
					  MySprite.Kind.GamePadPress,
					  MySprite.Type.Full,
					  true /* blink */);
		for (var w = new Utility.WaitForSeconds(1f, update_time_); !w.end(update_time_);) {	yield return null; }

		Player.Instance.setPhaseBattle();
		SystemManager.Instance.setFlowSpeed(-100f);
		while (TubeScroller.Instance.getDistance() < 100f) {
			yield return null;
		}
		
		for (var j = 0; j < 4; ++j) {
			for (var w = new Utility.WaitForSeconds(2f, update_time_); !w.end(update_time_);) {	yield return null; }
			for (var i = 0; i < 4; ++i) {
				for (var w = new Utility.WaitForSeconds(0.5f, update_time_); !w.end(update_time_);) { yield return null; }
				Enemy.create(Enemy.Type.Zako2);
			}

			if (j == 1) {
				Notice.create(-200f, 200f,
							  update_time_ + 3f,
							  MySprite.Kind.GamePadRelease,
							  MySprite.Type.Full,
							  true /* blink */);
			}
		}

		while (TubeScroller.Instance.getDistance() < 2400f) {
			yield return null;
		}

		Enemy dragon = Enemy.create(Enemy.Type.Dragon);
		SystemManager.Instance.setFlowSpeed(-10f);

		while (TubeScroller.Instance.getDistance() < 2800f) {
			for (var w = new Utility.WaitForSeconds(5f, update_time_); !w.end(update_time_);) {	yield return null; }
			for (var j = new Utility.WaitForSeconds(2f, update_time_); !j.end(update_time_);) {
				yield return null;
				Enemy.create(Enemy.Type.Zako2);
				for (var w = new Utility.WaitForSeconds(0.25f, update_time_); !w.end(update_time_);) {	yield return null; }
			}
			yield return null;
		}
		
		float flow_speed = 150f;
		SystemManager.Instance.setFlowSpeed(-150f);
		dragon.setMode(Dragon.Mode.Chase);

		for (var i = 0; i < 4; ++i) {
			for (var v = new Utility.WaitForSeconds(3f, update_time_); !v.end(update_time_);) {
				Enemy.create(Enemy.Type.Zako2);
				for (var w = new Utility.WaitForSeconds(0.5f, update_time_); !w.end(update_time_);) { yield return null; }
			}
			for (var w = new Utility.WaitForSeconds(2f, update_time_); !w.end(update_time_);) {	yield return null; }
		}

		for (var w = new Utility.WaitForSeconds(2f, update_time_); !w.end(update_time_);) {	yield return null; }
		dragon.setMode(Dragon.Mode.Farewell);
		for (var i = 0; i < 16; ++i) {
			float rot = 30f * i;
			Shutter.create(rot, flow_speed, update_time_);
			Shutter.create(rot+180f, flow_speed, update_time_);
			for (var w = new Utility.WaitForSeconds(1f, update_time_); !w.end(update_time_);) {	yield return null; }
		}
		while (TubeScroller.Instance.getDistance() < 9400f) {
			yield return null;
		}

		dragon.setMode(Dragon.Mode.LastAttack);
		for (var w = new Utility.WaitForSeconds(13f, update_time_); !w.end(update_time_);) { yield return null; }
		Notice.create(0f, 0f,
					  update_time_ + 6f,
					  MySprite.Kind.Logo,
					  MySprite.Type.Full,
					  false);
		for (var w = new Utility.WaitForSeconds(5f, update_time_); !w.end(update_time_);) { yield return null; }
		
		SystemManager.Instance.restart();
	}
}

} // namespace UTJ {
