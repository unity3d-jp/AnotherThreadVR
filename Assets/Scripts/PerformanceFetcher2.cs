using UnityEngine;
using System.Collections;

public class PerformanceFetcher2 : StateMachineBehaviour {

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (var i = 0; i < 500; ++i) {
			for (var j = 0; j < 1000; ++j) {
			}
		}
	}
}
