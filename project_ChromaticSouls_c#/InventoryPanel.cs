using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Corgi.GameLogic;

namespace Corgi.UI
{
    public class InventoryPanel : PanelBase, IOptionObserver
	{		
		public delegate void OnInitilizeSlotCallback(GameObject go);
		public OnInitilizeSlotCallback onInitilizeSlot; 
		
		public UIScrollView scrollview;
		public UIGrid		grid;
		public SlotGroup 	baseSlot;
		public int 			maxSlotGroup = 7;
		
		List<SlotGroup> 	_slotGroupList = new List<SlotGroup>();
		InventorySortType	_sortType = InventorySortType.All;
		UIWrapContent 		_wrapContent = null;
		int inventorySlotNum = 0;
		
		protected List<IInventoryItem> itemList = new List<IInventoryItem>();
		protected Dictionary<InventorySortType, List<IInventoryItem>> cachedInventoryItemDictionary = new Dictionary<InventorySortType, List<IInventoryItem>>();
		protected List<IInventoryItem>	cachedInventoryItemList = null;
		
		//seleted mode
		bool _isSelectMode = false;
		int _maxSelectNum = 0;
		int _currentSelectCount = 0;
		List<IInventoryItem> _selectedItemList = new List<IInventoryItem>();
		
		//filter mode
		CharacterItem _cachedCharacterItem = null;
		CharacterType _filterCharType = CharacterType.All;
		bool _isFilterCanEquip = false;
        EquipType _filterEquipType = EquipType.Weapon; 
		
		InventorySelectModeType _selectModeType = InventorySelectModeType.None;
		public List<IInventoryItem> SelectedItemList { get { return _selectedItemList; } }
		
		public InventorySortType SortType { get { return _sortType; } }
		public ItemSortType selectUserSortType = ItemSortType.Lock;

        public EquipItem selectedEquipEnhanceItem;

		public UILabel EmptyLabel;

		void OnDestroy()
		{
			if (itemList != null) { itemList.Clear(); }
			if (cachedInventoryItemDictionary != null) { cachedInventoryItemDictionary.Clear(); }
			if (cachedInventoryItemList != null) { cachedInventoryItemList.Clear(); }
			if (_selectedItemList != null) { _selectedItemList.Clear(); }
			_cachedCharacterItem = null;			

			if(null!=OptionManager.Instance)
				OptionManager.Instance.Detach(this);
		}
		
		public bool ActiveEmptyBG {
			set {
				if(EmptyLabel != null)
					EmptyLabel.alpha = value ? 1f : 0f;
			}
		}

		public void StartSelectMode(InventorySelectModeType type, int maxSelectNum)
		{
			_selectModeType = type;
			
			_isSelectMode = true;
			_maxSelectNum = maxSelectNum;
			
			_selectedItemList.Clear();
			
			//disable slots
			foreach(SlotGroup slotGroup in _slotGroupList)
			{
				foreach(InventorySlot slot in slotGroup.InventorySlots)
				{
					if(slot.InventoryItem == null)
						continue;
					
					bool isEnable = true;
					
					if(slot.InventoryItem.IsLocked)
						isEnable = false;
					
					if(type == InventorySelectModeType.Fuse)
					{
                        if (!slot.InventoryItem.CanFuse())
							isEnable = false;
					}

                    if(type == InventorySelectModeType.Sell)
                    {
                        if (!slot.InventoryItem.CanSell())
                            isEnable = false;
                    }
					
					slot.IsEnabled = isEnable;
				}
			}
		}
		public void ResetSelectMode()
		{
			_selectedItemList.Clear();
			
			_currentSelectCount = 0;
		}
       
		public void EndSelectMode()
		{
			_selectModeType = InventorySelectModeType.None;
			_isSelectMode = false;
			_maxSelectNum = 0;

			ResetSelectMode();

             foreach (SlotGroup slotGroup in _slotGroupList)
             {
                 InventorySlot[] slots = slotGroup.InventorySlots;
                 foreach (InventorySlot slot in slots)
                 {
                     slot.IsSelected = false;
                     slot.IsEnabled = true;
                 }
             }
		}

		public void UpdateOptionChange()
		{
			foreach (SlotGroup slotGroup in _slotGroupList)
			{
				InventorySlot[] slots = slotGroup.InventorySlots;
				foreach (InventorySlot slot in slots)
				{
					if (slot != null)
					{
						slot.SetDisplayName(true);
					}
				}
			}
		}

