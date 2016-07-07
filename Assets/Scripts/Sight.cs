using UnityEngine;
using System.Collections;

namespace UTJ {

public class Sight
{
	// singleton
	static Sight instance_;
	public static Sight Instance { get { return instance_ ?? (instance_ = new Sight()); } }

	const int SIGHT_MAX = 64;
	const float WIDTH_RATIO = 0.025f;

	private bool[] alive_table_;
	private Vector3[] positions_;
	private Vector2[] uv2_list_;

	private Vector3[][] vertices_;
	private Vector2[][] uv2s_;
	private int spawn_index_;
	private Mesh mesh_;
	private Material material_;
	static readonly int material_CamUp = Shader.PropertyToID("_CamUp");
	static readonly int material_CamPos = Shader.PropertyToID("_CamPos");
	static readonly int material_CurrentTime = Shader.PropertyToID("_CurrentTime");

	public Mesh getMesh() { return mesh_; }
	public Material getMaterial() { return material_; }

	public void init(Material material)
	{
		positions_ = new Vector3[SIGHT_MAX];
		uv2_list_ = new Vector2[SIGHT_MAX];
		for (var i = 0; i < uv2_list_.Length; ++i) {
			uv2_list_[i].x = -999f;
		}

		vertices_ = new Vector3[2][] { new Vector3[SIGHT_MAX*8], new Vector3[SIGHT_MAX*8], };
		uv2s_ = new Vector2[2][] { new Vector2[SIGHT_MAX*8], new Vector2[SIGHT_MAX*8], };

		var triangles = new int[SIGHT_MAX * 24];
		for (var i = 0; i < SIGHT_MAX; ++i) {
			triangles[i*24+ 0] = i*8+0;
			triangles[i*24+ 1] = i*8+3;
			triangles[i*24+ 2] = i*8+1;
			triangles[i*24+ 3] = i*8+0;
			triangles[i*24+ 4] = i*8+2;
			triangles[i*24+ 5] = i*8+3;
			triangles[i*24+ 6] = i*8+2;
			triangles[i*24+ 7] = i*8+5;
			triangles[i*24+ 8] = i*8+3;
			triangles[i*24+ 9] = i*8+2;
			triangles[i*24+10] = i*8+4;
			triangles[i*24+11] = i*8+5;
			triangles[i*24+12] = i*8+4;
			triangles[i*24+13] = i*8+7;
			triangles[i*24+14] = i*8+5;
			triangles[i*24+15] = i*8+4;
			triangles[i*24+16] = i*8+6;
			triangles[i*24+17] = i*8+7;
			triangles[i*24+18] = i*8+6;
			triangles[i*24+19] = i*8+1;
			triangles[i*24+20] = i*8+7;
			triangles[i*24+21] = i*8+6;
			triangles[i*24+22] = i*8+0;
			triangles[i*24+23] = i*8+1;
		}

		var uvs = new Vector2[SIGHT_MAX*8];
		for (var i = 0; i < SIGHT_MAX; ++i) {
			uvs[i*8+0] = new Vector2(0f, 0f);
			uvs[i*8+2] = new Vector2(1f, 0f);
			uvs[i*8+4] = new Vector2(1f, 1f);
			uvs[i*8+6] = new Vector2(0f, 1f);
			uvs[i*8+1] = new Vector2(WIDTH_RATIO, WIDTH_RATIO);
			uvs[i*8+3] = new Vector2(1f-WIDTH_RATIO, WIDTH_RATIO);
			uvs[i*8+5] = new Vector2(1f-WIDTH_RATIO, 1f-WIDTH_RATIO);
			uvs[i*8+7] = new Vector2(WIDTH_RATIO, 1f-WIDTH_RATIO);
		}

		var colors = new Color[SIGHT_MAX*8];
		for (var i = 0; i < SIGHT_MAX; ++i) {
			var idx = i*8;
			colors[idx+0] = new Color(0.1f, 1f, 0.2f, 0f);
			colors[idx+2] = new Color(0.1f, 1f, 0.2f, 0f);
			colors[idx+4] = new Color(0.1f, 1f, 0.2f, 0f);
			colors[idx+6] = new Color(0.1f, 1f, 0.2f, 0f);
			colors[idx+1] = new Color(0.1f, 1f, 0.2f, 0.5f);
			colors[idx+3] = new Color(0.1f, 1f, 0.2f, 0.5f);
			colors[idx+5] = new Color(0.1f, 1f, 0.2f, 0.5f);
			colors[idx+7] = new Color(0.1f, 1f, 0.2f, 0.5f);
		}
		
		mesh_ = new Mesh();
		mesh_.MarkDynamic();
		mesh_.name = "sight";
		mesh_.vertices = vertices_[0];
		mesh_.triangles = triangles;
		mesh_.uv = uvs;
		mesh_.uv2 = uv2s_[0];
		mesh_.colors = colors;
		mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);
		material_ = material;

