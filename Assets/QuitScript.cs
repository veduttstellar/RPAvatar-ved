using UnityEngine;

public class QuitScript : MonoBehaviour
{
    public void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}