		protected override void OnAwake ()
		{
			base.OnAwake ();

			OptionManager.Instance.Attach(this);

			if(baseSlot != null)
				baseSlot.gameObject.SetActive(false);
			
			//create slotGroups
			for(int i=0; i < maxSlotGroup; i++)
			{
				GameObject slotGo = GameObject.Instantiate(baseSlot.gameObject) as GameObject;
				slotGo.transform.parent = grid.transform;
				slotGo.transform.localPosition = Vector3.zero;
				slotGo.transform.localScale = Vector3.one;
				slotGo.SetActive(true);
				
				SlotGroup slotGroup = slotGo.GetComponent<SlotGroup>();
                if (slotGroup == null) { continue; }

				slotGroup.Index = i;
				InventorySlot[] slots = slotGroup.InventorySlots;
				foreach(InventorySlot slot in slots)
				{
					slot.SetOwner(gameObject);
					slot.Clear();
				}
				inventorySlotNum = slots.Length;
				
				_slotGroupList.Add(slotGroup);

                TutorialObjectTag[] tags = slotGroup.GetComponentsInChildren<TutorialObjectTag>();
                if (tags != null)
                {
                    for (int j = 0 ; j < tags.Length; j ++)
                    {
                        if (tags[j] != null && tags[j].HasTutorialTag(CorgiTutorialTag.EQUIPDIALOG_INVEN_SLOT_PREFIX))
                        {
                            tags[j].SetTutorialTag(CorgiTutorialTag.EQUIPDIALOG_INVEN_SLOT_PREFIX, i);
                        } 
                    }
                }

				//call eventFunc
				if(onInitilizeSlot != null)
					onInitilizeSlot(slotGo);
			}
			DestroyImmediate(baseSlot.gameObject);
		
			//set wrapContent event
			_wrapContent = GetComponentInChildren<UIWrapContent>();
			if(_wrapContent != null)
				_wrapContent.onInitializeItem = OnWrapContentInitializeItem;
			
			//create empty items
			for(int i=0; i<Player.Instance.InventoryMaxCount; i++)
				itemList.Add(null);
			
			// cached item list by sorttype
			foreach (InventorySortType sortType in Enum.GetValues(typeof(InventorySortType)))
			{
				List<IInventoryItem> invenItemList = new List<IInventoryItem>();
				cachedInventoryItemDictionary.Add(sortType, invenItemList);
			}
			
			if(scrollview == null)
				scrollview = GetComponentInChildren<UIScrollView>();
		}

		bool IsValidItemForSortType(InventorySortType sortType, InventoryItemType itemType)
		{
			switch (sortType)
			{
				case InventorySortType.Equip : 		
					if (itemType == InventoryItemType.Equip) { return true; }
					break;
				case InventorySortType.Skill : 
					if (itemType == InventoryItemType.Skill) { return true; }
					break;
				case InventorySortType.ETC:
					if (itemType != InventoryItemType.Equip && itemType != InventoryItemType.Skill) { return true; }
					break;
				case InventorySortType.All:
					return true;
			}
			return false;
		}
		
		public void SetFilterEnableCharacterOnly(CharacterType type, EquipType equipType)
		{
            _filterEquipType = equipType;
			_isFilterCanEquip = true;
			_filterCharType = type;
		}

        public void SetFilterEnableCharacterOnly(CharacterType type)
        {
            _isFilterCanEquip = true;
            _filterCharType = type;
        }
		
		public void CacheInventoryItem(CharacterItem charItem)
		{
			foreach (InventorySortType sortType in Enum.GetValues(typeof(InventorySortType)))
			{
				if (!cachedInventoryItemDictionary.ContainsKey(sortType)) { continue; }

				List<IInventoryItem> invenItemList = cachedInventoryItemDictionary[sortType];
				if (invenItemList != null)
				{
					invenItemList.Clear();
					foreach (IInventoryItem inventoryItem in Player.Instance.InventoryItemList)
					{
						if(inventoryItem == null)
							continue;
						
						if(IsValidItemForSortType(sortType, inventoryItem.ItemType) == false)
							continue;
						
						invenItemList.Add(inventoryItem);
					}
				}
			}
		}
		
