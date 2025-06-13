using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public GameObject profilePanel;

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
    }

    public void CloseProfile()
    {
        profilePanel.SetActive(false);
    }
}
