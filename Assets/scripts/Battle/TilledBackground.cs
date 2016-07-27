using UnityEngine;
using System.Collections;

public class TilledBackground : MonoBehaviour 
{
    private MeshRenderer renderer_;
    private Mesh mesh_;
    private MeshFilter filter_;
    private Material material_;
	private Transform transform_;

	void Start () 
    {
        mesh_ = new Mesh();
        transform_ = transform;
        renderer_ = GetComponent<MeshRenderer>();
        filter_ = GetComponent<MeshFilter>();
        filter_.mesh = mesh_;
        material_ = renderer_.material;
        mesh_.vertices = new Vector3[]
        {
            new Vector3(1,1,1),
            new Vector3(1,0,1),
            new Vector3(0,1,1),
            new Vector3(0,0,1)
        };

        mesh_.triangles = new int[]
        {
            0, 1, 2,
            2, 1, 3
        };

        mesh_.uv = new Vector2[]
        {
            new Vector2(0,1),
            new Vector2(0,0),
            new Vector2(1,1),
            new Vector2(1,0)
        };
	}

	void Update () 
    {
        var scale = new Vector2(transform_.localScale.x, transform_.localScale.y);
        material_.SetTextureScale("_MainTex", scale);
        material_.SetTextureOffset("_MainTex", new Vector2(scale.x/10, scale.y/10));
	}
}
