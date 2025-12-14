# TotoBook - GitHub Copilot Instructions

## プロジェクト概要

TotoBook は、画像ビューアおよびアーカイブビューアの WPF アプリケーションです。
画像ファイルやアーカイブファイル（ZIP、RAR、CBZ など）を閲覧するための機能を提供します。

## 技術スタック

- **.NET**: .NET 9.0 (Windows 10.0.17763.0 以降をターゲット)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **アーキテクチャパターン**: MVVM (Model-View-ViewModel)
- **言語**: C#

### 主要な NuGet パッケージ

- `CommunityToolkit.Mvvm` (8.4.0) - MVVM パターンの実装支援
- `sharpcompress` (0.39.0) - アーカイブファイルの処理
- `YamlDotNet` (16.3.0) - YAML 形式の設定ファイル管理
- `Microsoft-WindowsAPICodePack-Shell` (1.1.5) - Windows Shell API
- `System.Drawing.Common` (9.0.1) - 画像処理
- `System.Interactive` (6.0.1) - LINQ 拡張

## プロジェクト構造

```
TotoBook/
├── View/               # XAMLビュー
├── ViewModel/          # ビューモデル（MVVM）
├── Spi/               # Susieプラグイン関連
├── MainWindow.xaml    # メインウィンドウ
├── App.xaml           # アプリケーションエントリポイント
└── *.cs               # モデル・ユーティリティクラス

TotoBook_Test/         # 単体テストプロジェクト
```

## コーディング規約

### 一般的なルール

1. **日本語コメント**: XML ドキュメントコメントおよびインラインコメントは日本語で記述
2. **命名規則**:
   - クラス名・メソッド名: PascalCase
   - プライベートフィールド: camelCase
   - プロパティ: PascalCase
3. **文字コード**: アーカイブファイルの処理では Shift-JIS (CP932) を使用

### MVVM パターン

- **ViewModel**: `CommunityToolkit.Mvvm`の`ObservableRecipient`を継承
- **データバインディング**: XAML と ViewModel を明確に分離
- **イベント**: ViewModel から View への通知には`EventHandler`を使用
- **コマンド**: ボタンやメニューのアクションは`ICommand`として実装

### ファイル命名

- **ViewModel**: `*ViewModel.cs` (例: `MainWindowViewModel.cs`, `PreferenceDialogViewModel.cs`)
- **ViewModel インターフェース**: `I*ViewModel.cs` (例: `IFileListItemViewModel.cs`, `IFileTreeItemViewModel.cs`)
- **View (XAML)**: `*.xaml` および `*.xaml.cs` (例: `MainWindow.xaml`, `PreferenceDialog.xaml`)
- **テストクラス**: `*_Test.cs` (例: `Archive_Test.cs`, `MainWindowViewModel_Test.cs`)
- **モデル・ユーティリティクラス**: PascalCase で記述 (例: `Archive.cs`, `ApplicationSettings.cs`, `BrowseHistory.cs`)

## 主要コンポーネント

### MainWindowViewModel

- メインウィンドウのビジネスロジックを管理
- ファイルリスト、画像表示、ナビゲーション、履歴管理を担当
- 自動ページ送り機能を持つ`AutoPagerTimer`を使用

### Archive

- SharpCompress ライブラリを使用したアーカイブファイル処理
- Shift-JIS (CP932) エンコーディングでアーカイブ内のファイル名を読み込み
- ZIP、RAR、CBZ などのフォーマットに対応

### SpiManager

- Susie プラグイン（.spi ファイル）の読み込みと管理
- 画像読み込みプラグイン（Import）とアーカイブプラグイン（Archive）を区別
- プラグインを通じて画像やアーカイブの処理を拡張可能

### ApplicationSettings

- YAML 形式の設定ファイル（settings.yaml）の管理
- シングルトンパターンで実装
- 対応する画像/アーカイブの拡張子リストを保持

### BrowseHistory

- ユーザーの閲覧履歴を管理
- 前後のナビゲーションをサポート

## 開発時の注意点

### 画像処理

- 画像の表示には`BitmapImage`を使用
- メモリリークを防ぐため、使用後は適切にリソースを解放

### アーカイブ処理

- アーカイブ内のファイル名は日本語（Shift-JIS）に対応する必要がある
- `IArchive`オブジェクトは`IDisposable`を実装しているため、必ず`Dispose`する

### UI/UX の配慮

- 左ペインにファイルリスト、右ペインに画像表示の 2 ペイン構成
- キーボードショートカットのサポート（MainWindow.xaml.cs 内で定義）
- フォーカス管理に注意（ファイルリスト、画像表示の切り替え）

### テスト

- 単体テストは`TotoBook_Test`プロジェクトに配置
- テスト対象: Archive、FileNameComparer、MainWindowViewModel
- テストデータは`TotoBook_Test/testdata/`に配置

## ビルド・実行

```bash
# ビルド
dotnet build TotoBook.sln

# リリースビルド
dotnet build TotoBook.sln -c Release

# テスト実行
dotnet test TotoBook_Test/TotoBook_Test.csproj
```

## コード生成時の推奨事項

1. **MVVM パターンに従う**: View と ViewModel の責務を明確に分離
2. **日本語コメントを追加**: 特に複雑なロジックには詳細な説明を記述
3. **Susie プラグイン対応**: 画像読み込みは SpiManager を経由
4. **エンコーディングに注意**: アーカイブファイル名は Shift-JIS で処理
5. **リソース管理**: `IDisposable`を実装するオブジェクトは`using`文で管理
6. **非同期処理**: 画像読み込みなど時間のかかる処理は非同期化を検討
7. **エラーハンドリング**: ファイル I/O、画像読み込み時は適切な例外処理を実装

## 既知の設計パターン

- **Singleton**: `ApplicationSettings.Instance`
- **Observer**: ViewModel のプロパティ変更通知（`INotifyPropertyChanged`）
- **Command**: UI 操作を`ICommand`として実装
- **Factory**: `ArchiveFactory`によるアーカイブオブジェクト生成
