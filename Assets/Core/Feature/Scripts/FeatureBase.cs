using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class FeatureBase : MonoBehaviour
{
    [SerializeField] private FeatureType featureType;
    [Tooltip("Object scaled open/closed. Falls back to this transform when empty.")]
    [SerializeField] private Transform content;
    [SerializeField] private Button[] closeButtons;
    [SerializeField] private float duration = 0.25f;

    private CanvasGroup canvasGroup;
    private Sequence sequence;

    public FeatureType FeatureType => featureType;
    public bool IsShowing { get; private set; }

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (content == null) content = transform;

        foreach (Button button in closeButtons)
        {
            button.onClick.AddListener(Close);
        }
    }

    public virtual void Show()
    {
        if (IsShowing) return;

        IsShowing = true;
        gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        content.localScale = Vector3.zero;

        PlaySequence(1f, Ease.OutBack).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            OnShow();
        });
    }

    public virtual void Close()
    {
        if (!IsShowing) return;

        IsShowing = false;
        canvasGroup.interactable = false;
        OnClose();

        PlaySequence(0f, Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    protected virtual void OnShow() { }

    protected virtual void OnClose() { }

    private Sequence PlaySequence(float target, Ease scaleEase)
    {
        sequence?.Kill();
        sequence = DOTween.Sequence()
            .Join(canvasGroup.DOFade(target, duration))
            .Join(content.DOScale(target, duration).SetEase(scaleEase))
            .SetUpdate(true)
            .SetLink(gameObject);
        return sequence;
    }
}
