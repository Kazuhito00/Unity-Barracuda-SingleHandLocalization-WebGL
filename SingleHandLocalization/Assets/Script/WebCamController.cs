using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Barracuda;

public class WebCamController : MonoBehaviour
{
    int width = 640;
    int height = 480;
    int fps = 30;
    WebCamTexture webcamTexture;

    // MobileNetモデル関連
    public NNModel modelAsset;    
    private MobileNet mobileNet;
    
    Texture2D texture;
    Color32[] cameraBuffer = null;

    // 推論結果描画用テキスト
    public Text text;
    private readonly FPSCounter fpsCounter = new FPSCounter();

    void Start() 
    {
        // Webカメラ準備
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[0].name, this.width, this.height, this.fps);
        webcamTexture.Play();
        
        // MobileNetV2推論用クラス
        mobileNet = new MobileNet(modelAsset);
        StartCoroutine(WebCamTextureInitialize());
    }

    IEnumerator WebCamTextureInitialize()
    {
        while (true) {
            if (webcamTexture.width > 16 && webcamTexture.height > 16) {
                GetComponent<Renderer>().material.mainTexture = webcamTexture;
                cameraBuffer = new Color32[webcamTexture.width * webcamTexture.height];
                texture = new Texture2D(webcamTexture.width, webcamTexture.height);
                break;
            }
            yield return null;
        }
    }
    
    void Update()
    {
        fpsCounter.Update();

        // 入力用テクスチャ準備
        webcamTexture.GetPixels32(cameraBuffer);
        texture.SetPixels32(cameraBuffer);
        texture.Apply();
        
        // 推論
        var results = mobileNet.Inference(texture, 0.7f);
        int classId = (int)results[0];
        float maxScore = results[1];

        // 描画
        if ((classId == 0) || (cameraBuffer == null)) {
            // pass
        } else {
            int pointX = (int)(results[2] * webcamTexture.width);
            int pointY = (int)(results[3] * webcamTexture.height);
            pointX = (int)(Mathf.Clamp(pointX, 0, webcamTexture.width));
            pointY = (int)(Mathf.Clamp(pointY, 0, webcamTexture.height));
            var drawPoint = new Vector2(pointX, pointY);
            if (classId == 1) {
                Color32 color = new Color32(0, 0, 255, 0);
                DrawPoint(drawPoint, color, 10.0f);    
            } else if (classId == 2){
                Color32 color = new Color32(0, 255, 0, 0);
                DrawPoint(drawPoint, color, 10.0f);    
            }
            texture.SetPixels32(cameraBuffer);
            texture.Apply();
            GetComponent<Renderer>().material.mainTexture = texture;
        }
    
        // 描画用テキスト構築
        string resultText = "";
        resultText = "FPS:" + fpsCounter.FPS.ToString("F2") + "\n" + "\n";        
        resultText = resultText + "Class ID:" + classId.ToString() + "\n";
        resultText = resultText + "Score:" + maxScore.ToString("F3") + "\n";
        if (classId >= 0) {
            resultText = resultText + "Name:" + mobileNet.getClassName(classId) + "\n";
        } else {
            resultText = resultText + "Name:????\n";
        }
#if UNITY_IOS || UNITY_ANDROID
        resultText = resultText + SystemInfo.graphicsDeviceType;
#endif

        // テキスト画面反映
        text.text = resultText;
    }

    private void DrawPoint(Vector2 point, Color32 color, double brushSize = 1.5f)
    {
        // 点描画
        point.x = (int)point.x;
        point.y = (int)point.y;

        int start_x = Mathf.Max(0, (int)(point.x - (brushSize - 1)));
        int end_x = Mathf.Min(webcamTexture.width, (int)(point.x + (brushSize + 1)));
        int start_y =  Mathf.Max(0, (int)(point.y - (brushSize - 1)));
        int end_y = Mathf.Min(webcamTexture.height, (int)(point.y + (brushSize + 1)));

        for (int x = start_x; x < end_x; x++) {
            for (int y = start_y; y < end_y; y++) {
                double length = Mathf.Sqrt(Mathf.Pow(point.x - x, 2) + Mathf.Pow(point.y - y, 2));
                if (length < brushSize) {
                    cameraBuffer.SetValue(color, (webcamTexture.width - x) + (webcamTexture.width * (webcamTexture.height - y)));
                }
            }
        }
    }
}