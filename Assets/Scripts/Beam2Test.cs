using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class Beam2Test : MonoBehaviour {

	public Material material_;

	struct Bullet {
		public bool alive_;
		public Vector3 position_;
		public Vector3 velocity_;
		public int id_;
		public int cnt_;
	}
	private Bullet[] pool_;
	private int pool_idx_;

	private MeshFilter mf_;
	private MeshRenderer mr_;

	IEnumerator loop()
	{
		yield return new WaitForSeconds(0.5f);

		pool_ = new Bullet[16000];
		for (var i = 0; i < pool_.Length; ++i) {
			pool_[i].alive_ = false;
			pool_[i].cnt_ = 0;
		}
		pool_idx_ = 0;

		for (;;) {

			// spawn
			if (Random.Range(0, 10) < 2) {
				for (var i = 0; i < 1; ++i) {
					while (pool_[pool_idx_].alive_) {
						++pool_idx_;
						if (pool_idx_ >= pool_.Length) {
							pool_idx_ = 0;
						}
					}
					pool_[pool_idx_].alive_ = true;
					pool_[pool_idx_].position_ = Vector3.zero;
					pool_[pool_idx_].velocity_ = Random.onUnitSphere * 8f;
					pool_[pool_idx_].velocity_.y = Mathf.Abs(pool_[pool_idx_].velocity_.y);
					pool_[pool_idx_].id_ = Beam2.Instance.spawn(1f /* width */, Beam2.Type.EnemyBullet);
					pool_[pool_idx_].cnt_ = 100;
				}
			}

			// update
			for (var i = 0; i < pool_.Length; ++i) {
				if (!pool_[i].alive_) continue;
					
				pool_[i].position_ += pool_[i].velocity_ * (1f/60f);

				--pool_[i].cnt_;
				if (pool_[i].cnt_ <= 0) {
					pool_[i].alive_ = false;
					Beam2.Instance.destroy(pool_[i].id_);
				}

			}
			
			// render
			Beam2.Instance.begin(0 /* front */);
			for (var i = 0; i < pool_.Length; ++i) {
				if (!pool_[i].alive_) continue;

				var tail = pool_[i].position_ - (pool_[i].velocity_ * 1f);
				Beam2.Instance.renderUpdate(0 /* front */,
										   pool_[i].id_,
										   ref pool_[i].position_,
										   ref tail);
			}
			Beam2.Instance.end();

			yield return null;
		}
	}

	void Start()
	{
		mf_ = GetComponent<MeshFilter>();
		mr_ = GetComponent<MeshRenderer>();
		Beam2.Instance.init(material_);
		var mesh = Beam2.Instance.getMesh();
		mf_.sharedMesh = mesh;
		mr_.sharedMaterial = Beam2.Instance.getMaterial();
		mr_.SetPropertyBlock(Beam2.Instance.getMaterialPropertyBlock());
		StartCoroutine(loop());
	}
	
	void Update()
	{
		Beam2.Instance.render(0 /* front */);
	}
}

} // namespace UTJ {
