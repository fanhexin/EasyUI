using System;
using System.Collections.Generic;
using UniRx.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
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
        
        [SerializeField] RectTransform _itemPrefab;
        [SerializeField] RectTransform _header;
        [SerializeField] RectTransform _footer;
        [SerializeField] float _contentHeadPadding;
        [SerializeField] float _contentFootPadding;
        [SerializeField] float _spacing;
        [SerializeField] bool _loadOnStart = true;
        
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
        
        public SerializedProperty EditorGetLoadOnStartSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_loadOnStart));
        }
        
        public SerializedProperty EditorGetContentHeadPaddingSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_contentHeadPadding));
        }
        
        public SerializedProperty EditorGetContentFootPaddingSp(SerializedObject so)
        {
            return so.FindProperty(nameof(_contentFootPadding));
        }
#endif

        readonly ItemPool _itemPool;
        readonly LinkedList<(int index, RectTransform rectTrans)> _itemLinkedList = new LinkedList<(int, RectTransform)>();

        int _orientation;
        int _capacityCnt;
        float _lastNormalizedPos;
        int _direction;
        private bool _isHeaderExisting;
        private bool _isFooterExisting;

        protected RecyclerScrollRect()
        {
            _itemPool = new ItemPool(this);
        }

        protected override void Awake()
        {
            base.Awake();
            InitState();
            // 根据rider的提示，Unity的Object类型跟null比较时效率较低下，不应该在update中调用，从而做出的优化
            _isHeaderExisting = _header != null;
            _isFooterExisting = _footer != null;
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
            
            if (_loadOnStart)
            {
                Load();
            }
            base.Start();
        }

        void InitState()
        {
            _orientation = vertical ? 1 : 0;
            _direction = 2 * _orientation - 1;
            _lastNormalizedPos = _orientation;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            vertical = true;
            horizontal = false;
        }
