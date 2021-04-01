# Unity-Barracuda-SingleHandLocalization-WebGL
Unity Barracudaで[Single-Hand-Localization](https://github.com/Kazuhito00/Single-Hand-Localization)を動作させるサンプルです。<br>
単一の手の位置推定を行います。<br>
また、現時点(2021/04/02)でUnityのWebGLはCPU推論のみのサポートです。<br><br>
<img src="https://user-images.githubusercontent.com/37477845/113316638-8ba9f880-9349-11eb-8cfc-f65ec82bd300.gif" width="60%"><br>
※上記イメージはブラウザ上でのWebGL実行(CPU推論)

# Demo
動作確認用ページは以下。<br>
[https://kazuhito00.github.io/Unity-Barracuda-SingleHandLocalization-WebGL/WebGL-Build](https://kazuhito00.github.io/Unity-Barracuda-SingleHandLocalization-WebGL/WebGL-Build/)

# Requirement (Unity)
* Unity 2021.1.0b6 or later
* Barracuda 1.3.0 or later

# FPS(参考値)
WebCamController.cs の Update()の呼び出し周期を計測したものです。<br>
以下のように動作は基本的に非同期処理のため、FPSは見かけ上のFPSであり、推論自体のFPSではありません。<br>
|  | SingleHandLocalization |
| - | :- |
| WebGL<br>CPU：Core i7-8750H CPU @2.20GHz | 約6.7FPS<br>CSharpBurst |
| WebGL<br>CPU：CPU：Core i5-5200U CPU @2.20GHz | 約3.0FPS<br>CSharpBurst |
| Unity Editor<br>GPU：GTX 1050 Ti Max-Q(4GB) | 約45FPS<br>ComputePrecompiled |

<img src="https://user-images.githubusercontent.com/37477845/113316578-7634ce80-9349-11eb-87c0-c4102c595e46.gif" width="60%">
※上記イメージはUnity Editorでの実行(GPU推論)

# Reference
* [Barracuda 1.3.0 preview](https://docs.unity3d.com/Packages/com.unity.barracuda@1.3/manual/index.html)
* [Kazuhito00/Single-Hand-Localization](https://github.com/Kazuhito00/Single-Hand-Localization)

# Author
高橋かずひと(https://twitter.com/KzhtTkhs)

# License 
Unity-Barracuda-SingleHandLocalization-WebGL is under [Apache v2 License](LICENSE).