		alive_table_ = new bool[SIGHT_MAX];
		for (var i = 0; i < SIGHT_MAX; ++i) {
			alive_table_[i] = false;
		}
		spawn_index_ = 0;
	}

	public void restart()
	{
		for (var i = 0; i < alive_table_.Length; ++i) {
			alive_table_[i] = false;
		}
	}

	public int spawn(double update_time)
	{
		int cnt = 0;
		while (alive_table_[spawn_index_]) {
			++spawn_index_;
			if (spawn_index_ >= SIGHT_MAX) {
				spawn_index_ = 0;
			}
			++cnt;
			if (cnt >= SIGHT_MAX) {
				Debug.LogError("EXCEED Sight POOL!");
				Debug.Assert(false);
				return -1;
			}
		}
		alive_table_[spawn_index_] = true;

		uv2_list_[spawn_index_] = new Vector2((float)update_time, 0f /* unused */);
		return spawn_index_;
	}

	public void destroy(int id)
	{
		alive_table_[id] = false;
	}

	// public void updateAll(float dt)
	// {
	// 	for (var i = 0; i < sizes_.Length; ++i) {
	// 		if (sizes_[i] > 0f) {
	// 			sizes_[i] -= 960f * dt;
	// 			if (sizes_[i] < 0f) {
	// 				sizes_[i] = 0f;
	// 			}
	// 		}
	// 	}
	// }

	public void renderUpdate(int id, ref Vector3 pos)
	{
		positions_[id] = pos;
	}

	// public bool isShown(int id)
	// {
	// 	return sizes_[id] > 0f;
	// }

	public void begin()
	{
	}

	public void end(int front)
	{
		for (var i = 0; i < SIGHT_MAX; ++i) {
			int idx = i*8;
			vertices_[front][idx+0] = positions_[i];
			vertices_[front][idx+1] = positions_[i];
			vertices_[front][idx+2] = positions_[i];
			vertices_[front][idx+3] = positions_[i];
			vertices_[front][idx+4] = positions_[i];
			vertices_[front][idx+5] = positions_[i];
			vertices_[front][idx+6] = positions_[i];
			vertices_[front][idx+7] = positions_[i];
			uv2s_[front][idx+0] = uv2_list_[i];
			uv2s_[front][idx+1] = uv2_list_[i];
			uv2s_[front][idx+2] = uv2_list_[i];
			uv2s_[front][idx+3] = uv2_list_[i];
			uv2s_[front][idx+4] = uv2_list_[i];
			uv2s_[front][idx+5] = uv2_list_[i];
			uv2s_[front][idx+6] = uv2_list_[i];
			uv2s_[front][idx+7] = uv2_list_[i];
		}
	}

	public void render(int front, Camera camera, double render_time)
	{
		mesh_.vertices = vertices_[front];
		mesh_.uv2 = uv2s_[front];
		material_.SetVector(material_CamUp, camera.transform.up);
		material_.SetVector(material_CamPos, camera.transform.position);
		material_.SetFloat(material_CurrentTime, (float)render_time);
	}
}

} // namespace UTJ {