		void SortInventory()
		{
			foreach (InventorySortType sortType in Enum.GetValues(typeof(InventorySortType)))
			{
				if (!cachedInventoryItemDictionary.ContainsKey(sortType)) { continue; }

				List<IInventoryItem> invenItemList = cachedInventoryItemDictionary[sortType];
				if (invenItemList == null)
					continue;
				
				//sort
				//InventoryItemHelper.Sort(selectUserSortType, invenItemList);
                InventorySortingManager.Instance.Sort(_sortType, selectUserSortType, invenItemList);
			}
		}
		
		public void SetInventory(InventorySortType sortType, CharacterItem charItem)
		{
			_sortType = sortType;
			
			SetInventory(charItem);
		}

        public bool FindSeason2Equip(EquipType selectedEquipType, bool shouldEquipable)
        {
            if (itemList == null) { return false; }
            for (int i = 0; i < itemList.Count; i++)
            {
                IInventoryItem item = itemList[i];
                if (item != null)
                {
                    if (item.ItemType != InventoryItemType.Equip) { continue; }
                    if (!item.IsNew) { continue; }

                    EquipItem equipItem = item as EquipItem;
                    if (equipItem != null)
                    {
                        if (shouldEquipable)
                        {
                            if (equipItem.IsBinding) { continue; }
                        }

                        if (equipItem.IsSeason2 && equipItem.EquipType == selectedEquipType)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
		
		void OnDisable()
		{
			StopCoroutine("ResetPosition");
		}
		
		IEnumerator ResetPosition()
		{
			Hide(true);
			yield return 0;
			
			scrollview.transform.localPosition = Vector3.zero;
			scrollview.GetComponent<UIPanel>().clipOffset = Vector2.zero;
			yield return 0;
			scrollview.ResetPosition();
			yield return 0;
			
			_wrapContent.mFirstTime = true;
			_wrapContent.ResetChildPositions();
			yield return 0;
			_wrapContent.WrapContent();
			yield return 0;
			
			while(System.Math.Abs(scrollview.transform.localPosition.y) > 50f)
			{
				scrollview.transform.localPosition = Vector3.zero;
				scrollview.GetComponent<UIPanel>().clipOffset = Vector2.zero;
				scrollview.ResetPosition();
				yield return 0;
			}
		
			Hide(false);
            if (owner != null)
            {
                owner.SendMessage("OnUpdateCompleteInventoryPanel", SendMessageOptions.DontRequireReceiver);
            }
		}
		
		void SetWrapMinMax()
		{
            if(_wrapContent != null)
            {
                _wrapContent.maxIndex = 0;
                _wrapContent.minIndex = (itemList.Count / inventorySlotNum);
                _wrapContent.minIndex *= -1;

                if (_wrapContent.minIndex == 0)
                    _wrapContent.minIndex = -1;
            }
		}
		
		public void AddInventorySlot(int slotNum)
		{
			if(Player.Instance.IsInventoryFull())
				return;
			if(_sortType == InventorySortType.All)
			{
				for(int i=0; i<slotNum; i++)
					itemList.Add(null);
				
				SetWrapMinMax();
			}
		}

        public void SetEquipEnhanceS2Inven()
        {
            //reset slots
            foreach (SlotGroup slotGroup in _slotGroupList)
            {
                InventorySlot[] slots = slotGroup.InventorySlots;
                foreach (InventorySlot slot in slots)
                {
                    if (slot != null)
                    {
                        slot.Clear();
                        slot.gameObject.SetActive(false);
                    }
                }
            }

            itemList.Clear();
            itemList = Player.Instance.GetInventoryItemListByType(InventoryItemType.FortuneEquip);

            cachedInventoryItemList = Player.Instance.GetInventoryItemListByType(InventoryItemType.Equip);

            foreach (IInventoryItem item in cachedInventoryItemList)
            {
                if (item == null) { continue; }
                if (selectedEquipEnhanceItem.Id == item.Id)
                    continue;

                if (item.IsLocked)
                    continue;

                if (item.ItemType != InventoryItemType.Equip)
                    continue;

                if (Player.Instance.IsEquippedItem(item.Id))
                    continue;

                EquipItem tempItem = item as EquipItem;

                if (tempItem.EquipType == EquipType.Trinket)
                    continue;

                if (selectedEquipEnhanceItem != null)
                {
                    if (selectedEquipEnhanceItem.EquipTier != tempItem.EquipTier)
                        continue;

                    if (selectedEquipEnhanceItem.EquipGrade != tempItem.EquipGrade)
                        continue;
                }

                itemList.Add(item);
            }

            if (selectedEquipEnhanceItem != null)
            {
                InventorySortingManager.Instance.SortEquipEnhanceS2Exp(selectedEquipEnhanceItem.Code, itemList);
            }
            PushItems();
        }

		public void SetInventory(CharacterItem charItem)
		{			
			if(!gameObject.activeInHierarchy)
				return;
			
			_cachedCharacterItem = charItem;
			
			//collect inventory items
			if (cachedInventoryItemList == null) 
			{ 
				CacheInventoryItem(charItem); 
			}
			else
			{
				int cachedItemCount = 0;
				List<IInventoryItem> invenItemList = cachedInventoryItemDictionary[InventorySortType.All];
				if (invenItemList != null)
				{
					cachedItemCount += invenItemList.Count;
				}
				
				if(Player.Instance.InventoryCount != cachedItemCount)
					CacheInventoryItem(charItem);
			}
			
			//sorting
			SortInventory();

			if (!cachedInventoryItemDictionary.ContainsKey(_sortType)) 
				return; 
			cachedInventoryItemList = cachedInventoryItemDictionary[_sortType];
			
			//reset slots
			foreach(SlotGroup slotGroup in _slotGroupList)
			{
				InventorySlot[] slots = slotGroup.InventorySlots;
				foreach(InventorySlot slot in slots)
				{
					slot.Clear();
                    slot.gameObject.SetActive(false);
                    /*
					if(_sortType == InventorySortType.All)
						slot.gameObject.SetActive(true);
					else
						slot.gameObject.SetActive(false);
                    */
				}
			}
			
			//check showing emptyslots
			itemList.Clear();
			if(_sortType == InventorySortType.All)
			{
				//copy items
				foreach(IInventoryItem item in cachedInventoryItemList)
				{
					if(_selectModeType == InventorySelectModeType.Fuse)
					{
						if(item.IsLocked)
							continue;
                        if (!item.CanFuse())
							continue;
						if(Player.Instance.IsEquippedItem(item.Id))
							continue;
					}
					else if(_selectModeType == InventorySelectModeType.Sell)
					{
						if(item.IsLocked)
							continue;
						if(Player.Instance.IsEquippedItem(item.Id))
							continue;
                        if (!item.CanSell())
                            continue;
					}
					
					itemList.Add(item);
				}
				
				//adding emptyslot
                /*
				for(int i=cachedInventoryItemList.Count; i<Player.Instance.InventoryMaxCount; i++)
					itemList.Add(null);
                 */
			}
			else
			{
				//copy items
				foreach(IInventoryItem item in cachedInventoryItemList)
				{
					//filter
					if(_selectModeType == InventorySelectModeType.SkillEnhance)
					{
						if(item.IsLocked)
							continue;
					}
					else if(_selectModeType == InventorySelectModeType.Fuse)
					{
						if(item.IsLocked)
							continue;
						if(Player.Instance.IsEquippedItem(item.Id))
							continue;
					}
					else if(_selectModeType == InventorySelectModeType.Sell)
					{
						if(item.IsLocked)
							continue;
                        if (!item.CanSell())
                            continue;
						if(Player.Instance.IsEquippedItem(item.Id))
							continue;
					}
                    else if(_selectModeType == InventorySelectModeType.EquipEnhance)
                    {
                        if (selectedEquipEnhanceItem.Id == item.Id)
                            continue;

                        if (item.IsLocked)
                            continue;

                        if (Player.Instance.IsEquippedItem(item.Id))
                            continue;

                        EquipItem tempItem = item as EquipItem;

                        if(selectedEquipEnhanceItem != null)
                        {
                            if (selectedEquipEnhanceItem.EquipTier != tempItem.EquipTier)
                                continue;

                            if (selectedEquipEnhanceItem.EquipGrade != tempItem.EquipGrade)
                                continue;
                        }
                    }
					
					if(_isFilterCanEquip)
					{
						if(_sortType == InventorySortType.Equip)
						{
							EquipItem equipItem = item as EquipItem;
	
	                        //skip not can equip
	                        if(charItem != null)
							{
								//skip my equped
								if(charItem.IsEquipped(equipItem))
									continue;

                                //skip wrong type
                                if (equipItem.EquipType != _filterEquipType)
                                    continue;
								
								//check class
								if(equipItem.IsCanEquipByCharType(charItem) == false)
									continue;
							}
						}
						else if(_sortType == InventorySortType.Skill)
						{
							SkillItem skillItem = item as SkillItem;
    							
							if(charItem != null && skillItem.CanEquip(charItem) == false)
								continue;

                             skillItem.CheckSkillLearnStatus(charItem);
						}
					}
					
					itemList.Add(item);
				}
			}
            PushItems();
		}

        void PushItems()
        {
            //push items
            if (itemList.Count > 0)
            {
                int itemIndex = 0;
                foreach (SlotGroup slotGroup in _slotGroupList)
                {
                    InventorySlot[] slots = slotGroup.InventorySlots;

                    foreach (InventorySlot slot in slots)
                    {
                        IInventoryItem item = itemList[itemIndex++];
                        if (item != null)
                        {
                            slot.gameObject.SetActive(true);
                            slot.ShowItemName = true;
                            slot.SetItem(item);

                            if (_cachedCharacterItem != null)
                            {
                                EquipItem equipItem = item as EquipItem;
                                if (equipItem != null)
                                {
                                    bool isBindingWithID = equipItem.IsBinding && (equipItem.BindingId != null);
                                    slot.SetBinding(isBindingWithID, _cachedCharacterItem.IsEquipBinding(equipItem.Id));
                                }
                            }

                            if (_selectModeType == InventorySelectModeType.SkillEnhance && item.ItemType == InventoryItemType.Skill)
                            {
                                SkillItem skillItem = item as SkillItem;
                                if (skillItem != null)
                                {
                                    if (slot.IndicatorCoverSprite != null)
                                    {
                                        if (skillItem.SchoolType == InventorySortingManager.Instance.TargetSkillSchool)
                                        {
                                            if (!LeanTween.isTweening(slot.IndicatorCoverSprite.gameObject))
                                            {
                                                LeanTween.value(slot.IndicatorCoverSprite.gameObject, slot.SetIndicatorCoverAlpha, 0.05f, 0.25f, 0.8f).setRepeat(-1).setLoopPingPong();
                                            }
                                        }
                                        else
                                        {
                                            LeanTween.cancel(slot.IndicatorCoverSprite.gameObject);
                                            slot.SetIndicatorCoverAlpha(0f);
                                        }
                                    }
                                }
                            }

                            /*
                            if(_isFilterCanEquip && item.ItemType == InventoryItemType.Equip)
                            {
                                EquipItem equipItem = item as EquipItem;
                                if(equipItem.CanEquip(charItem) == false)
                                {
                                    slot.IsEnabled = false;
                                }
                            }
                            else if(item.ItemType == InventoryItemType.Equip)
                            {
                                if(Player.Instance.IsEquippedItem(item.Id))
                                {
                                    slot.IsEnabled = false;
                                }
                            }
                            */
                        }

                        if (itemList.Count <= itemIndex)
                            break;
                    }

                    if (itemList.Count <= itemIndex)
                        break;
                }
            }

            ActiveEmptyBG = itemList.Count == 0;

            SetWrapMinMax();

            StopCoroutine("ResetPosition");
            StartCoroutine("ResetPosition");
        }
		
		void OnWrapContentInitializeItem (GameObject go, int wrapIndex, int realIndex)
		{	
			SlotGroup slotGroupSender = go.GetComponent<SlotGroup>();
			SlotGroup slotGroup = _slotGroupList[wrapIndex];
			
			//reset slot
			InventorySlot[] slots = slotGroupSender.InventorySlots;	
			foreach(InventorySlot slot in slots)
				slot.Clear();
			
			//get real itemIndex
			int itemIndex = System.Math.Abs(realIndex) * slots.Length;
			
			if(itemList.Count <= itemIndex)
			{
				if(_sortType != InventorySortType.All || Player.Instance.IsInventoryFull())
				{
					foreach(InventorySlot slot in slots)
					{
						slot.gameObject.SetActive(false);
					}
				}
				return;
			}
			
			int slotNum = slots.Length;
			int setSlotIndex = 0;
			foreach(InventorySlot slot in slots)
			{			
				IInventoryItem item = itemList[itemIndex++];
				if(item != null)
				{
					setSlotIndex++;
					slot.gameObject.SetActive(true);
                    slot.ShowItemName = true;
					slot.SetItem(item);
					
					//check select
					foreach(IInventoryItem selectedItem in _selectedItemList)
					{
						if(item == selectedItem)
						{
							slot.IsSelected = true;
							break;
						}
					}
					
					//check disable
					bool isEnable = true;
					if( _selectModeType != InventorySelectModeType.None)
					{
						if(item.IsLocked)
							isEnable = false;
						if(_selectModeType == InventorySelectModeType.Fuse)
						{
                            if (!item.CanFuse())
								isEnable = false;
						}
					}

                    if (_cachedCharacterItem != null)
                    {
                        EquipItem equipItem = item as EquipItem;
                        if (equipItem != null)
                        {
                            bool isBindingWithID = equipItem.IsBinding && (equipItem.BindingId != null);
                            slot.SetBinding(isBindingWithID, _cachedCharacterItem.IsEquipBinding(equipItem.Id));
                        }
                    }

					if (_selectModeType == InventorySelectModeType.SkillEnhance && item.ItemType == InventoryItemType.Skill)
					{
						SkillItem skillItem = item as SkillItem;
						if (skillItem != null)
						{
							if (slot.IndicatorCoverSprite != null)
							{
                                if (skillItem.SchoolType == InventorySortingManager.Instance.TargetSkillSchool)
								{
									if (!LeanTween.isTweening(slot.IndicatorCoverSprite.gameObject))
									{
										LeanTween.value(slot.IndicatorCoverSprite.gameObject, slot.SetIndicatorCoverAlpha, 0.05f, 0.25f, 0.8f).setRepeat(-1).setLoopPingPong();
									}
								}
								else
								{
									LeanTween.cancel(slot.IndicatorCoverSprite.gameObject);
									slot.SetIndicatorCoverAlpha(0f);
								}
							}
						}
					}

					//check can equip
					/*
					if(_isFilterCanEquip && item.ItemType == InventoryItemType.Equip)
					{
						EquipItem equipItem = item as EquipItem;
						if(equipItem.CanEquip(_cachedCharacterItem) == false)
						{
							isEnable = false;
						}
					}
					else if(item.ItemType == InventoryItemType.Equip)
					{
						if(Player.Instance.IsEquippedItem(item.Id))
						{
							isEnable = false;
						}
					}
					*/
					slot.IsEnabled = isEnable;					
				}
				
				if(itemList.Count <= itemIndex)
				{
					if(_sortType != InventorySortType.All)
					{
						for(int i=setSlotIndex; i<slotNum; i++)
						{
							slots[i].gameObject.SetActive(false);
						}
					}
					return;
				}
			}
		}
		
		//from inventorySlot
		void OnSelectionSlot(GameObject sender)
		{			
			if(sender.GetComponent<InventorySlot>() != null)
			{
				InventorySlot slot = sender.GetComponent<InventorySlot>();
				if(slot.InventoryItem == null) //empty slot skip
					return;
				
				if(slot.IsSelected == true)
				{
					slot.IsSelected = false;
					_currentSelectCount--;
					
					foreach(IInventoryItem selectedItem in _selectedItemList)
					{
						if(slot.InventoryItem == selectedItem)
						{
							_selectedItemList.Remove(selectedItem);
							break;
						}
					}
				}
				else
				{
					if (_selectModeType == InventorySelectModeType.SkillEnhance)
					{
						SkillEnhanceDialog skillEnhanceDialog = GamePhaseManager.Instance.GetSkillEnhanceDialog();
						if (skillEnhanceDialog != null && skillEnhanceDialog.IsExpectedLevelMax) { return; }
					}

                    if (_selectModeType == InventorySelectModeType.EquipEnhance)
                    {
                        EquipEnhanceDialogS2 equipEnhanceDialog = GamePhaseManager.Instance.GetEquipEnhanceDialogS2();
                        if (equipEnhanceDialog != null && equipEnhanceDialog.IsExpectedLevelMax) { return; }
                    }

					if(_currentSelectCount < _maxSelectNum)
					{	
						if(_selectedItemList.Contains(slot.InventoryItem))
							return;
						
						slot.IsSelected = true;
						_currentSelectCount++;
						
						_selectedItemList.Add(slot.InventoryItem);
					}
				}
			}
			
			if(owner != null)
				owner.SendMessage("OnSelectionSlot", sender, SendMessageOptions.DontRequireReceiver);
		}
		
		//from inventorySlot
		void OnClickSlot(GameObject sender)
		{
			if(_isSelectMode)
			{
				OnSelectionSlot(sender);
			}
			else
			{
				if(owner != null)
					owner.SendMessage("OnClickSlot", sender, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
