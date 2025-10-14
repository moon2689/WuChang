using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SakashoUISystem;

namespace UIAnimation.Actions
{
    [Serializable]
    public class ActionEntryDictionary : SerializableDictionary<int, ActionEntry>
    {
    }

    [Serializable]
    public class ActionEntryContainer
    {
        public ActionEntryContainer()
        {
            entryDict = new ActionEntryDictionary();
        }

        [SerializeField] private int rootId = 0;

        [SerializeField] private int latestId = -1;

        public int LatestId
        {
            get
            {
                if (latestId < 0)
                {
                    latestId = rootId;
                }

                return latestId;
            }
            set { latestId = value; }
        }

        public ActionEntry RootEntry
        {
            get
            {
                if (EntryDict.Count == 0)
                {
                    ResetIds();
                    return null;
                }

                return EntryDict[rootId];
            }
        }

        private void ResetIds()
        {
            rootId = 0;
            latestId = -1;
        }

        [SerializeField] private ActionEntryDictionary entryDict;

        public ActionEntryDictionary EntryDict
        {
            get { return entryDict; }
        }

        public ActionEntry GetEntry(int id)
        {
            if (entryDict.ContainsKey(id) == false)
            {
                return null;
            }

            return EntryDict[id];
        }

        public void AddEntry(ActionEntry entry)
        {
            entryDict.Add(LatestId, entry);
            entry.Id = LatestId++;
        }

        public void InsertChildEntryTo(ActionEntry childEntry, ActionEntry parentEntry)
        {
            if (parentEntry.EntryType != ActionEntryType.ParallelBlock)
            {
                throw new System.Exception("Trying to insert child entry into non-ParallelBlock parent entry");
            }

            AddEntry(childEntry);
            childEntry.ParentId = parentEntry.Id;
            parentEntry.AddChildId(childEntry.Id);
        }

        public void AppendEntryTo(ActionEntry nextEntry, ActionEntry thisEntry)
        {
            AddEntry(nextEntry);
            thisEntry.NextId = nextEntry.Id;
            nextEntry.PrevId = thisEntry.Id;
        }

        public void RemoveEntry(ActionEntry entry)
        {
            foreach (var child in entry.GetChildEntries(this))
            {
                var nextId = child.NextId;
                while (GetEntry(nextId) != null)
                {
                    var nextEntry = GetEntry(nextId);
                    nextId = nextEntry.NextId;
                    RemoveEntry(nextEntry);
                }

                RemoveEntry(child);
            }

            if (entry.Id == rootId)
            {
                if (entry.HasNext)
                {
                    rootId = entry.NextId;
                    GetEntry(entry.NextId).ResetPrevId();
                }
                else
                {
                    latestId = rootId;
                }
            }

            if (entry.HasPrev)
            {
                if (entry.HasNext)
                {
                    GetEntry(entry.PrevId).NextId = entry.NextId;
                    GetEntry(entry.NextId).PrevId = entry.PrevId;
                }
                else
                {
                    GetEntry(entry.PrevId).ResetNextId();
                }
            }

            if (entry.HasParent)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("removing: " + entry.Id + " from parent: " + GetEntry(entry.ParentId).Id);
#endif
                GetEntry(entry.ParentId).RemoveChildId(entry.Id);
                if (entry.HasNext)
                {
                    GetEntry(entry.ParentId).AddChildId(entry.NextId);
                    GetEntry(entry.NextId).ResetPrevId();
                    GetEntry(entry.NextId).ParentId = entry.ParentId;
                }
            }

            entryDict.Remove(entry.Id);
        }

        public void RemoveAllEntries()
        {
            entryDict.Clear();
            ResetIds();
        }
    }
}