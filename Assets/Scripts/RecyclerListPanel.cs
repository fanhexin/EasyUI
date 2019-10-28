using EasyUI;
using EasyUI.UGuiExtension;
using UnityEngine;
using UnityEngine.UI;

public class RecyclerListPanel : UIPanel, RecyclerScrollRect.IAdapter
{
    [SerializeField] RecyclerScrollRect _scrollRect;
    [SerializeField] int _itemCount = 50;

    protected override void Awake()
    {
        base.Awake();
        _scrollRect.adapter = this;
    }

    void RecyclerScrollRect.IAdapter.OnBindView(int index, RectTransform view)
    {
        view.GetComponentInChildren<Text>().text = $"item_{index}";
        view.name = $"item_{index}";
    }

    int RecyclerScrollRect.IAdapter.ItemCount => _itemCount;
}
