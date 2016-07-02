using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ModalWindow : MonoBehaviour {

	public delegate void CloseCallback();
	public event CloseCallback OnClose;

	public Button closeBtn;
	public GameObject mainWindow;

	private Animator animator;
	private bool hideStarted = false;
	private bool isHidden = false;

	// Use this for initialization
	void Start () {
		closeBtn.onClick.AddListener (handleClose);
		animator = mainWindow.GetComponent<Animator> ();
		setShow (false, false);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void setShow(bool show, bool animated = true) {
		if (!animated || isHidden&&show)
			mainWindow.SetActive (show);
		if (animated) {
			hideStarted = !show;

			if (animator) {
				animator.SetBool("Hidden", !show);
			}
		}
		isHidden = !show;
	}

	void hideWindow() {
		mainWindow.SetActive(false);
	}

	void handleClose() {
		if (OnClose != null) OnClose ();
		setShow (false, true);
	}
}
