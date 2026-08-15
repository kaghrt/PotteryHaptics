using UnityEngine;
using Project.Core;

namespace Project.Visual
{
    /// <summary>
    /// 指位置をシェーダー(ClayDeformation等)に渡し、リアルタイムで凹み表現を更新する。
    /// MaterialPropertyBlockを使うので、同じマテリアルを共有していても
    /// オブジェクトごとに別々の見た目にできる。
    ///
    /// 【注意】FingerTracker.CurrentPositionは今のところ、デバイス原点を基準にした
    /// 独自座標(x/100, height/100, z/100)になっている。オブジェクトのワールド座標と
    /// ズレる場合は、offsetプロパティで調整するか、座標変換を別途挟む必要がある。
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class DeformationShaderDriver : MonoBehaviour
    {
        [SerializeField] private FingerTracker fingerTracker;
        [SerializeField] private VirtualSurfaceBase surface; // IsInContact参照用。未設定でも動く

        [Tooltip("FingerTrackerの座標系とこのオブジェクトのワールド座標系のズレを補正するオフセット")]
        [SerializeField] private Vector3 positionOffset = Vector3.zero;

        private static readonly int TouchPositionId = Shader.PropertyToID("_TouchPositionWS");
        private static readonly int TouchActiveId = Shader.PropertyToID("_TouchActive");

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (fingerTracker == null) return;

            bool isActive = surface == null || surface.IsInContact;
            Vector3 touchPositionWS = fingerTracker.CurrentPosition + positionOffset;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(TouchPositionId, touchPositionWS);
            propertyBlock.SetFloat(TouchActiveId, isActive ? 1f : 0f);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
