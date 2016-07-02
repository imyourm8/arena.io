using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Skill : MonoBehaviour {

	public delegate void SkillUseCallback(Skill skill);
	public event SkillUseCallback OnSkillUse;

	private float energyCost;
	private float cooldown = 0;
	private CooldownButton btnController;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public float EnergyCost {
		get { return energyCost; }
		set { energyCost = value; }
	}

	public float Cooldown {
		get { return cooldown; }
		set { cooldown = value; if (btnController) btnController.Cooldown = cooldown; }
	}

	public GameObject ButtonController {
		set {  
			value.GetComponent<Button> ().onClick.AddListener(handleBtnTap);
			btnController = value.GetComponent<CooldownButton>();
			if (cooldown > 0) btnController.Cooldown = cooldown;
		}
	}

	private void handleBtnTap() {
		OnSkillUse (this);
	}

	virtual public void Use() {
		btnController.Use ();
	}
}
