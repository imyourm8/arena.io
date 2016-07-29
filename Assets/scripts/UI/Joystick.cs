using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    private Image joystick_ = null;
    private RectTransform rect_;
    private Vector4 joystickBounds_;

    public Vector2 inputDirection;

    void Start()
    {
        rect_ = GetComponent<RectTransform>();
        joystickBounds_ = new Vector4(
            rect_.position.x - joystick_.rectTransform.rect.width / 2,
            rect_.position.x + joystick_.rectTransform.rect.width / 2,
            rect_.position.y - joystick_.rectTransform.rect.height / 2,
            rect_.position.y + joystick_.rectTransform.rect.height / 2
        );
    }

    #region IDragHandler implementation
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        HandleDrag(eventData);
    }
    #endregion
    
    #region IPointerUpHandler implementation
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }
    #endregion

    #region IPointerDownHandler implementation

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        HandleDrag(eventData);
    }

    #endregion

    private void HandleDrag(PointerEventData eventData)
    {
        Vector2 pos = Vector2.zero;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / rect_.sizeDelta.x) * 2 - 1;
            pos.y = (pos.y / rect_.sizeDelta.y) * 2 - 1;

            inputDirection = new Vector2(pos.x, pos.y);
            inputDirection = inputDirection.magnitude > 1 ? inputDirection.normalized : inputDirection;

            joystick_.rectTransform.anchoredPosition = new Vector3(
                inputDirection.x * (rect_.sizeDelta.x / 3), inputDirection.y * (rect_.sizeDelta.y / 3), 0
            );
        }
    }

    private void Reset()
    {
        joystick_.rectTransform.anchoredPosition = Vector3.zero;
        inputDirection = Vector2.zero;
    }

    private void OnDisable()
    {
        Reset();
    }
}
