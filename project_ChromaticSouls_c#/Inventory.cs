
using System;
using System.Collections;
using System.Collections.Generic;

using Corgi;
using Corgi.Spec;

namespace Corgi.GameLogic
{
	public class Inventory
	{
		List<IInventoryItem> _inventoryItemList = new List<IInventoryItem>();

		public List<IInventoryItem> InventoryItemList { get { return _inventoryItemList; } }
		public int Count { get { return _inventoryItemList.Count; } }

		public Inventory()
		{
		}

		public bool Load(JSONObject json)
		{
			_inventoryItemList.Clear ();

			foreach(JSONObject jsonItem in CorgiJson.ParseArray(json, "inventory"))
			{
				InventoryItemType itemType = (InventoryItemType)CorgiSpecHelper.ParseEnum(typeof(InventoryItemType), CorgiJson.ParseString(jsonItem, "itemType"));
                IInventoryItem item = InventoryItemFactory.Create(itemType, jsonItem);

                if (item == null)
				{
					DebugUtils.Assert(false);
                    continue;
				}
				_inventoryItemList.Add(item);
			}

			return true;
		}

		public IInventoryItem GetInventoryItem(InventoryItemType itemType, string id)
		{
            return GetInventoryItem(id);
		}

        public IInventoryItem GetInventoryItem(string id)
        {
            foreach (IInventoryItem item in _inventoryItemList)
            {
                if (item == null)
                {
                    continue;
                }

                if (String.Equals(id, item.Id) == true)
                {
                    return item;
                }

            }
            return null;
        }

		public IInventoryItem GetInventoryItem(InventoryItemType itemType, long code)
		{
            foreach(IInventoryItem item in _inventoryItemList)
            {
                if(item.ItemType == itemType && item.Code == code)
                {
                    return item;
                }

            }
            return null;
		}

		public int GetInventoryItemCount(long code)
		{
			int cnt = 0;
			foreach(IInventoryItem item in _inventoryItemList)
			{
				if(item.Code == code)
				{
					cnt+=item.CurStack;
				}
				
			}
			return cnt;
		}

        public int GetInventoryItemCount(InventoryItemType itemType)
        {
            int cnt = 0;
            foreach (IInventoryItem item in _inventoryItemList)
            {
                if (item.ItemType == itemType)
                    cnt += item.CurStack;
            }
            return cnt;
        }

		public int GetInventoryItemCount(InventoryItemType itemType, long code)
		{
			int cnt = 0;
			foreach (IInventoryItem item in _inventoryItemList)
			{
				if (item != null)
				{
                    if (item.IsLocked)
                        continue;

					if (item.Code == code && item.ItemType == itemType)
					{
						cnt += item.CurStack;
					}
				}

			}
			return cnt;
		}

