using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ModelDownloader : MonoBehaviour
{
    private string baseUrl = "https://tbzgewjnfcjviexmgnsq.supabase.co/storage/v1/object/public/test/";
    private string downloadFolder = "Assets/Resources/Postcards/";

    // URL para obtener los nombres de los modelos desde la base de datos
    private string modelsApiUrl = "https://tbzgewjnfcjviexmgnsq.supabase.co/rest/v1/Postcard?select=model";

    void Start()
    {
        // Crea la carpeta si no existe
        if (!Directory.Exists(downloadFolder))
        {
            Directory.CreateDirectory(downloadFolder);
        }

        // Obtener los nombres de los modelos desde Supabase
        StartCoroutine(GetModelNamesFromSupabase());
    }

    IEnumerator GetModelNamesFromSupabase()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(modelsApiUrl))
        {
            // Enviar la solicitud
            webRequest.SetRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRiemdld2puZmNqdmlleG1nbnNxIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc0MTcxOTIyMCwiZXhwIjoyMDU3Mjk1MjIwfQ.wARV8xWmmPgw2q_eDzhpepJmXwJqWWbfZNaoabvQ3so"); // Reemplaza con tu JWT
            webRequest.SetRequestHeader("apikey", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRiemdld2puZmNqdmlleG1nbnNxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDE3MTkyMjAsImV4cCI6MjA1NzI5NTIyMH0.aGbANeeHSe-MskK3U6z-DJI-x6TSAryHoS6mKonCF6w"); // Reemplaza con tu API Key

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Parsear la respuesta JSON y extraer los nombres de los modelos
                List<ModelResponse> models = JsonConvert.DeserializeObject<List<ModelResponse>>(webRequest.downloadHandler.text);

                // Descargar cada modelo
                foreach (var model in models)
                {
                    StartCoroutine(DownloadModel(model.model));
                }
            }
            else
            {
                Debug.LogError($"Error obteniendo modelos: {webRequest.error}");
            }
        }
    }

    IEnumerator DownloadModel(string modelName)
    {
        string url = baseUrl + modelName;
        string filePath = Path.Combine(downloadFolder, modelName);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Enviar la solicitud
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Guardar el archivo
                File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                Debug.Log($"Modelo descargado: {modelName}");
            }
            else
            {
                Debug.LogError($"Error descargando {modelName}: {webRequest.error}");
            }
        }
    }

    // Clase para deserializar la respuesta de Supabase
    [System.Serializable]
    public class ModelResponse
    {
        public string model;
    }
}
