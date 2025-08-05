using System.Linq;
using Saber.Director;
using Saber.Frame;

using Saber.World;
using UnityEngine;
using UnityEngine.UI;


namespace Saber.UI
{
    public class Wnd_SelectWeapon : WndBase
    {
        public interface IHandler : IWndHandler
        {
        }

        [SerializeField] private Button m_btnClose;
        [SerializeField] private GameObject m_BtnTemp;
        [SerializeField] private GameObject m_Grid;

        private IHandler m_Handler;


        protected override bool PauseGame => true;

        protected override void OnAwake()
        {
            base.OnAwake();
            m_Handler = base.m_WndHandler as IHandler;
            m_btnClose.onClick.AddListener(OnClickClose);

            /*
            BigWorld bigWorld = GameApp.Entry.Game.World;
            if (bigWorld != null && bigWorld.Player is SCharacter cha)
            {
                foreach (var weaponItem in cha.WeaponStyleInfo.m_Weapons)
                {
                    Button button = GameObject.Instantiate(m_BtnTemp).GetComponent<Button>();
                    button.transform.SetParent(m_Grid.transform);
                    button.transform.localScale = Vector3.one;
                    var skillConfig = GameApp.Entry.Config.SkillInfo.m_Skills.FirstOrDefault(a => a.m_ID == weaponItem.m_SkillID);
                    button.transform.GetComponentInChildren<Text>().text = skillConfig.m_Name;
                    button.onClick.AddListener(() =>
                    {
                        m_Handler?.OnClickChangeWeapon(weaponItem);
                        GameApp.Entry.Game.Audio.PlayCommonClick();
                        Destroy();
                    });
                    button.gameObject.SetActive(true);
                }
            }
            */
        }

        void OnClickClose()
        {
            Destroy();
        }

        protected override void OnDestroy()
        {
            GameApp.Entry.Game.Audio.Play2DSound("Sound/UI/ActorInfoWndClose");
            base.OnDestroy();
        }
    }
}