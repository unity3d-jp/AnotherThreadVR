using UnityEngine;
using System.Collections;

namespace UTJ {

public class Trail
{
	// singleton
	static Trail instance_;
	public static Trail Instance { get { return instance_ ?? (instance_ = new Trail()); } }

	public enum Type
	{
		None,
		NoneA,
		Player,
		PlayerA,
		Missile,
		MissileA,
	}

	const int TRAIL_MAX = 32;
	public const int NODE_NUM = 16;
	const int MAX_PRECISION_LEVEL = 16;
	const int UPDATE_POSITION_NUM = NODE_NUM * MAX_PRECISION_LEVEL;

	private bool[] alive_table_;
	private int spawn_index_;

	public Vector3[] positions_;
	public float[] widths_;
	public Type[] types_;
	public float[] update_time_;

	public Vector3[][] vertices_;
	public Vector3[][] normals_;
	public Vector2[][] uv2s_;
	
	private Mesh mesh_;
	private Material material_;
	private MaterialPropertyBlock material_property_block_;

	public Mesh getMesh() { return mesh_; }
	public Material getMaterial() { return material_; }
	public MaterialPropertyBlock getMaterialPropertyBlock() { return material_property_block_; }

	private int[] work_index_list_;

	// Main Thread
	public void init(Material material)
	{
		alive_table_ = new bool[TRAIL_MAX];
		for (var i = 0; i < TRAIL_MAX; ++i) {
			alive_table_[i] = false;
		}
		spawn_index_ = 0;
		positions_ = new Vector3[UPDATE_POSITION_NUM*TRAIL_MAX];
		widths_ = new float[TRAIL_MAX];
		types_ = new Type[TRAIL_MAX];
		update_time_ = new float[UPDATE_POSITION_NUM*TRAIL_MAX];

		vertices_ = new Vector3[2][] { new Vector3[NODE_NUM*2*TRAIL_MAX], new Vector3[NODE_NUM*2*TRAIL_MAX], };
		normals_ = new Vector3[2][] { new Vector3[NODE_NUM*2*TRAIL_MAX], new Vector3[NODE_NUM*2*TRAIL_MAX], };
		uv2s_ = new Vector2[2][] { new Vector2[NODE_NUM*2*TRAIL_MAX], new Vector2[NODE_NUM*2*TRAIL_MAX], };
		for (var i = 0; i < TRAIL_MAX; ++i) {
			clear(i);
		}

		var triangles = new int[(NODE_NUM-1)*6*TRAIL_MAX];
		for (var l = 0; l < TRAIL_MAX; ++l) {
			var lidx = l*(NODE_NUM-1)*6;
			var idx = l*NODE_NUM*2;
			for (var i = 0; i < (NODE_NUM-1); ++i) {
				triangles[lidx+i*6+0] = idx+(i+0)*2+0;
				triangles[lidx+i*6+1] = idx+(i+0)*2+1;
				triangles[lidx+i*6+2] = idx+(i+1)*2+0;
				triangles[lidx+i*6+3] = idx+(i+1)*2+0;
				triangles[lidx+i*6+4] = idx+(i+0)*2+1;
				triangles[lidx+i*6+5] = idx+(i+1)*2+1;
			}
		}

		var uvs = new Vector2[NODE_NUM*2*TRAIL_MAX];
		for (var i = 0; i < TRAIL_MAX; ++i) {
			var idx = i*NODE_NUM*2;
			for (var j = 0; j < NODE_NUM; ++j) {
				uvs[idx+j*2+0] = new Vector2(0f, 0f);
				uvs[idx+j*2+1] = new Vector2(1f, 0f);
			}
		}

		work_index_list_ = new int[NODE_NUM];

		mesh_ = new Mesh();
		mesh_.MarkDynamic();
		mesh_.name = "trail";
		mesh_.vertices = vertices_[0];
		mesh_.normals = normals_[0];
		mesh_.triangles = triangles;
		mesh_.uv = uvs;
		mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);
		material_ = material;
		material_property_block_ = new MaterialPropertyBlock();
#if UNITY_5_3
		material_.SetColor("_Colors0", new Color(0f, 0f, 0f, 0f)); // None
		material_.SetColor("_Colors1", new Color(0f, 0f, 0f, 0f)); // NoneA
		material_.SetColor("_Colors2", new Color(0.1f, 0.6f, 1f, 1f)); // Player
		material_.SetColor("_Colors3", new Color(0.1f, 0.6f, 1f, 0f)); // PlayerA
		material_.SetColor("_Colors4", new Color(0.1f, 1f, 0.5f, 1f)); // Missile
		material_.SetColor("_Colors5", new Color(0.1f, 1f, 0.5f, 0f)); // MissileA
#else
		var col_list = new Vector4[] {
			new Vector4(0f, 0f, 0f, 0f), // None
			new Vector4(0f, 0f, 0f, 0f), // NoneA
			new Vector4(0.1f, 0.6f, 1f, 0.2f), // Player
			new Vector4(0.1f, 0.6f, 1f, 0f), // PlayerA
			new Vector4(0.1f, 1f, 0.5f, 1f), // Missile
			new Vector4(0.1f, 1f, 0.5f, 0f), // MissileA
		};
		material_property_block_.SetVectorArray("_Colors", col_list);
#endif
	}

