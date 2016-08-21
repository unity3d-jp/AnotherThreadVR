using UnityEngine;
using System.Collections;

namespace UTJ {

public class Shockwave
{
	// singleton
	static Shockwave instance_;
	public static Shockwave Instance { get { return instance_ ?? (instance_ = new Shockwave()); } }

	const int SHOCKWAVE_MAX = 32;
	const int VERTEX_NUM = 32;
	const float DIFFERENCE_TIME_FOR_WIDTH = 0.02f;

	private Vector3[] positions_;
	private float[] time_list_;

	private Vector3[][] vertices_;
	private Vector3[][] normals_;

	private int spawn_index_;
	private Mesh mesh_;
	private Material material_;
	static readonly int material_CamUp = Shader.PropertyToID("_CamUp");
	static readonly int material_CurrentTime = Shader.PropertyToID("_CurrentTime");

	public Mesh getMesh() { return mesh_; }
	public Material getMaterial() { return material_; }

	public void init(Material material)
	{
		positions_ = new Vector3[SHOCKWAVE_MAX];
		for (var i = 0; i < positions_.Length; ++i) {
			positions_[i] = new Vector3(0f, 0f, -99999f);
		}
		time_list_ = new float[SHOCKWAVE_MAX];

		vertices_ = new Vector3[2][] { new Vector3[SHOCKWAVE_MAX*VERTEX_NUM*2], new Vector3[SHOCKWAVE_MAX*VERTEX_NUM*2], };
		normals_ = new Vector3[2][] { new Vector3[SHOCKWAVE_MAX*VERTEX_NUM*2], new Vector3[SHOCKWAVE_MAX*VERTEX_NUM*2], };

		var triangles = new int[SHOCKWAVE_MAX*VERTEX_NUM*6];
		for (var i = 0; i < SHOCKWAVE_MAX; ++i) {
			for (var j = 0; j < VERTEX_NUM; ++j) {
				int idx = i*VERTEX_NUM+j;
				int idx0 = (i*VERTEX_NUM+j)*2;
				int j1 = (j < VERTEX_NUM-1 ? j+1 : 0);
				int idx1 = (i*VERTEX_NUM+j1)*2;
				triangles[idx*6+0] = idx0+0;
				triangles[idx*6+1] = idx0+1;
				triangles[idx*6+2] = idx1+0;
				triangles[idx*6+3] = idx1+0;
				triangles[idx*6+4] = idx0+1;
				triangles[idx*6+5] = idx1+1;
			}
		}

		mesh_ = new Mesh();
		mesh_.MarkDynamic();
		mesh_.name = "shockwave";
		mesh_.vertices = vertices_[0];
		mesh_.normals = normals_[0];
		mesh_.triangles = triangles;
		mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);
		material_ = material;

		spawn_index_ = 0;
	}

	public void begin()
	{
	}

	public void end(int front)
	{
		float phase_step = (Mathf.PI*2f/(float)VERTEX_NUM);
		for (var i = 0; i < SHOCKWAVE_MAX; ++i) {
			int idx = i*VERTEX_NUM;
			for (var j = 0; j < VERTEX_NUM; ++j) {
				float theta = phase_step * (float)j; // theta can be set at the time when initialization.
				int idx0 = (idx+j)*2;
				vertices_[front][idx0+0] = positions_[i];
				vertices_[front][idx0+1] = positions_[i];
				normals_[front][idx0+0].x = time_list_[i];
				normals_[front][idx0+0].y = theta;
				normals_[front][idx0+0].z = 1f; // distortion level
				normals_[front][idx0+1].x = time_list_[i] + DIFFERENCE_TIME_FOR_WIDTH;
				normals_[front][idx0+1].y = theta;
				normals_[front][idx0+1].z = 0f; // distortion level
			}
		}
	}

	public void render(int front, Camera camera, double render_time)
	{
		mesh_.vertices = vertices_[front];
		mesh_.normals = normals_[front];
		material_.SetVector(material_CamUp, camera.transform.up);
		material_.SetFloat(material_CurrentTime, (float)render_time);
	}

	public void spawn(ref Vector3 pos, double update_time)
	{
		int id = spawn_index_;
		++spawn_index_;
		if (spawn_index_ >= SHOCKWAVE_MAX) {
			spawn_index_ = 0;
		}

		positions_[id] = pos;
		time_list_[id] = (float)update_time;
	}

}

} // namespace UTJ {
