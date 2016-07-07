using UnityEngine;
using System.Collections;

public class OperatorAnimator : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.IsName("Base Layer.WIN00")) {
			animator.SetBool("goodluck", false);
		}
	}
}
