using System.IO;
using UnityEngine;

public class ExternalFileManager : MonoBehaviour
{

    // Set this path to your target folder
    private string externalFilePath = @"C:\Users\mrkra\OneDrive\Documents\GitHub\Casino-Game\Assets\Scripts\Currency.txt";
    
    // Or use a relative path from a known location
    // private string externalFilePath = Path.Combine(
    //     System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
    //     "MyGame", "currency.txt"
    // );
    
    public void WriteToExternalFile(int value)
    {
        try
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(externalFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write the value
            File.WriteAllText(externalFilePath, value.ToString());
            Debug.Log($"Successfully wrote {value} to {externalFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
        }
    }
    
    public int ReadFromExternalFile()
    {
        try
        {
            if (File.Exists(externalFilePath))
            {
                string content = File.ReadAllText(externalFilePath);
                if (int.TryParse(content, out int value))
                {
                    Debug.Log($"Successfully read {value} from {externalFilePath}");
                    return value;
                }
            }
            else
            {
                Debug.LogWarning($"File not found: {externalFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read from file: {e.Message}");
        }
        
        return 0; // Default value
    }

}