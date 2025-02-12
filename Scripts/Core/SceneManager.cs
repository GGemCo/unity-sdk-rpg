namespace GGemCo.Scripts.Core
{
    public abstract class SceneManager
    {
        public static void ChangeScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}