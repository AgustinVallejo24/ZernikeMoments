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
        else if (Input.GetKeyDown(KeyCode.F3))
        {

        }
    }
    
}
