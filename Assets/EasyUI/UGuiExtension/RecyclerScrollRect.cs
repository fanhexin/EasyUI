using System;
using EasyUI.UGuiExtension.Extension;
using UniRx.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyUI.UGuiExtension
{
    public class RecyclerScrollRect : ScrollRect
    {
        public interface IAdapter
        {
            void OnBindView(int index, RectTransform view);
            int ItemCount { get; }
        }
        
        public IAdapter adapter { private get; set; }
        
        // todo header和footer不可见后disable掉
        [SerializeField] RectTransform _itemPrefab;
        [SerializeField] RectTransform _header;
        [SerializeField] RectTransform _footer;
        [SerializeField] float _spacing;
        
#if UNITY_EDITOR
        public SerializedProperty EditorGetItemPrefabSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_itemPrefab));
        }
        
        public SerializedProperty EditorGetHeaderSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_header));
        }
        
        public SerializedProperty EditorGetFooterSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_footer));
        }
        
        public SerializedProperty EditorGetSpacingSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_spacing));
        }
#endif

        readonly TopRecycler _topRecycler;
        readonly BottomRecycler _bottomRecycler;
        readonly ItemPool _itemPool;
        
        bool _isDragging;
        int _topItemIndex;

        Rect _viewWorldRect;
        int _capacityCnt;

        protected RecyclerScrollRect()
        {
            _topRecycler = new TopRecycler(this);
            _bottomRecycler = new BottomRecycler(this);
            _itemPool = new ItemPool(this);
        }

        protected override void Start()
        {
            // Ugui上start会被自动调用，做个Editor是否在运行的判断
            #if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            #endif
            
            if (vertical == horizontal)
            {
                throw new Exception("Must be vertical or horizontal mode!");
            }
            
            _viewWorldRect = viewport.GetWorldRect();
            SetContentSize();
            FillContent();
            base.Start();
        }

        #if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            vertical = true;
            horizontal = false;
        }
        #endif

        public void Reload()
        {
            // 无论之前滚动到哪，回到顶部
            if (vertical)
            {
                verticalNormalizedPosition = 1;
            }
            else
            {
                horizontalNormalizedPosition = 0;
            }

            ReturnItems();
            SetContentSize();
            FillContent();
        }

        void ReturnItems()
        {
            int startIndex = _header == null ? _capacityCnt - 1 : _capacityCnt;
            for (int i = startIndex; i >= 0; i--)
            {
                _itemPool.Return(content.GetChild(i));
            }
        }

        void SetContentSize()
        {
            float side = 0;
            
            if (_header != null)
            {
                side += GetRectSide(_header.rect);
            }

            if (_footer != null)
            {
                side += GetRectSide(_footer.rect);
            }

            side += adapter.ItemCount * GetRectSide(_itemPrefab.rect) + (adapter.ItemCount - 1) * _spacing;
            
            var contentSize = content.sizeDelta;
            if (vertical)
            {
                contentSize.y = side;
            }
            else
            {
                contentSize.x = side;
            }
            content.sizeDelta = contentSize;
        }

        void FillContent()
        {
            float viewSide = GetRectSide(viewRect.rect);
            float itemSide = GetRectSide(_itemPrefab.rect);

            _capacityCnt = Mathf.Min(Mathf.CeilToInt(viewSide / (itemSide + _spacing)) + 1, adapter.ItemCount);
            
            _topItemIndex = 0;
            
            for (int i = 0; i < _capacityCnt; i++)
            {
                RectTransform item = _itemPool.Rent().GetComponent<RectTransform>();
                item.anchoredPosition = GetItemPos(i);
                if (_footer != null)
                {
                    item.SetSiblingIndex(_footer.GetSiblingIndex());
                }
                adapter.OnBindView(i, item);
            }
        }

        Vector2 GetItemPos(int index)
        {
            float pos = 0;
            if (_header != null)
            {
                pos += GetRectSide(_header.rect);
            }

            pos = GetRectSide(content.rect) * 0.5f -
                   (pos + (index + 0.5f) * GetRectSide(_itemPrefab.rect) + _spacing * index);

            if (horizontal)
            {
                pos = -pos;
            }
            
            return vertical ? new Vector2(0, pos) : new Vector2(pos, 0);
        }

        float GetRectSide(Rect rect)
        {
            return vertical ? rect.height : rect.width;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _isDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _isDragging = false;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            
            if (!_isDragging && velocity == Vector2.zero)
            {
                return;
            }

            _topRecycler.RecycleItems();
            _bottomRecycler.RecycleItems();
        }
        
        abstract class Recycler 
        {
            protected readonly RecyclerScrollRect _scrollRect;

            protected Recycler(RecyclerScrollRect scrollRect)
            {
                _scrollRect = scrollRect;
            }                
            
            protected abstract void MoveItem(RectTransform item);
            protected abstract int sideIndex { get; set; }
            protected abstract int startIndex { get; }
            protected abstract int direction { get; }
            protected abstract bool isOutOfRange(int index);
            
            public void RecycleItems()
            {
                for (int i = 0; ; i++)
                {
                    var item = _scrollRect.content
                        .GetChild(startIndex + i * direction)
                        .GetComponent<RectTransform>();
                    
                    if (_scrollRect._viewWorldRect.Overlaps(item.GetWorldRect()))
                    {
                        return;
                    }

                    int index = sideIndex + direction;
                    if (isOutOfRange(index))
                    {
                        return;
                    }
                    
                    sideIndex = index;
                    item.anchoredPosition = _scrollRect.GetItemPos(sideIndex);
                    MoveItem(item);
                    _scrollRect.adapter.OnBindView(sideIndex, item);
                }
            }
        }

        class TopRecycler : Recycler
        {
            public TopRecycler(RecyclerScrollRect scrollRect) 
                : base(scrollRect)
            {
            }

            protected override void MoveItem(RectTransform item)
            {
                int index = _scrollRect.content.childCount - (_scrollRect._footer == null?1:2);
                item.SetSiblingIndex(index);
            }

            protected override int sideIndex
            {
                get => _scrollRect._topItemIndex + _scrollRect._capacityCnt - 1;
                set => _scrollRect._topItemIndex = value - _scrollRect._capacityCnt + 1;
            }

            protected override int startIndex => _scrollRect._header == null?0:1;
            protected override int direction => 1;
            protected override bool isOutOfRange(int index)
            {
                return index >= _scrollRect.adapter.ItemCount;
            }
        }

        class BottomRecycler : Recycler
        {
            public BottomRecycler(RecyclerScrollRect scrollRect) 
                : base(scrollRect)
            {
            }

            protected override void MoveItem(RectTransform item)
            {
                item.SetSiblingIndex(_scrollRect._header == null?0:1);
            }

            protected override int sideIndex
            {
                get => _scrollRect._topItemIndex;
                set => _scrollRect._topItemIndex = value;
            }

            protected override int startIndex => _scrollRect.content.childCount - (_scrollRect._footer == null?1:2);
            protected override int direction => -1;
            protected override bool isOutOfRange(int index)
            {
                return index < 0;
            }
        }

        class ItemPool : ObjectPool<Transform>
        {
            readonly RecyclerScrollRect _scrollRect;

            public ItemPool(RecyclerScrollRect scrollRect)
            {
                _scrollRect = scrollRect;
            }
            
            protected override Transform CreateInstance()
            {
                return Instantiate(_scrollRect._itemPrefab, _scrollRect.content);
            }

            protected override void OnBeforeRent(Transform instance)
            {
                instance.SetParent(_scrollRect.content);
                base.OnBeforeRent(instance);
            }

            protected override void OnBeforeReturn(Transform instance)
            {
                instance.SetParent(_scrollRect.viewport);
                base.OnBeforeReturn(instance);
            }
        }
    }
}