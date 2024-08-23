using ReadyPlayerMe.Core;
using UnityEngine;

public class MyAvatarLoader : MonoBehaviour
{
    [SerializeField, Tooltip("Set this to the URL or shortcode of the Ready Player Me Avatar you want to load.")]
    private string avatarUrl = "https://models.readyplayer.me/66ac2b5ec03e6ace580aa0ee.glb";

    private GameObject avatar;
    // Add 2 editable fields to store the new masculine and feminine animator controllers
    [SerializeField] private RuntimeAnimatorController masculineController;
    [SerializeField] private RuntimeAnimatorController feminineController;

    private void Start()
    {
        ApplicationData.Log();
        var avatarLoader = new AvatarObjectLoader();
        // use the OnCompleted event to set the avatar and setup animator
        avatarLoader.OnCompleted += (_, args) =>
        {
            avatar = args.Avatar;
            SetAnimatorController(args.Metadata.OutfitGender); //  <--------------- ADDED
        };
        avatarLoader.LoadAvatar(avatarUrl);
    }
		
    // This method is used to reassign the appropriate animator controller based on outfit gender
    private void SetAnimatorController(OutfitGender outfitGender)
    {
        var animator = avatar.GetComponent<Animator>();
        // set the correct animator based on outfit gender
        if (animator != null && outfitGender == OutfitGender.Masculine)
        {
            animator.runtimeAnimatorController = masculineController;
        }
        else
        {
            animator.runtimeAnimatorController = feminineController;
        }
    }

    private void OnDestroy()
    {
        if (avatar != null) Destroy(avatar);
    }
}