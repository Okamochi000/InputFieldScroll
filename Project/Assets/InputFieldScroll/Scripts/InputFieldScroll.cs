using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// スクロールバー対応したInputField
/// </summary>
public class InputFieldScroll : InputField
{
    public enum SeekType
    {
        None,               // シーク無し
        ActivateDrawStart,  // アクティブ状態で先頭にキャレット移動
        ActivateDrawEnd,    // アクティブ状態で最後尾にキャレット移動
    }

    private const int CONTENT_ADD_SIZE = 100;

    private ScrollRect scrollRect_ = null;
    private SeekType seekType_ = SeekType.None;
    private int seekIndex_ = 0;

#if UNITY_EDITOR || UNITY_STANDALONE
    private bool isSelected_ = false;
    private InputFieldDrag inputFieldDrag_ = null;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
    protected void Update()
    {
        if (!Application.isPlaying) { return; }
        if (GetInputFieldDrag() == null || GetInputFieldDrag().IsDraging) { return; }

        // スクロールバーの位置を更新する
        if (isSelected_) { UpdateScrollBar(); }
    }

    protected override void LateUpdate()
    {
        if (seekType_ == SeekType.None)
        {
            // 更新処理
            base.LateUpdate();
        }
        else
        {
            // アクティブ状態にする
            this.ActivateInputField();

            // 更新処理
            base.LateUpdate();

            // 描画開始位置設定
            m_DrawStart = GetLineCaretPosition(seekIndex_, true);

            // キャレット位置設定
            if (seekType_ == SeekType.ActivateDrawStart)
            {
                // キャレットを先頭に移動する
                caretPosition = GetLineCaretPosition(seekIndex_, true);
            }
            else
            {
                // キャレットを最後尾に移動する
                int lastLineIndex = seekIndex_ + textComponent.cachedTextGenerator.lineCount - 1;
                caretPosition = GetLineCaretPosition(lastLineIndex, false);
            }

            // 編集内容反映
            this.UpdateLabel();

            // シークパラメータを戻す
            seekIndex_ = 0;
            seekType_ = SeekType.None;
        }
    }

    /// <summary>
    /// フォーカスされた
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        isSelected_ = true;

        // スクロールバーの位置を更新する
        if (isSelected_)
        {
            GetInputFieldDrag();
            if (inputFieldDrag_ != null && !inputFieldDrag_.IsDraging) { UpdateScrollBar(); }
        }
    }

    /// <summary>
    /// フォーカスが外れた
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        isSelected_ = false;

        // スクロールバーの位置を更新する
        if (isSelected_) { UpdateScrollBar(); }
    }
#endif

    /// <summary>
    /// 指定行に移動する
    /// </summary>
    /// <param name="index"></param>
    /// <param name="seekType"></param>
    public void SeekLine(int index, SeekType seekType)
    {
        seekType_ = seekType;
        seekIndex_ = index;
    }
    
    /// <summary>
    /// スクロールバーに合わせて位置を調整する
    /// </summary>
    /// <param name="seekType"></param>
    public void FitToScrollBar(SeekType seekType)
    {
        ScrollRect scrollRect = GetScrollRect();
        int index = 0;
        if (scrollRect != null && IsScroll())
        {
            int overLineCount = cachedInputTextGenerator.lineCount - textComponent.cachedTextGenerator.lineCount + 1;
            index = (int)(scrollRect.verticalNormalizedPosition * (float)overLineCount);
            index = Mathf.Min((overLineCount - 1), index);
        }
        SeekLine(index, seekType);
    }

    /// <summary>
    /// スクロールが存在するか
    /// </summary>
    /// <returns></returns>
    public bool IsScroll()
    {
        if (m_DrawStart > 0) { return true; }
        if (m_DrawEnd < this.m_Text.Length) { return true; }
        return false;
    }

    /// <summary>
    /// キャレットが存在する行番号を取得する
    /// </summary>
    /// <param name="caretPos"></param>
    /// <returns></returns>
    public int GetCaretLineIndex(int caretPos)
    {
        if (caretPos == 0) { return 0; }

        int index = 0;
        foreach (UILineInfo uILineInfo in cachedInputTextGenerator.lines)
        {
            if (caretPos < uILineInfo.startCharIdx) { break; }
            index++;
        }
        index = Mathf.Min(index, (cachedInputTextGenerator.lines.Count - 1));
        index = Mathf.Max(0, index);

        return index;
    }

    /// <summary>
    /// 行のキャレット位置を取得する
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public int GetLineCaretPosition(int index, bool isStart)
    {
        // 範囲内に収める
        if (cachedInputTextGenerator.lineCount == 0) { return 0; }
        index = Mathf.Min(index, (cachedInputTextGenerator.lineCount - 1));
        index = Mathf.Max(0, index);

        // 最後尾のキャレット位置を取得
        if (!isStart)
        {
            if (index == (cachedInputTextGenerator.lineCount - 1)) { return m_Text.Length; }
            return (cachedInputTextGenerator.lines[(index + 1)].startCharIdx - 1);
        }

        // 先頭のキャレット位置を取得
        return cachedInputTextGenerator.lines[index].startCharIdx;
    }

    /// <summary>
    /// スクロールバーを更新する
    /// </summary>
    /// <returns></returns>
    private void UpdateScrollBar()
    {
        ScrollRect scrollRect = GetScrollRect();
        if (scrollRect != null)
        {
            scrollRect.gameObject.SetActive(IsScroll());

            Vector2 sizeDelta = scrollRect.content.sizeDelta;
            sizeDelta.y = preferredHeight + CONTENT_ADD_SIZE;
            scrollRect.content.sizeDelta = sizeDelta;
            if (IsScroll())
            {
                int drawLinePosition = GetCaretLineIndex(m_DrawStart);
                int overLineCount = cachedInputTextGenerator.lineCount - textComponent.cachedTextGenerator.lineCount + 1;
                scrollRect.verticalNormalizedPosition = (float)drawLinePosition / (float)overLineCount;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 0;
            }
        }
    }

    /// <summary>
    /// ScrollRectを取得する
    /// </summary>
    /// <returns></returns>
    private ScrollRect GetScrollRect()
    {
        if (scrollRect_ != null) { return scrollRect_; }

        scrollRect_ = this.GetComponentInChildren<ScrollRect>();
        return scrollRect_;
    }

    /// <summary>
    /// InputFieldDragを取得する
    /// </summary>
    /// <returns></returns>
    private InputFieldDrag GetInputFieldDrag()
    {
        if (inputFieldDrag_ != null) { return inputFieldDrag_; }

        inputFieldDrag_ = this.GetComponentInChildren<InputFieldDrag>();
        return inputFieldDrag_;
    }
}
