using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    //private Animator animator;
    public Button avatarButton;
    public Button contButton;

    // Start is called before the first frame update
    void Start()
    {


        //SceneManager.LoadScene("entry");


        // Find the Animator component on the Avatar


        //animator = GetComponent<Animator>();

        contButton.onClick.AddListener(OnClientButtonClick);
        avatarButton.onClick.AddListener(OnServerButtonClick);
    }

    // Update is called once per frame
    //public void PlayAnimation(string animationName)
    //{
    //    // Play the specified animation


    //    animator.Play(animationName);
    //}



    public void OnClientButtonClick()
    {
      
        //NetworkManager.Singleton.StartClient();
        PlayerPrefs.SetString("Role", "Controller");
        SceneManager.LoadScene("ControllerScene");
    }


    public void OnServerButtonClick()
    {
     
        //NetworkManager.Singleton.StartServer();
        PlayerPrefs.SetString("Role", "Avatar");
        SceneManager.LoadScene("UserScene");
    }


}
