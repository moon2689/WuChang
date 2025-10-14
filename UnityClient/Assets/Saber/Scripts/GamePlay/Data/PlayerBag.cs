using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saber.CharacterController;
using Saber.Config;
using Saber.Frame;

namespace Saber
{
    public class PlayerBag
    {
        public class Item
        {
            public PropItemInfo Config;
            public int Count;

            public int ID => Config.m_ID;

            public Item(int id, int count)
            {
                Config = GameApp.Entry.Config.PropInfo.m_Props.FirstOrDefault(a => a.m_ID == id);
                Count = count;
            }
        }

        private static PlayerBag s_Instance;
        public static PlayerBag Instance => s_Instance ??= new();

        private List<Item> m_Items = new();

        public List<Item> Items => m_Items;


        private PlayerBag()
        {
            var savedItems = GameApp.Entry.Game.ProgressMgr.Items;
            foreach (var a in savedItems)
            {
                AddItem(a.m_ID, a.m_Count);
            }
        }

        public Item GetItemByID(int id)
        {
            return Items.FirstOrDefault(a => a.ID == id);
        }

        public int CalcItemCount(int id)
        {
            Item item = GetItemByID(id);
            return item != null ? item.Count : 0;
        }

        public Item GetItemByIndex(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                return Items[index];
            }

            return null;
        }

        public void AddItem(int id, int count)
        {
            if (count <= 0)
            {
                return;
            }

            Item item = GetItemByID(id);
            if (item != null)
            {
                item.Count += count;
            }
            else
            {
                item = new(id, count);
                m_Items.Add(item);
            }
        }

        public bool UseItem(int id)
        {
            Item item = GetItemByID(id);
            if (item != null)
            {
                if (PlayerTryUseItem(item))
                {
                    --item.Count;
                    if (item.Count <= 0)
                        m_Items.Remove(item);
                    return true;
                }
            }

            return false;
        }

        bool PlayerTryUseItem(Item item)
        {
            var t = item.Config.m_PropType;
            if (t == EPropType.BackToIdol)
            {
                return GameApp.Entry.Game.World.GoNearestIdol();
            }
            else if (t == EPropType.HealHp)
            {
                return GameApp.Entry.Game.Player.CAbility.Eat(() =>
                {
                    // 睡意治疗
                    GameApp.Entry.Game.Player.Heal(item.Config.m_Param1);
                });
            }
            else if (t == EPropType.HealHpContinuous)
            {
                return GameApp.Entry.Game.Player.CAbility.Eat(() =>
                {
                    // 持续治疗
                    GameApp.Entry.Game.Player.CBuff.AddBuff(EBuffType.HeadlHP, item.Config.m_Param1, item.Config.m_Param2);
                });
            }
            else
            {
                Debug.LogError($"Unknown type:{item.Config.m_PropType}");
                return false;
            }
        }

        public PlayerPropItemInfo[] ToItemsArray()
        {
            PlayerPropItemInfo[] array = new PlayerPropItemInfo[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                PlayerPropItemInfo item = new PlayerPropItemInfo();
                item.m_ID = Items[i].ID;
                item.m_Count = Items[i].Count;
                array[i] = item;
            }

            return array;
        }

        public PropItemInfo GetItemConfig(int id)
        {
            return GameApp.Entry.Config.PropInfo.m_Props.FirstOrDefault(a => a.m_ID == id);
        }
    }
}