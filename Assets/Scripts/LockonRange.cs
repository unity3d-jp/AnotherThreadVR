
using UnityEngine;
using System.Collections;

namespace UTJ {

public class LockonRange
{
	// singleton
	static LockonRange instance_;
	public static LockonRange Instance { get { return instance_ ?? (instance_ = new LockonRange()); } }

	private Vector3[] positions_;
	private Mesh mesh_;
	private Material material_;
	private bool on_;
	private float transparency_;

	public Mesh getMesh() { return mesh_; }
	public Material getMaterial() { return material_; }
	static readonly int material_Transparency = Shader.PropertyToID("_Transparency");

	public void init(Material material)
	{
		const int VERTS = 8;
		var vertices = new Vector3[VERTS*6];
		var colors = new Color[VERTS*6];
		float x0 = Mathf.Cos((float)-1/(float)VERTS * Mathf.PI*2f);
		float y0 = Mathf.Sin((float)-1/(float)VERTS * Mathf.PI*2f);
		const float ZF = 0f;
		const float ZB = 200f;
		const float RADF = 0f;
		const float RADB = 12.5f;
		for (var i = 0; i < VERTS; ++i) {
			float x1 = Mathf.Cos(((float)(i)/(float)VERTS) * Mathf.PI*2f);
			float y1 = Mathf.Sin(((float)(i)/(float)VERTS) * Mathf.PI*2f);
			vertices[i*6+0] = new Vector3(x0*RADF, y0*RADF, ZF);
			vertices[i*6+1] = new Vector3(x1*RADF, y1*RADF, ZF);
			vertices[i*6+2] = new Vector3(x1*RADB, y1*RADB, ZB);
			vertices[i*6+3] = new Vector3(x0*RADB, y0*RADB, ZB);
			vertices[i*6+4] = new Vector3(x0*RADB, y0*RADB, ZB);
			vertices[i*6+5] = new Vector3(x1*RADF, y1*RADF, ZF);
			x0 = x1;
			y0 = y1;

			colors[i*6+0] = new Color(0.1f, 1f, 0.2f, 0.25f);
			colors[i*6+1] = new Color(0.1f, 1f, 0.2f, 0.25f);
			colors[i*6+2] = new Color(0.1f, 1f, 0.2f, 0.5f);
			colors[i*6+3] = new Color(0.1f, 1f, 0.2f, 0.5f);
			colors[i*6+4] = new Color(0.1f, 1f, 0.2f, 0f);
			colors[i*6+5] = new Color(0.1f, 1f, 0.2f, 0f);
		}
		var indices = new int[VERTS*6];
		for (var i = 0; i < VERTS*6; ++i) {
			indices[i] = i;
		}

		mesh_ = new Mesh();
		mesh_.name = "lockon_range";
		mesh_.vertices = vertices;
		mesh_.colors = colors;
		mesh_.RecalculateBounds();
		mesh_.SetIndices(indices, MeshTopology.LineStrip, 0);
		material_ = material;

		on_ = false;
		transparency_ = 1f;
	}

	public void render(float dt)
	{
		if (on_) {
			transparency_ = Mathf.Clamp(transparency_ + (4f*dt), 0f, 1f);
		} else {
			transparency_ = Mathf.Clamp(transparency_ + (-25f*dt), 0f, 1f);
		}
		material_.SetFloat(material_Transparency, transparency_);
	}

	public void setOn(bool flg)
	{
		on_ = flg;
	}
}

} // namespace UTJ {
