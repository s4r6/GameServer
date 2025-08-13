# GameServer

[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![WebSocket](https://img.shields.io/badge/Protocol-WebSocket-brightgreen)](https://developer.mozilla.org/ja/docs/Web/API/WebSockets_API)

## プロジェクト概要
本リポジトリは、大学院の研究で開発している3D空間上でリスクアセスメントを体験・学習できるゲームのサーバー側実装です。  
ASP.NET Core を用いて **WebSocket通信**を行い、複数クライアント（Unity WebGL）間のリアルタイム同期を実現しています。  

クライアント側リポジトリはこちら: [MyResearch](https://github.com/s4r6/MyResearch)

---

## 使用している主な技術
- **フレームワーク**: ASP.NET Core 8.0
- **通信**: WebSocket (リアルタイム双方向通信)
- **アーキテクチャ**: Clean Architecture を参考
  - 主に以下の4層構造  
    - View & Infrastracture
    - Interface Adapter  
    - Application  
    - Domain  
- **開発言語**: C#

---

## ディレクトリ構成
```
GameServer/
├── Application/ # ユースケース層（アプリケーションサービス）
├── Domain/ # ドメイン層（エンティティ,ドメインサービス）
├── Infrastructure/ # インフラ層（WebSocket実装、永続化、外部I/O）
├── InterfaceAdapter/ # インターフェース層（Presenter、Controller、Gateway実装）
├── Utility/ # 共通ユーティリティ（拡張メソッド、ヘルパー等）
├── Logs/ # ログ出力（ランタイム生成想定・運用用）
└── Master/ # マスターデータ等（必要に応じて）
```
