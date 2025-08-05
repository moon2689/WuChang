using UnityEngine;

namespace Saber.Frame
{
    // 游戏启动器
    public class GameLauncher : MonoBehaviour
    {
        void Awake()
        {
            //Debug.Log("Game Launch...");
            GameApp.Create();
            Destroy(gameObject);
        }
    }
}
