using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    #region UI Connection
    [SerializeField] TMP_Text textGUI = null;
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Screen.SetResolution(640, 480, false, 60);
    }

    void Start()
    {
        this.frameCount = 0;
        this.lastTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        this.frameCount++;
        // 前回の記録時刻からの時間が一定間隔を超えたらフレーム数を時間で割ってFPSを更新
        float deltaTime = Time.realtimeSinceStartup - this.lastTime;
        if (deltaTime >= 0.5f)
        {
            var fps = this.frameCount / deltaTime;
            this.textGUI.text = $"FPS: {fps.ToString("00.00")}";

            this.frameCount = 0;
            this.lastTime = Time.realtimeSinceStartup;
        }
    }

    int frameCount = 0;
    float lastTime = 0;
}