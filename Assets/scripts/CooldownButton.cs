using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownButton : MonoBehaviour {

	public Button overlayButton;
	public Button mainButton;
	public GameObject icon;
	public float startIconAlpha;

	private float cooldown = 0;
	private float initialCooldown = 0;

	// Use this for initialization
	void Start () {
		mainButton.onClick.AddListener (handleClick);
	}
	
	// Update is called once per frame
	void Update () {
		if (cooldown > 0) {
			cooldown -= Time.deltaTime;
			float mix = cooldown / initialCooldown;
			icon.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, startIconAlpha + (1 - startIconAlpha) * (1-mix));
			overlayButton.image.fillAmount = mix;
		} else if (!mainButton.interactable) {
			mainButton.interactable = true;
		}
	}

	private void handleClick() {
		Use ();
	}

	public void Use()
	{
		mainButton.interactable = false;
		cooldown = initialCooldown;
		icon.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, startIconAlpha);
		overlayButton.image.fillAmount = 1;
	}

	public float Cooldown {
		set { initialCooldown = value; }
	}
}
