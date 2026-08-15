using UnityEngine;
using Project.Core;

namespace Project.Visual
{
    /// <summary>
    /// 粘性テスト用。指の水平移動速度に応じて、泥のメッシュを移動方向に伸ばす
    /// (MudSurfaceシェーダーの_StretchAmount/_StretchDirectionWSを駆動する)。
    /// 静止するとSmoothDampでなめらかに元の形に戻る(粘性らしい「ゆっくり戻る」動き)。
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MudStretchDriver : MonoBehaviour
    {
        [SerializeField] private FingerTracker fingerTracker;

        [Tooltip("速度をどれくらい伸びに変換するか")]
        [SerializeField] private float stretchPerSpeed = 0.5f;

        [Tooltip("最大の伸び量[m]")]
        [SerializeField] private float maxStretch = 0.08f;

        [Tooltip("値の変化のなめらかさ(大きいほどゆっくり追従・ゆっくり戻る)")]
        [SerializeField] private float smoothTime = 0.15f;

        private static readonly int StretchAmountId = Shader.PropertyToID("_StretchAmount");
        private static readonly int StretchDirectionId = Shader.PropertyToID("_StretchDirectionWS");

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;

        private float currentStretch;
        private float stretchVelocity; // SmoothDamp用の内部速度
        private Vector3 currentDirection = Vector3.forward;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (fingerTracker == null) return;

            Vector3 velocity = fingerTracker.CurrentVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float speed = horizontalVelocity.magnitude;

            float targetStretch = Mathf.Clamp(speed * stretchPerSpeed, 0f, maxStretch);
            currentStretch = Mathf.SmoothDamp(currentStretch, targetStretch, ref stretchVelocity, smoothTime);

            if (speed > 0.001f)
            {
                currentDirection = Vector3.Lerp(currentDirection, horizontalVelocity.normalized, Time.deltaTime * 10f);
            }

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(StretchAmountId, currentStretch);
            propertyBlock.SetVector(StretchDirectionId, currentDirection);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
