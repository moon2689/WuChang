using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UIAnimation.Actions
{
    public enum ActionEntryType
    {
        Executable,
        ParallelBlock
    };

    [Serializable]
    public class ActionEntry
    {
        public enum ActionEntryWrapMode
        {
            Clamp,
            Loop
        };

        [SerializeField]
        private ActionEntryWrapMode entryWrapMode = ActionEntryWrapMode.Clamp;
        public ActionEntryWrapMode EntryWrapMode
        {
            get
            {
                return entryWrapMode;
            }
            set
            {
                entryWrapMode = value;
            }
        }

        public ActionEntry()
        {
            nextId = kNotExistId;
            prevId = kNotExistId;
            parentId = kNotExistId;
        }

        [SerializeField]
        private const int kNotExistId = -1;


        [SerializeField]
        private int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }


        [SerializeField]
        private int parentId = kNotExistId;
        public int ParentId
        {
            get
            {
                return parentId;
            }
            set
            {
                parentId = value;
            }
        }

        public bool HasParent
        {
            get
            {
                return ParentId != kNotExistId;
            }
        }


        [SerializeField]
        private int nextId = kNotExistId;
        public int NextId
        {
            get
            {
                return nextId;
            }
            set
            {
                nextId = value;
            }
        }

        public bool HasNext
        {
            get
            {
                return NextId != kNotExistId;
            }
        }

        public void ResetNextId()
        {
            NextId = kNotExistId;
        }


        [SerializeField]
        private int prevId = kNotExistId;
        public int PrevId
        {
            get
            {
                return prevId;
            }
            set
            {
                prevId = value;
            }
        }

        public bool HasPrev
        {
            get
            {
                return PrevId != kNotExistId;
            }
        }

        public void ResetPrevId()
        {
            PrevId = kNotExistId;
        }


        [SerializeField]
        private List<int> childEntryIds = new List<int>();

        public void AddChildId(int id)
        {
            if (childEntryIds.Contains(id))
            {
                UnityEngine.Debug.LogError("Entry with id: " + id + " already contained");
            }
            childEntryIds.Add(id);
        }

        public List<ActionEntry> GetChildEntries(ActionEntryContainer container)
        {
            return childEntryIds.Select(id => container.GetEntry(id)).ToList();
        }

        public void RemoveChildId(int id)
        {
            childEntryIds.Remove(id);
        }

        public bool HasChild
        {
            get
            {
                return EntryType == ActionEntryType.ParallelBlock && childEntryIds.Count > 0;
            }
        }


        [SerializeField]
        private ActionEntryType entryType;
        public ActionEntryType EntryType
        {
            get
            {
                return entryType;
            }
            set
            {
                entryType = value;
            }
        }


        [SerializeField]
        private IAction primaryAction;
        public IAction PrimaryAction
        {
            get
            {
                return primaryAction;
            }
            set
            {
                if (EntryType != ActionEntryType.Executable)
                {
                    throw new System.Exception("Trying to set PrimaryAction to non-Executable entry");
                }
                primaryAction = value;
            }
        }


        public bool IsParallelBlockFinished(ActionEntryContainer container)
        {
            if (EntryType == ActionEntryType.ParallelBlock)
            {
                foreach (var childEntry in GetChildEntries(container))
                {
                    if (!childEntry.IsParallelBlockFinished(container))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                if (!HasNext)
                {
                    return (PrimaryAction == null || PrimaryAction.IsDone());
                }
                int last = NextId;
                int next = container.GetEntry(last).NextId;
                while (next != kNotExistId)
                {
                    last = next;
                    next = container.GetEntry(next).NextId;
                }
                return (PrimaryAction == null || PrimaryAction.IsDone()) && (container.GetEntry(last).PrimaryAction == null || container.GetEntry(last).PrimaryAction.IsDone());
            }
        }

        public void StepAction(float deltaTime, bool shouldPause)
        {
            PrimaryAction.OnStep(deltaTime, shouldPause);
        }

        public bool IsPlayable
        {
            get
            {
                return PrimaryAction != null && !PrimaryAction.IsDone();
            }
        }

        public void ResetAction()
        {
            if (PrimaryAction != null)
            {
                PrimaryAction.ResetStatus();
            }
        }


        public void PrepareAction()
        {
            if (PrimaryAction != null)
            {
                PrimaryAction.Prepare();
            }
        }

    }
}
