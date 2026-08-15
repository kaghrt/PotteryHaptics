using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 指位置(ワールド座標、単位はメートル)を提供する抽象。
    /// 実機ではLeap Motion由来のProviderを、大学に行くまではダミー入力を実装する。
    /// </summary>
    public interface IFingerPositionProvider
    {
        Vector3 GetFingerPosition();
        bool IsTracking { get; }
    }
}
