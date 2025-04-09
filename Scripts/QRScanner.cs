using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRScanner : MonoBehaviour
{
    public string modelsPath = "Postcards";
    public Transform modelParent;
    private WebCamTexture webcamTexture;
    private string lastQrCode = string.Empty;
    private bool qrDetected = false;
    private GameObject currentModel;
    public float baseScale = 1f;

    void Start()
    {
        RawImage rawImage = GetComponent<RawImage>();
        rawImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        StartCoroutine(InitCamera());
    }

    IEnumerator InitCamera()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No se encontraron cámaras.");
            yield break;
        }

        int requestedWidth = 1920;
        int requestedHeight = 1080;
        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, requestedWidth, requestedHeight);
        GetComponent<RawImage>().texture = webcamTexture;
        webcamTexture.Play();
        StartCoroutine(ScanQRCode());
        yield break;
    }

    IEnumerator ScanQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();
        var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);

        while (true)
        {
            try
            {
                snap.SetPixels32(webcamTexture.GetPixels32());
                var result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width, webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);

                if (result != null)
                {
                    string url = result.Text;
                    if (url.Contains("/test/"))
                    {
                        string modelName = url.Substring(url.LastIndexOf("/test/") + 6);
                        modelName = System.IO.Path.GetFileNameWithoutExtension(modelName);

                        if (lastQrCode != modelName)
                        {
                            lastQrCode = modelName;
                            qrDetected = true;
                            Debug.Log("QR Detectado: " + modelName);
                            LoadModel(modelName, result.ResultPoints);
                        }
                    }
                }
                else if (qrDetected)
                {
                    qrDetected = false;
                    lastQrCode = string.Empty;
                    Debug.Log("QR Perdido");
                    HideModel();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }

            yield return null;
        }
    }

    void LoadModel(string modelName, ResultPoint[] qrPoints)
    {
        HideModel();
        GameObject loadedModel = Resources.Load<GameObject>($"{modelsPath}/{modelName}");

        if (loadedModel != null)
        {
            currentModel = Instantiate(loadedModel, modelParent);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            AdjustModelScale(qrPoints);
            Debug.Log($"Modelo {modelName} cargado en la escena.");
        }
        else
        {
            Debug.LogWarning($"No se encontró el modelo: {modelName}");
        }
    }

    void AdjustModelScale(ResultPoint[] qrPoints)
    {
        if (qrPoints != null && qrPoints.Length >= 2)
        {
            float qrSize = Vector2.Distance(new Vector2(qrPoints[0].X, qrPoints[0].Y), new Vector2(qrPoints[1].X, qrPoints[1].Y));
            float scaleFactor = Mathf.Clamp(qrSize / 500f, 0.5f, 3f);
            currentModel.transform.localScale = Vector3.one * baseScale * scaleFactor;
        }
    }

    void HideModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
            Debug.Log("Modelo ocultado.");
        }
    }

    void Update()
    {
        if (qrDetected && currentModel != null && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                // Mover el modelo horizontalmente con el dedo
                float moveSpeed = 0.005f;
                currentModel.transform.localPosition += new Vector3(delta.x * moveSpeed, delta.y * moveSpeed, 0f);
            }
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            webcamTexture.Stop();
        }
        else
        {
            if (webcamTexture != null && !webcamTexture.isPlaying)
            {
                webcamTexture.Play();
            }
        }
    }
}
