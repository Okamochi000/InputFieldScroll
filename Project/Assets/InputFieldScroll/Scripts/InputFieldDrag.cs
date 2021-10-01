using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// InputFieldのドラッグ操作
/// </summary>
public class InputFieldDrag : MonoBehaviour, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    public bool IsDraging { get; private set; } = false;

    [SerializeField] private InputFieldScroll inputFieldScroll = null;

    /// <summary>
    /// ドラッグ操作中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (inputFieldScroll == null) { return; }
        inputFieldScroll.FitToScrollBar(InputFieldScroll.SeekType.ActivateDrawEnd);
        IsDraging = true;
    }

    /// <summary>
    /// ドラッグ操作終了
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        IsDraging = false;
    }

    /// <summary>
    /// ポインターが離れた時
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (inputFieldScroll == null) { return; }
        inputFieldScroll.FitToScrollBar(InputFieldScroll.SeekType.ActivateDrawEnd);
    }
}
