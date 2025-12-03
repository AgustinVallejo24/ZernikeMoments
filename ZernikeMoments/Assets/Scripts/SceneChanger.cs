using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("UploadDrawing");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene("SymbolConfiguration");
        }        
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void GoToUploadDrawing()
    {
        SceneManager.LoadScene("UploadDrawing");
    }

    public void GoToUploadImage()
    {
        SceneManager.LoadScene("UploadImage");
    }

    public void GoToConfigSymbols()
    {
        SceneManager.LoadScene("SymbolConfiguration");
    }

    public void GoToRecognizer()
    {
        SceneManager.LoadScene("ZernikeMoments");
    }
    
    public void GoToTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
