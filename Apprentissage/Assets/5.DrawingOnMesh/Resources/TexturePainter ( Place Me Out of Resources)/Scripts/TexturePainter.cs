/// <summary>
/// Francis Collin, 1738286
/// Il s'agit de la classe principale du projet, elle est chargée de diffuser des raycasts sur un modèle et de placer des Prefabs de pinceaux devant la caméra du canvas.
/// </summary>


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Painter_BrushMode{PAINT,DECAL};
public class TexturePainter : MonoBehaviour {
	public GameObject brushCursor,brushContainer; //Le curseur qui overlaps le modèle et notre conteneur pour les pinceaux peints
	public Camera sceneCamera,canvasCam;  //La caméra qui regarde le modèle et la caméra qui regarde le canvas.
	public Sprite cursorPaint,cursorDecal; //Curseur pour les différentes fonctions
	public RenderTexture canvasTexture; //Texture de rendu qui regarde notre texture de base et les pinceaux peints
	public Material baseMaterial; //The material of our base texture (Were we will save the painted texture)

	Painter_BrushMode mode; //Notre mode peinture(pinceaux ou décalcomanies)
	float brushSize=1.0f; //La taille de notre pinceau
		Color brushColor; //La couleur sélectionnée
	int brushCounter =0,MAX_BRUSH_COUNT=1000; //Pour éviter d'avoir des millions de pinceaux
	bool saving =false; //Bool pour vérifier si nous sauvegardons la texture


	void Update () {
		brushColor = ColorSelector.GetColor (); //Met à jour notre couleur peinte avec la couleur sélectionnée
		if (Input.GetMouseButton(0)) {
			DoAction();
		}
		UpdateBrushCursor ();
	}


	//L'action principale, instancie un pinceau ou un décalque à la position cliquée sur la carte UV
	void DoAction(){	
		if (saving)
			return;
		Vector3 uvWorldPosition=Vector3.zero;		
		if(HitTestUVPosition(ref uvWorldPosition)){
			GameObject brushObj;
			if(mode==Painter_BrushMode.PAINT){

				brushObj=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Peindre un pinceau
				brushObj.GetComponent<SpriteRenderer>().color=brushColor; //Définit la couleur du pinceau
			}
			else
			{
				brushObj=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/DecalEntity")); //Peindre un décalque
			}
			brushColor.a=brushSize*2.0f; //Les pinceaux ont un alpha pour avoir un effet de fusion lorsqu'ils sont peints un par dessus l'autre.
			brushObj.transform.parent=brushContainer.transform; //Ajoute le pinceau à notre conteneur pour être netoyé plus tard.
			brushObj.transform.localPosition=uvWorldPosition; //La position du pinceau (dans l'UVMap)
			brushObj.transform.localScale=Vector3.one*brushSize; //La taille du pinceau
		}
		brushCounter++; //Ajouter jusqu'au nombre maximum de pinceaux.
		if (brushCounter >= MAX_BRUSH_COUNT) { //Si nous atteignons le maximum de pinceaux disponibles, aplatit la texture et efface les pinceaux
			brushCursor.SetActive (false);
			saving=true;
			Invoke("SaveTexture",0.1f);
			
		}
	}

	//Met à jour en temps réel le curseur de peinture sur le mesh
	void UpdateBrushCursor(){
		Vector3 uvWorldPosition=Vector3.zero;
		if (HitTestUVPosition (ref uvWorldPosition) && !saving) {
			brushCursor.SetActive(true);
			brushCursor.transform.position =uvWorldPosition+brushContainer.transform.position;									
		} else {
			brushCursor.SetActive(false);
		}		
	}

	// Renvoie la position sur le texuremap en fonction d'un hit dans le mesh collider
	bool HitTestUVPosition(ref Vector3 uvWorldPosition){
		RaycastHit hit;
		Vector3 cursorPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay=sceneCamera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast(cursorRay,out hit,200)){
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;			
			Vector2 pixelUV  = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			uvWorldPosition.x=pixelUV.x-canvasCam.orthographicSize;//Pour centrer le UV sur X
			uvWorldPosition.y=pixelUV.y-canvasCam.orthographicSize;//Pour centrer le UV sur Y
			uvWorldPosition.z=0.0f;
			return true;
		}
		else{		
			return false;
		}
		
	}

	// Définit le matériau de base avec notre texture de notre canvas, puis supprime tous nos pinceaux
	void SaveTexture(){		
		brushCounter=0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = canvasTexture;
		Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);		
		tex.ReadPixels (new Rect (0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
		tex.Apply ();
		RenderTexture.active = null;
		baseMaterial.mainTexture =tex; //Met la texture peinte comme la texture de base
		foreach (Transform child in brushContainer.transform) {//Supprime les pinceaux
			Destroy(child.gameObject);
		}
		//StartCoroutine ("SaveTextureToFile"); //Méthode pour sauvegarder la texture
		Invoke ("ShowCursor", 0.1f);
	}

	//Affiche à nouveau le curseur de l'utilisateur (pour éviter de l'enregistrer dans la texture)
	void ShowCursor(){	
		saving = false;
	}

	////////////////// MÉTHODES PUBLIQUES //////////////////

	public void SetBrushMode(Painter_BrushMode brushMode){ //Sets if we are painting or placing decals
		mode = brushMode;
		brushCursor.GetComponent<SpriteRenderer> ().sprite = brushMode == Painter_BrushMode.PAINT ? cursorPaint : cursorDecal;
	}
	public void SetBrushSize(float newBrushSize){ //Sets the size of the cursor brush or decal
		brushSize = newBrushSize;
		brushCursor.transform.localScale = Vector3.one * brushSize;
	}

	////////////////// MÉTHODES OPTIONNELLES //////////////////

	#if !UNITY_WEBPLAYER
	IEnumerator SaveTextureToFile(Texture2D savedTexture){		
			brushCounter=0;
			string fullPath=System.IO.Directory.GetCurrentDirectory()+"\\UserCanvas\\";
			System.DateTime date = System.DateTime.Now;
			string fileName = "CanvasTexture.png";
			if (!System.IO.Directory.Exists(fullPath))		
				System.IO.Directory.CreateDirectory(fullPath);
			var bytes = savedTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes(fullPath+fileName, bytes);
			Debug.Log ("<color=orange>Saved Successfully!</color>"+fullPath+fileName);
			yield return null;
		}
	#endif
}
