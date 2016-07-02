using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public interface INumberColorLerping
{
	float Value();
}

[ExecuteInEditMode]
public class DamageNumber : MonoBehaviour 
{
	public Text text;
	public Color minColor;
	public Color maxColor;
	public float maxScaleMult;
	public float destroyAfterSeconds = 1f;
	public DamageNumberContainer animatedContainer;

	public delegate void DestoryCallback (DamageNumber script);
	public event DestoryCallback OnDestory;

	private INumberColorLerping colorLerp_;
	private Vector2 position_;
	private float startTime_;
	public Vector2 scale;
	
	public INumberColorLerping ColorLerp
	{
		set { colorLerp_ = value; }
	}

	public Vector2 Scale
	{
		get { return scale; }
		set { scale = value; }
	}

	public void PlayRandom()
	{
		var ind = Random.Range (0, 3);
		animatedContainer.animator.Play ("fly_"+ind.ToString());
		startTime_ = Time.time;
	}

	public Vector2 Position
	{
		get { return position_; }
		set { position_ = value; }
	}

	void Start () {
		text.color = minColor;
	}

	void LateUpdate () {
		if (Time.time - startTime_ > destroyAfterSeconds) {
			if (OnDestory!=null) OnDestory(this);
			return;
		}

		float alpha = text.color.a;
		float lerpValue = colorLerp_ != null ? colorLerp_.Value () : 0;
		var newColor = Color.Lerp (minColor, maxColor, lerpValue);
		newColor.a = alpha;
		text.color = newColor;
		gameObject.transform.localScale = scale * Mathf.Lerp (1, maxScaleMult, lerpValue);
		//transform.transform.localPosition = new Vector3 (position_.x, position_.y, transform.transform.localPosition.z);
	}
}
