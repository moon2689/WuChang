using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace UIAnimation.Actions
{
    [AddComponentMenu("UIAnimation/Actions/Action Runner")]
    public class ActionRunner : MonoBehaviour
    {
        public enum RunnerStatus
        {
            Stop,
            Play,
            Pause
        };

        [SerializeField]
        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        [SerializeField]
        private ActionEntryContainer entryContainer = new ActionEntryContainer();
        public ActionEntryContainer EntryContainer
        {
            get
            {
                return entryContainer;
            }
            set
            {
                entryContainer = value;
            }
        }

        public event Action OnFinishedAllActionsEvent;

        private int numOfExecutableActionEntry;
        private int numOfFinishedActionEntry;

        public RunnerStatus Status { get; private set; }

        private bool isInitialized = false;
        void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            PrepareRunner();
            numOfExecutableActionEntry = 0;

            if (numOfFinishedActionEntry != 0 || numOfExecutableActionEntry != 0)
            {
                throw new System.Exception("Both numOfFinishedActionEntry and numOfExecutableActionEntry should be 0 before Reinialization");
            }

            foreach (ActionEntry entry in EntryContainer.EntryDict.Values)
            {
                if (entry.EntryType == ActionEntryType.Executable && entry.PrimaryAction)
                {
                    numOfExecutableActionEntry++;
                    entry.PrimaryAction.OnActionDone += OnExecutableActionEntryIsDone;
                }
            }
        }

        public void ReInitialize()
        {
            isInitialized = false;
            Initialize();
        }

        public bool Run()
        {
            if (this.gameObject == null || !this.gameObject.activeInHierarchy)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(string.Format("Coroutine couldn't be started because game object '{0}:{1}' is inactive!", gameObject.name, gameObject.GetInstanceID()));
#endif
                return false;
            }

            if (EntryContainer.RootEntry == null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("<color=green>No Root Entry found, 'Run' action cancelled.</color>");
#endif
                return false;
            }

            if (Status == RunnerStatus.Pause)
            {
                Status = RunnerStatus.Play;
                return true;
            }

            if (Status == RunnerStatus.Stop)
            {
                // コメントアウト理由：UIScalableButtonなど当初想定しなかった使い方に合わせるのために。UISystemではコメントアウトしない予定 by Lx
                //  ResetRunner();
                numOfFinishedActionEntry = 0;
                Status = RunnerStatus.Play;
                PrepareEntry(EntryContainer.RootEntry);
                StartCoroutine(Step(EntryContainer.RootEntry));
            }

            return true;
        }

        public void FastForward()
        {
            if (Status != RunnerStatus.Stop)
            {
                Stop();
            }
            else
            {
                ResetRunner();
            }
            FinalizeEntryFrom(EntryContainer.RootEntry);
        }

        public void Pause()
        {
            if (Status == RunnerStatus.Play)
            {
                Status = RunnerStatus.Pause;
            }
        }

        public void Stop()
        {
            StopAllCoroutines();
            ResetRunner();
        }

        public void Abort()
        {
            if (Status == RunnerStatus.Stop)
            {
                return;
            }

            StopAllCoroutines();
            if (OnFinishedAllActionsEvent != null)
            {
                OnFinishedAllActionsEvent();
            }
            PrepareRunner();
        }

        private void ResetRunner()
        {
            PrepareRunner();
            foreach (var entry in entryContainer.EntryDict.Values)
            {
                entry.ResetAction();
            }
        }

        private void PrepareRunner()
        {
            numOfFinishedActionEntry = 0;
            Status = RunnerStatus.Stop;
        }

        private IEnumerator Step(ActionEntry entry)
        {
            foreach (var childEntry in entry.GetChildEntries(EntryContainer))
            {
                StartCoroutine(Step(childEntry));
            }

            while (entry.IsPlayable)
            {
                entry.StepAction(Time.deltaTime, Status == RunnerStatus.Pause);
                yield return null;
            }

            if (entry.HasNext)
            {
                while (entry.EntryType == ActionEntryType.ParallelBlock && !entry.IsParallelBlockFinished(EntryContainer))
                {
                    yield return null;
                }
                var nextEntry = EntryContainer.GetEntry(entry.NextId);
                PrepareEntry(nextEntry);
                StartCoroutine(Step(nextEntry));
            }

            if (entry.EntryType == ActionEntryType.ParallelBlock && entry.EntryWrapMode == ActionEntry.ActionEntryWrapMode.Loop)
            {
                while (!entry.IsParallelBlockFinished(EntryContainer))
                {
                    yield return null;
                }
                yield return null; // restart on the next frame of last tweener's IsDone

                ResetEntryFrom(entry);
                PrepareEntry(entry);
                StartCoroutine(Step(entry));
            }
        }

        private void OnExecutableActionEntryIsDone()
        {
            numOfFinishedActionEntry++;
            if (numOfFinishedActionEntry == numOfExecutableActionEntry)
            {
                PrepareRunner();
                if (OnFinishedAllActionsEvent != null)
                {
                    OnFinishedAllActionsEvent();
                }
            }
        }

        private void ResetEntryFrom(ActionEntry entry)
        {
            TraverseFromEntry(entry, (ActionEntry e) => e.ResetAction());
        }

        private void FinalizeEntryFrom(ActionEntry entry)
        {
            TraverseFromEntry(entry, (ActionEntry e) =>
            {
                if (e.EntryType == ActionEntryType.Executable && e.PrimaryAction != null)
                {
                    //                    e.PrimaryAction.Prepare(); // TweenLegacyAnimation needs to be Prepared before NormailzedTime could be set
                    e.PrimaryAction.FinalizeAction(true);
                }
            });
        }

        private void TraverseFromEntry(ActionEntry entry, Action<ActionEntry> procedure)
        {
            procedure(entry);

            foreach (var child in entry.GetChildEntries(EntryContainer))
            {
                var nextId = child.NextId;
                while (EntryContainer.GetEntry(nextId) != null)
                {
                    var nextEntry = EntryContainer.GetEntry(nextId);
                    nextId = nextEntry.NextId;
                    TraverseFromEntry(nextEntry, procedure);
                }
                TraverseFromEntry(child, procedure);
            }

            if (entry.HasNext)
            {
                TraverseFromEntry(EntryContainer.GetEntry(entry.NextId), procedure);
            }
        }


        private void PrepareEntry(ActionEntry entry)
        {
            foreach (var child in entry.GetChildEntries(EntryContainer))
            {
                PrepareEntry(child);
            }

            entry.PrepareAction();
        }

        [SerializeField]
        private bool fastforwardOnDisable = false;
        public bool FastforwardOnDisable
        {
            get
            {
                return fastforwardOnDisable;
            }
            set
            {
                fastforwardOnDisable = value;
            }
        }

        // OnDisable is called before SetActive(false)
        void OnDisable()
        {
            if (fastforwardOnDisable && Status == RunnerStatus.Play)
            {
                FastForward();
            }
            else
            {
                StopAllCoroutines();
            }
        }
    }
}