        public ItemChanges UpdateItemChanges(JSONObject itemChanges) 
        {
			List<JSONObject> AddItems = null;
			List<JSONObject> UpdateItems = null;
			List<string> DeleteItems = null;

			if(CorgiJson.IsValid(itemChanges, "add"))
			{
				AddItems = CorgiJson.ParseArray(itemChanges, "add");
			}
			if(CorgiJson.IsValid(itemChanges, "update"))
			{
                UpdateItems = CorgiJson.ParseArray(itemChanges, "update");
			}
			if(CorgiJson.IsValid(itemChanges, "delete"))
			{
                DeleteItems = CorgiJson.ParseArrayString(itemChanges, "delete");
			}

            DebugUtils.Log(AddItems);
            DebugUtils.Log(UpdateItems);
            DebugUtils.Log(DeleteItems);

            ItemChanges ret = new ItemChanges();
            ret.AddItems = new List<IInventoryItem>();
            ret.DeleteItems = new List<IInventoryItem>();
            ret.UpdateItems = new List<IInventoryItem>();
			ret.UpdateItemsStackDiff = new List<int>();
            ret.AddRelics = new List<IInventoryItem>();
            ret.UpdateRelics = new List<IInventoryItem>();
            ret.UpdateRelicsStackDiff = new List<int>();

            // Add Items
			if(AddItems != null)
			{
				foreach (JSONObject addItem in AddItems)
				{
					if (addItem == null)
					{
						continue;
					}

					InventoryItemType itemType = (InventoryItemType)CorgiSpecHelper.ParseEnum(typeof(InventoryItemType), CorgiJson.ParseString(addItem, "itemType"));

                    if (itemType == InventoryItemType.Relic)
                    {
                        Player.Instance.LoadRelic(addItem);
                        IInventoryItem item = InventoryItemFactory.Create(itemType, addItem);
                        if (item == null)
                        {
                            DebugUtils.Assert(false);
                        }
                        ret.AddRelics.Add(item);
                    }
                    else
                    {
                        IInventoryItem item = InventoryItemFactory.Create(itemType, addItem);

                        if (item == null)
                        {
                            DebugUtils.Assert(false);
                        }

                        //check new
                        item.IsNew = true;
                        _inventoryItemList.Add(item);
                        ret.AddItems.Add(item);
                    }
				}
			}

            // Delete Items
			if(DeleteItems != null)
			{
				foreach (string deleteItemId in DeleteItems)
				{
					if (deleteItemId == null)
					{
						continue;
					}

					IInventoryItem item = GetInventoryItem(deleteItemId);

					if(item == null)
					{
						DebugUtils.Assert(false);                    
					}
					
					//check new
					item.IsNew = true;
					_inventoryItemList.Remove(item);
					ret.DeleteItems.Add(item);
				}
			}

            // Update Items
			if(UpdateItems != null)
			{
				foreach (JSONObject updateItem in UpdateItems)
				{
					if (updateItem == null)
					{
						continue;
					}

                    InventoryItemType itemType = (InventoryItemType)CorgiSpecHelper.ParseEnum(typeof(InventoryItemType), CorgiJson.ParseString(updateItem, "itemType"));

                    if (itemType == InventoryItemType.Relic)
                    {
                        string relicId = CorgiJson.ParseString(updateItem, "relicId");
                        RelicItem existingRelicItem = Player.Instance.GetRelic(relicId);
                        if (existingRelicItem == null)
                        {
                            DebugUtils.Assert(false);
                            continue;
                        }

                        int prevStack = existingRelicItem.Stack;
                        RelicItem relicItem = Player.Instance.LoadRelic(updateItem);
                        if (relicItem == null)
                        {
                            DebugUtils.Assert(false);
                            continue;
                        }

                        int stackDiff = relicItem.Stack - prevStack;

                        ret.UpdateRelics.Add(relicItem);
                        ret.UpdateRelicsStackDiff.Add(stackDiff);
                    }
                    else
                    {
                        IInventoryItem updatedItem = GetInventoryItem(CorgiJson.ParseString(updateItem, "itemId"));
                        if (updatedItem == null)
                        {
                            DebugUtils.Assert(false);
                            continue;
                        }

                        int prevStack = updatedItem.CurStack;
                        if (updatedItem.InitObject(updateItem) == false)
                        {
                            DebugUtils.Assert(false);
                            continue;
                        }

                        int stackDiff = updatedItem.CurStack - prevStack;

                        //check new
                        if (updatedItem.IsNew)
                            updatedItem.IsNew = false;
                        ret.UpdateItems.Add(updatedItem);
                        ret.UpdateItemsStackDiff.Add(stackDiff);
                    }					
				}
			}

            return ret;
        }

		public bool IsEquippedItem(string id)
		{
			if (id != null)
			{
				foreach (GameLogic.CharacterItem charItem in Player.Instance.CharList)
				{
					if (charItem != null)
					{
						if (charItem.HasSkill(id) || charItem.HasEquip(id)) { return true; };
					}
				}
			}
			return false;
		}

		public IInventoryItem[] FindItems(long code)
		{
			List<IInventoryItem> findList = new List<IInventoryItem>();
			
			foreach(IInventoryItem item in _inventoryItemList)
			{
				if(item.Code == code)
					findList.Add(item);
			}
			
			if(findList.Count > 0)
				return findList.ToArray();
			
			return null;
		}
		
		public int GetCountSkillScrollsBySchoolType(SkillSchoolType type, SkillGradeType grade)
		{
			int count = 0;
			foreach(IInventoryItem item in _inventoryItemList)
			{
				if(item.ItemType == InventoryItemType.Skill)
				{
					SkillItem skillItem = item as SkillItem;
					if(skillItem.SchoolType == type && skillItem.SkillGrade == (int)grade)
						count++;
				}
			}
			return count;
		}
		
		public int GetCountSkillScrollsByPremium()
		{
			int count = 0;
			foreach(IInventoryItem item in _inventoryItemList)
			{
				if(item.ItemType == InventoryItemType.Skill)
				{
					SkillItem skillItem = item as SkillItem;
					if(skillItem.Premium > 0)
						count++;
				}
			}
			return count;
		}
	}

    public struct ItemChanges
    {
        public List<IInventoryItem> AddItems;
        public List<IInventoryItem> DeleteItems;
        public List<IInventoryItem> UpdateItems;
		public List<int> UpdateItemsStackDiff;
        public List<IInventoryItem> AddRelics;
        public List<IInventoryItem> UpdateRelics;
        public List<int> UpdateRelicsStackDiff;
    }
}
