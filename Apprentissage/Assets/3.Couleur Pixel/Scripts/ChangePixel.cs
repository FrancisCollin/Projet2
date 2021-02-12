using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePixel : MonoBehaviour
{
    public Camera cam;
    public Texture baseTexture;                  // used to deterimne the dimensions of the runtime texture
    public Material meshMaterial;                 // used to bind the runtime texture as the albedo of the mesh

    private PaintableTexture mainMap;
    private PaintableTexture metalic;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;

        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
        tex.Apply();
    }
}

////TODO : Faire en sorte que le Base Map sois généré au début de la scène en mode Read/Write et même chose pour le Metallic Map au besoin.
//[System.Serializable]
//public class PaintableTexture
//{
//    public string id;
//    public RenderTexture runTimeTexture;
//    public RenderTexture paintedTexture;

//    private Material mPaintInUV;
//    private Material mFixedEdges;
//    private RenderTexture fixedIlsands;

//    public PaintableTexture(Color clearColor, int width, int height, string id,
//        Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, RenderTexture markedIlsandes)
//    {
//        this.id = id;

//        runTimeTexture = new RenderTexture(width, height, 0)
//        {
//            anisoLevel = 0,
//            useMipMap = false,
//            filterMode = FilterMode.Bilinear
//        };

//        paintedTexture = new RenderTexture(width, height, 0)
//        {
//            anisoLevel = 0,
//            useMipMap = false,
//            filterMode = FilterMode.Bilinear
//        };


//        fixedIlsands = new RenderTexture(paintedTexture.descriptor);

//        Graphics.SetRenderTarget(runTimeTexture);
//        GL.Clear(false, true, clearColor);
//        Graphics.SetRenderTarget(paintedTexture);
//        GL.Clear(false, true, clearColor);


//        mPaintInUV = new Material(sPaintInUV);
//        if (!mPaintInUV.SetPass(0)) Debug.LogError("Invalid Shader Pass: ");
//        mPaintInUV.SetTexture("_MainTex", paintedTexture);

//        mFixedEdges = new Material(fixIlsandEdgesShader);
//        mFixedEdges.SetTexture("_IlsandMap", markedIlsandes);
//        mFixedEdges.SetTexture("_MainTex", paintedTexture);

//    }
// }
