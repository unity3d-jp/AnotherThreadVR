using UnityEngine;
using System.Collections;

namespace UTJ {

public class VRSprite {

	// public enum Kind {
	// 	Square,
	// 	Target,
	// 	GamePadPress,
	// 	GamePadRelease,
	// }

	public enum Type {
		None,
		Full,
		Half,
		Locked,
		LockFired,
		Black,
		Red,
		Blue,
		Magenta,
		Green,
		Yellow,
		Cyan,
		GuardMark,
	}

	// singleton
	static VRSprite instance_;
	public static VRSprite Instance { get { return instance_ ?? (instance_ = new VRSprite()); } }

	private Vector2[][] uv_list_;

	// UI
	const int RECT_MAX = 128;
	private Vector3[][] vertices_;
	private Vector3[][] normals_;
	private Vector2[][] uvs_;

	private Mesh mesh_;
	private int index_;
	private Material material_;
	private MaterialPropertyBlock material_property_block_;
	static readonly int material_CamUp = Shader.PropertyToID("_CamUp");
	static readonly int material_CamPos = Shader.PropertyToID("_CamPos");
	
	public Mesh getMesh() { return mesh_; }
	public Material getMaterial() { return material_; }
	public MaterialPropertyBlock getMaterialPropertyBlock() { return material_property_block_; }

	public void init(Sprite[] sprites, Material material)
	{
		uv_list_ = new Vector2[sprites.Length][];
		float atlas_width = sprites[0].texture.width;
		for (var i = 0; i < sprites.Length; ++i) {
			float x0 = sprites[i].textureRect.xMin / atlas_width;
			float x1 = sprites[i].textureRect.xMax / atlas_width;
			float y0 = sprites[i].textureRect.yMin / atlas_width;
			float y1 = sprites[i].textureRect.yMax / atlas_width;
			uv_list_[i] = new Vector2[4];
			uv_list_[i][0] = new Vector2(x0, y0);
			uv_list_[i][1] = new Vector2(x1, y0);
			uv_list_[i][2] = new Vector2(x0, y1);
			uv_list_[i][3] = new Vector2(x1, y1);
		}

		vertices_ = new Vector3[2][] { new Vector3[RECT_MAX*4], new Vector3[RECT_MAX*4], };
		normals_ = new Vector3[2][] { new Vector3[RECT_MAX*4], new Vector3[RECT_MAX*4], };
		uvs_ = new Vector2[2][] { new Vector2[RECT_MAX*4], new Vector2[RECT_MAX*4], };

		var triangles = new int[RECT_MAX*6];
		for (var i = 0; i < RECT_MAX; ++i) {
			triangles[i*6+0] = i*4+0;
			triangles[i*6+1] = i*4+1;
			triangles[i*6+2] = i*4+2;
			triangles[i*6+3] = i*4+2;
			triangles[i*6+4] = i*4+1;
			triangles[i*6+5] = i*4+3;
		}

		// var uvs = new Vector2[RECT_MAX*4];
		// for (var i = 0; i < RECT_MAX; ++i) {
		// 	uvs[i*4+0] = new Vector2(0f, 0f);
		// 	uvs[i*4+1] = new Vector2(1f, 0f);
		// 	uvs[i*4+2] = new Vector2(0f, 1f);
		// 	uvs[i*4+3] = new Vector2(1f, 1f);
		// }

		mesh_ = new Mesh();
		mesh_.name = "VRSprite";
		mesh_.MarkDynamic();
		mesh_.vertices = vertices_[0];
		mesh_.normals = normals_[0];
		mesh_.uv = uvs_[0];
		mesh_.triangles = triangles;
		mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);

