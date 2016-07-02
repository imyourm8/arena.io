using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;

public class FloatNumberSpawner : MonoBehaviour 
{
	public GameObject whereToAddNumber;
	public GameObject prefabWithText;
	public float floatingTime = 1f;
	public float fadeTime = 0.2f;
	public float randomMinX = -5f;
	public float randomMaxX = 5f;
	public float randomMinY = 0;
	public float randomMaxY = 5f;
	public float floatHeight = 100f;
	public Vector3 maxScale;
	public float scaleDuration = 0.1f;

	private INumberColorLerping numberColorLerp_;
	public INumberColorLerping ColorLerping
	{
		set{ numberColorLerp_ = value;}
	}

	void Start()
	{

	}

	public void SpawnNumber(int number, Vector2 origin)
	{
        GameObject obj = prefabWithText.GetPooled ();
		if (obj==null) return;
		var dmgNumberScript = obj.GetComponentInChildren<DamageNumber> ();
		dmgNumberScript.ColorLerp = numberColorLerp_;
		Vector2 startPos;
		startPos.x = origin.x;
		startPos.y = origin.y;
		dmgNumberScript.text.text = number.ToString ();
		startPos.x += Random.Range(randomMinX, randomMaxX);
		startPos.y += Random.Range(randomMinY, randomMaxY);
		dmgNumberScript.Position = startPos;
		obj.transform.SetParent(whereToAddNumber.transform, false);

		dmgNumberScript.OnDestory -= HandleNumberDestroyed;
		dmgNumberScript.OnDestory += HandleNumberDestroyed;
		dmgNumberScript.PlayRandom ();
		/*
		//var seq = DOTween.Sequence ();
		var oldScale = dmgNumberScript.Scale;
		var scaleTween = DOTween.To (()=>dmgNumberScript.Scale,x=>dmgNumberScript.Scale=x,maxScale, scaleDuration);
		scaleTween.SetEase (Ease.OutElastic);
		//seq.Append (scaleTween);

		var tweener = DOTween.To (()=>dmgNumberScript.Position, x=>dmgNumberScript.Position = x, startPos, floatingTime);
		tweener.OnComplete (()=>
		{
			dmgNumberScript.Scale = oldScale;
			pool_.ReturnObject(obj);
		});
		//seq.Append (tweener);
		var alphaTween = dmgNumberScript.text.DOFade (0, fadeTime);
		alphaTween.SetDelay (floatingTime - fadeTime);
		*/
	}

	void HandleNumberDestroyed(DamageNumber script)
	{
        script.animatedContainer.gameObject.ReturnPooled();
	}
}
