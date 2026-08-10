# ThirdParty/

このフォルダは教授支給プロジェクトから移植する以下のアセットを配置する場所です。
容量が大きい(過去に100MB超でgit pushがrejectされた実績あり)ため、
**このフォルダ自体はコミット対象外**とし、移植元と手順だけをここに記録します。

## 移植するもの

- `UltrahapticsCoreAsset(beta9)`
  - 移植元: 教授支給プロジェクト(2019.4.14f1の動作確認済みバージョン)
  - Leap Motion Core Assets 4.4.0 がHandModel/FingerModelのreflectionに必要
- `Assets/StreamingAssets/Python`
  - 移植元: 同上

## 移植時の注意点(過去のハマりどころ)

- `LeapHandController` が missing script になっていたら、孤立コンポーネントを削除し、
  Hand Model Manager を Leap Service Provider に配線し直す
- Main Camera の Transform Scale が 1.0 になっているか必ず確認
  (過去にコピー操作でScaleが0.168に壊れるバグが2プロジェクトで発生済み)
- Active Input Handling は Both に設定