		material_ = material;
		material_property_block_ = new MaterialPropertyBlock();
#if UNITY_5_3
		material_.SetColor("_Colors0", new Color(0f, 0f, 0f, 0f)); // None
		material_.SetColor("_Colors1", new Color(1f, 1f, 1f, 1f)); // Full
		material_.SetColor("_Colors2", new Color(1f, 1f, 1f, 0.5f)); // Half
		material_.SetColor("_Colors3", new Color(0.1f, 1f, 0.2f)); // Locked
		material_.SetColor("_Colors4", new Color(1f, 0.5f, 0f)); // LockFired
		material_.SetColor("_Colors5", new Color(0f, 0f, 0f)); // Black
		material_.SetColor("_Colors6", new Color(1f, 0f, 0f)); // Red
		material_.SetColor("_Colors7", new Color(0f, 0f, 1f)); // Blue
		material_.SetColor("_Colors8", new Color(1f, 0f, 1f)); // Magenta
		material_.SetColor("_Colors9", new Color(0f, 1f, 0f)); // Green
		material_.SetColor("_Colors10", new Color(1f, 1f, 0f)); // Yellow
		material_.SetColor("_Colors11", new Color(0f, 1f, 1f)); // Cyan
		material_.SetColor("_Colors12", new Color(0.2f, 1f, 1f, 1f)); // GuardMark
#else
		var col_list = new Vector4[] {
			new Color(0f, 0f, 0f, 0f), // None
			new Color(1f, 1f, 1f, 1f), // Full
			new Color(1f, 1f, 1f, 0.5f), // Half
			new Color(0.1f, 1f, 0.2f), // Locked
			new Color(1f, 0.5f, 0f), // LockFired
			new Color(0f, 0f, 0f), // Black
			new Color(1f, 0f, 0f), // Red
			new Color(0f, 0f, 1f), // Blue
			new Color(1f, 0f, 1f), // Magenta
			new Color(0f, 1f, 0f), // Green
			new Color(1f, 1f, 0f), // Yellow
			new Color(0f, 1f, 1f), // Cyan
			new Color(0.2f, 1f, 1f), // GuardMark
		};
		material_property_block_.SetVectorArray("_Colors", col_list);
#endif
	}

	public void begin()
	{
		index_ = 0;
	}

	public void end(int front)
	{
		for (var i = index_*4; i < vertices_[front].Length; ++i) {
			normals_[front][i] = new Vector3(0f, 0f, (float)Type.None);
		}
	}

	public void renderUpdate(int front, ref Vector3 pos, ref Vector2 size, MySprite.Kind kind, Type type)
	{
		if (index_ >= RECT_MAX) {
			Debug.LogError("EXCEED sprite POOL!");
			return;
		}
		int sprite_id = (int)kind;
		if (sprite_id >= uv_list_.Length) {
			Debug.LogError("VRSprite out of range.");
			return;
		}
		int idx = index_*4;
		vertices_[front][idx+0] = pos;
		vertices_[front][idx+1] = pos;
		vertices_[front][idx+2] = pos;
		vertices_[front][idx+3] = pos;
		float width = size.x*0.5f;
		float height = size.y*0.5f;
		normals_[front][idx+0] = new Vector3(-width, -height, (float)type);
		normals_[front][idx+1] = new Vector3( width, -height, (float)type);
		normals_[front][idx+2] = new Vector3(-width,  height, (float)type);
		normals_[front][idx+3] = new Vector3( width,  height, (float)type);
		uvs_[front][idx+0] = uv_list_[sprite_id][0];
		uvs_[front][idx+1] = uv_list_[sprite_id][1];
		uvs_[front][idx+2] = uv_list_[sprite_id][2];
		uvs_[front][idx+3] = uv_list_[sprite_id][3];
		++index_;
	}

	public void render(int front, Camera camera)
	{
		mesh_.vertices = vertices_[front];
		mesh_.normals = normals_[front];
		mesh_.uv = uvs_[front];
		material_.SetVector(material_CamUp, camera.transform.up);
		material_.SetVector(material_CamPos, camera.transform.position);
    }
}

} // namespace UTJ {
