using UnityEngine;

namespace GGemCo.Scripts.Scenes
{
    public class SceneIntro : MonoBehaviour
    {
        public void OnClickGameStart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
        }
    }
}
