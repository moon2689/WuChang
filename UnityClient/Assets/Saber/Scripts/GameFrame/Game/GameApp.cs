using System;
using UnityEngine;
using Saber.Director;

namespace Saber.Frame
{
    public class GameApp : MonoBehaviour
    {
        // 单例
        static GameApp s_instance;
        public static GameApp Instance => s_instance;

        static ScriptEntry s_entry;

        public static ScriptEntry Entry => s_entry ??= new ScriptEntry();
        public DirectorBase CurDir { get; private set; }


        public static void Create()
        {
            if (s_instance == null)
                new GameObject("GameApp", typeof(GameApp));
        }


        void Awake()
        {
            DontDestroyOnLoad(this);
            s_instance = this;
        }

        void Update()
        {
            if (CurDir == null)
            {
                CurDir = new DirectorInitGame();
                CurDir.Enter();
            }

            CurDir.Update();
            DirectorBase next = CurDir.GetNextDirector();
            TryEnterNextDir(next);
        }

        public void TryEnterNextDir(DirectorBase next)
        {
            if (next != null && next != CurDir)
            {
                CurDir.Exit();
                next.Enter();
                CurDir = next;
            }
        }

        void OnDestroy()
        {
            s_instance = null;
        }
    }
}