#endif

        public void Load()
        {
            SetContentSize();
            FillContent();
        }
        
        public void Reload()
        {
            _lastNormalizedPos = _orientation;
            ReturnItems();
            Load();

            // 无论之前滚动到哪，回到顶部
            normalizedPosition = new Vector2{[_orientation] = _orientation};
        }

        void ReturnItems()
        {
            if (_capacityCnt == 0)
            {
                return;
            }
            
            foreach (var item in _itemLinkedList)
            {
                _itemPool.Return(item.rectTrans);    
            }
            
            _itemLinkedList.Clear();
        }

        void SetContentSize()
        {
            float side = _contentHeadPadding + _contentFootPadding;
            
            if (_isHeaderExisting)
            {
                side += GetRectSide(_header.rect);
            }

            if (_isFooterExisting)
            {
                side += GetRectSide(_footer.rect);
            }

            float spaceSize = (adapter.ItemCount - 1 + (_isHeaderExisting?1:0) + (_isFooterExisting?1:0)) * _spacing;
            side += adapter.ItemCount * GetRectSide(_itemPrefab.rect) + spaceSize;
            
            content.SetSizeWithCurrentAnchors((RectTransform.Axis) _orientation, side);
        }

        void FillContent()
        {
            float viewSide = GetRectSide(viewRect.rect);
            float itemSide = GetRectSide(_itemPrefab.rect);

            _capacityCnt = Mathf.Min(Mathf.CeilToInt(viewSide / (itemSide + _spacing)) + 1, adapter.ItemCount);
            
            for (int i = 0; i < _capacityCnt; i++)
            {
                RectTransform item = _itemPool.Rent().GetComponent<RectTransform>();
                item.anchoredPosition = GetItemPos(i);
                if (_isFooterExisting)
                {
                    item.SetSiblingIndex(_footer.GetSiblingIndex());
                }
                adapter.OnBindView(i, item);
                _itemLinkedList.AddLast((i, item));
            }
        }

        Vector2 GetItemPos(int index)
        {
            float pos = _contentHeadPadding;
            if (_isHeaderExisting)
            {
                pos += GetRectSide(_header.rect) + _spacing;
            }

            pos = _direction * (GetRectSide(content.rect) * 0.5f -
                   (pos + (index + 0.5f) * GetRectSide(_itemPrefab.rect) + _spacing * index));

            return new Vector2 {[_orientation] = pos};
        }

        Vector2Int GetItemIndex(in Vector2 pos)
        {
            float p = pos[_orientation] * _direction - _contentHeadPadding;
            if (_isHeaderExisting)
            {
                p -= _header.rect.size[_orientation] + _spacing;
            }

            float itemSide = _itemPrefab.rect.size[_orientation] + _spacing;
            int topIndex = Mathf.FloorToInt(Mathf.Max(p, 0) / itemSide);
            int bottomIndex = Mathf.FloorToInt((p + viewport.rect.size[_orientation]) / itemSide);
            bottomIndex = Mathf.Min(adapter.ItemCount - 1, bottomIndex);
            return new Vector2Int(topIndex, bottomIndex);
        }

        float GetRectSide(in Rect rect)
        {
            return rect.size[_orientation];
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            UpdateByNormalizedPosChange(value, axis);
        }

        void UpdateByNormalizedPosChange(float value, int axis)
        {
            if (_capacityCnt == 0)
            {
                return;
            }

            if (axis != _orientation)
            {
                return;
            }

            float delta = value - _lastNormalizedPos;
            _lastNormalizedPos = value;

            if (velocity != Vector2.zero || Mathf.Approximately(delta, 0))
            {
                return;
            }

            UpdateHeaderFooterActiveState(content.anchoredPosition);
            var index = GetItemIndex(content.anchoredPosition);
            if (delta * _direction < 0)
            {
                RecycleTopItems(index);
            }
            else 
            {
                RecycleBottomItems(index);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            
            if (_capacityCnt == 0)
            {
                return;
            }

            if (velocity == Vector2.zero)
            {
                return;
            }
            
            UpdateHeaderFooterActiveState(content.anchoredPosition);
            var index = GetItemIndex(content.anchoredPosition);
            if (velocity[_orientation] * _direction > 0)
            {
                RecycleTopItems(index);
            }
            else
            {
                RecycleBottomItems(index);
            }
        }

        void RecycleTopItems(in Vector2Int index)
        {
            int topIndex = index.x;
            var item = _itemLinkedList.First;
            if (topIndex > _itemLinkedList.Last.Value.index)
            {
                int offset = Mathf.Max(0, topIndex + _capacityCnt - adapter.ItemCount);
                topIndex -= offset;
                
                while (item != null && topIndex < adapter.ItemCount)
                {
                    var v = item.Value;
                    v.rectTrans.anchoredPosition = GetItemPos(topIndex);
                    adapter.OnBindView(topIndex, v.rectTrans);
                    item.Value = (topIndex, v.rectTrans);
                    ++topIndex;
                    item = item.Next;
                }
                return;
            }

            int bottomIndex = _itemLinkedList.Last.Value.index;
            while (item != null && item.Value.index < topIndex && bottomIndex < adapter.ItemCount - 1)
            {
                var v = item.Value;
                v.rectTrans.anchoredPosition = GetItemPos(++bottomIndex);
                adapter.OnBindView(bottomIndex, v.rectTrans);
                _itemLinkedList.RemoveFirst();
                _itemLinkedList.AddLast((bottomIndex, v.rectTrans));
                item = _itemLinkedList.First;
            }
        }

        void RecycleBottomItems(in Vector2Int index)
        {
             int bottomIndex = index.y;
             var item = _itemLinkedList.Last;
             if (bottomIndex < _itemLinkedList.First.Value.index)
             {
                 int offset = Mathf.Max(0, _capacityCnt - bottomIndex - 1);
                 bottomIndex += offset;
                 
                 while (item != null && bottomIndex >= 0)
                 {
                     var v = item.Value;
                     v.rectTrans.anchoredPosition = GetItemPos(bottomIndex);
                     adapter.OnBindView(bottomIndex, v.rectTrans);
                     item.Value = (bottomIndex, v.rectTrans);
                     --bottomIndex;
                     item = item.Previous;
                 }
                 return;
             }
 
             int topIndex = _itemLinkedList.First.Value.index;
             while (item != null && item.Value.index > bottomIndex && topIndex > 0)
             {
                 var v = item.Value;
                 v.rectTrans.anchoredPosition = GetItemPos(--topIndex);
                 adapter.OnBindView(topIndex, v.rectTrans);
                 _itemLinkedList.RemoveLast();
                 _itemLinkedList.AddFirst((topIndex, v.rectTrans));
                 item = _itemLinkedList.Last;
             }           
        }

        void UpdateHeaderFooterActiveState(in Vector2 pos)
        {
            float len = pos[_orientation];
            if (_isHeaderExisting) 
                _header.gameObject.SetActive(len < GetRectSide(_header.rect) + _contentHeadPadding);
            
            if (_isFooterExisting)
                _footer.gameObject.SetActive(GetRectSide(content.rect) - len - GetRectSide(viewport.rect)
                                              < GetRectSide(_footer.rect) + _contentFootPadding);
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
                base.OnBeforeReturn(instance);
                instance.SetParent(_scrollRect.viewport);
            }
        }
    }
}