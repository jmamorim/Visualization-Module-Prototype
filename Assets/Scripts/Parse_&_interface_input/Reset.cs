using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    public void ResetVis()
    {
        SceneManager.LoadScene(0);
    }
}