	public void restart()
	{
		for (var i = 0; i < alive_table_.Length; ++i) {
			alive_table_[i] = false;
		}
	}

	// Main Thread
	public void render(int front)
	{
		if (material_ == null) {
			return;
		}
		mesh_.vertices = vertices_[front];
		mesh_.normals = normals_[front];
		mesh_.uv2 = uv2s_[front];
		// mesh_.RecalculateBounds(); // debug!
	}


	public void begin(int front)
	{
		var far = new Vector3(0f, 0f, 0f);
		for (var i = 0; i < NODE_NUM*TRAIL_MAX*2; ++i) {
			vertices_[front][i] = far;
		}
		var zero = new Vector2(0f, 0f);
		for (var i = 0; i < NODE_NUM*TRAIL_MAX*2; ++i) {
			uv2s_[front][i] = zero;
		}
	}

	public void end()
	{
	}

	public int spawn(ref Vector3 pos, float width, Type type, int length = NODE_NUM)
	{
		int cnt = 0;
		while (alive_table_[spawn_index_]) {
			++spawn_index_;
			if (spawn_index_ >= TRAIL_MAX) {
				spawn_index_ = 0;
			}
			++cnt;
			if (cnt >= TRAIL_MAX) {
				Debug.LogError("EXCEED Trail POOL!");
				Debug.Assert(false);
				return -1;
			}
		}
		alive_table_[spawn_index_] = true;
		int id = spawn_index_;
		for (var i = 0; i < UPDATE_POSITION_NUM; ++i) {
			positions_[id*UPDATE_POSITION_NUM + i] = pos;
		}
		widths_[id] = width;
		types_[id] = type;
		return id;
	}

	public void resetPosition(int id, ref Vector3 pos)
	{
		for (var i = 0; i < UPDATE_POSITION_NUM; ++i) {
			positions_[id*UPDATE_POSITION_NUM + i] = pos;
		}
	}

	public void update(int id, ref Vector3 pos, float dt, float flow_z, double update_time)
	{
		// shift
		for (var i = UPDATE_POSITION_NUM-1; i >= 1; --i) {
			positions_[id*UPDATE_POSITION_NUM + i] = positions_[id*UPDATE_POSITION_NUM + i-1];
			update_time_[id*UPDATE_POSITION_NUM + i] = update_time_[id*UPDATE_POSITION_NUM + i-1];
		}
		// flow
		for (var i = UPDATE_POSITION_NUM-1; i >= 1; --i) {
			positions_[id*UPDATE_POSITION_NUM + i].z += flow_z * dt;
		}
		// update root
		positions_[id*UPDATE_POSITION_NUM + 0] = pos;
		update_time_[id*UPDATE_POSITION_NUM + 0] = (float)update_time;
	}

	public void renderUpdate(int front, int id)
	{
		{
			int idx = id * UPDATE_POSITION_NUM;
			float time_prev = 0f;
			float time = 0f;
			int j = 0;
			for (var i = 0; i < NODE_NUM; ++i) {
				if (i > 0) {
					const float STANDARD_FPS = 0.008333f;
					for (;j < UPDATE_POSITION_NUM && time < STANDARD_FPS; ++j) {
						time += (time_prev - update_time_[idx+j]);
						time_prev = update_time_[idx+j];
					}
					--j;
				}
				work_index_list_[i] = j;
				time_prev = update_time_[idx+j];
				time = 0;
				++j;
			}
		}
		for (var i = 0; i < NODE_NUM; ++i) {
			var i0 = work_index_list_[i];
			vertices_[front][(id*NODE_NUM+i)*2+0] = positions_[id*UPDATE_POSITION_NUM+i0];
			vertices_[front][(id*NODE_NUM+i)*2+1] = positions_[id*UPDATE_POSITION_NUM+i0];
			var i1 = work_index_list_[Mathf.Clamp(i+1, 0, NODE_NUM-1)];
			var diff = positions_[id*NODE_NUM+i0] - positions_[id*UPDATE_POSITION_NUM+i1];
			normals_[front][(id*NODE_NUM+i)*2+0] = diff;
			normals_[front][(id*NODE_NUM+i)*2+1] = diff;
		}
		for (var i = 0; i < NODE_NUM; ++i) {
			uv2s_[front][(id*NODE_NUM+i)*2+0] = new Vector2(widths_[id], (float)types_[id]);
			uv2s_[front][(id*NODE_NUM+i)*2+1] = new Vector2(widths_[id], (float)types_[id]);
			if (i == 0 || i >= NODE_NUM-1) {
				uv2s_[front][(id*NODE_NUM+i)*2+1].y += 1f; // make it select alpha zero.
			}				
		}
	}

	public void destroy(int id)
	{
		clear(id);
	}

	private void clear(int id)
	{
		alive_table_[id] = false;
	}
}

} // namespace UTJ {
