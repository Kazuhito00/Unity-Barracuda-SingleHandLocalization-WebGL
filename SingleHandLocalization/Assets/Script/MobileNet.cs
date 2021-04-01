using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Barracuda;

public class MobileNet
{   
    readonly IWorker worker;

    private int inputShapeX = 128;
    private int inputShapeY = 128;

    string[] className = {
        "無し",
        "パー",
        "グー"
    };

    public MobileNet(NNModel modelAsset)
    {
        var model = ModelLoader.Load(modelAsset);

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("Worker:CPU");
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model); // CPU
#else
        Debug.Log("Worker:GPU");
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model); // GPU
#endif
    }

    public float[] Inference(Texture2D texture, float score_th=0.5f)
    {
        // テクスチャコピー
        Texture2D inputTexture = new Texture2D(texture.width, texture.height);
        var tempColor32 = texture.GetPixels32();
        inputTexture.SetPixels32(tempColor32);
        inputTexture.Apply();
        Graphics.CopyTexture(texture, inputTexture);

        // テクスチャリサイズ、およびColor32データ取得
        TextureScale.Bilinear(inputTexture, inputShapeX, inputShapeY);
        var color32 = inputTexture.GetPixels32();
        MonoBehaviour.Destroy(inputTexture);
        
        int pixelCount = 0;
        float[] floatValues = new float[inputShapeX * inputShapeY * 3];
        for (int i = (color32.Length - 1); i >= 0 ; i--) {
            var color = color32[i];
            floatValues[pixelCount * 3 + 0] = (color.r - 0) / 255.0f;
            floatValues[pixelCount * 3 + 1] = (color.g - 0) / 255.0f;
            floatValues[pixelCount * 3 + 2] = (color.b - 0) / 255.0f;
            pixelCount += 1;
        }
        var inputTensor = new Tensor(1, inputShapeY, inputShapeX, 3, floatValues);

        // 推論実行
        worker.Execute(inputTensor);

        // 出力：クラスID
        var outputTensor01 = worker.PeekOutput("class_output");
        var outputArray01 = outputTensor01.ToReadOnlyArray();

        float maxScore = 0.0f;
        int classId = 0;
        for (int i = 0; i < outputArray01.Length; i++) {
            float score = outputArray01[i];
            if ((maxScore < score) && (score_th < score)) {
                maxScore = score;
                classId = i;
            }
        }
        
        // 出力：座標
        var outputTensor02 = worker.PeekOutput("localization_output");
        var outputArray02 = outputTensor02.ToReadOnlyArray();
        float pointX = (float)outputArray02[0];
        float pointY = (float)outputArray02[1];

        // 最終出力
        float[] results = {
            (float)classId, 
            Mathf.Clamp((float)maxScore, 0.0F, 1.0F), 
            Mathf.Clamp(pointX, 0.0F, 1.0F), 
            Mathf.Clamp(pointY, 0.0F, 1.0F)
        };
        
        // 解放
        inputTensor.Dispose();
        outputTensor01.Dispose();
        outputTensor02.Dispose();

        return results;
    }

    ~MobileNet()
    {
        worker?.Dispose();
    }

    public string getClassName(int classId)
    {
        if (classId < 0 || className.Length <= classId){
            return "";
        }
        return className[classId];
    }
}