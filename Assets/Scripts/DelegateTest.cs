using UnityEngine;
using System.Collections;

namespace UTJ {

public class DelegateTest : MonoBehaviour {

	class A
	{
		public RigidbodyTransform rigidbody_;
		public void m()
		{
			Debug.Log(rigidbody_.transform_.position_);
		}
	}

	delegate void AF();
	AF af;

	// Use this for initialization
	void Start () {
		A a0 = new A();
		a0.rigidbody_.transform_.position_ = Vector3.forward;
		A a1 = new A();
		a1.rigidbody_.transform_.position_ = Vector3.back;
		var a0f = new AF(a0.m);
		a0f();
		var a1f = new AF(a1.m);
		a1f();
		a0.rigidbody_.transform_.position_ = Vector3.left;
		a0f();
		a1.rigidbody_.transform_.position_ = Vector3.right;
		a1f();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

} // namespace UTJ {
