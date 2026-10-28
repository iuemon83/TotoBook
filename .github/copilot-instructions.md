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

## 開発フロー

### 検証用ビルド

ユーザーは基本的にReleaseビルドで動作確認する。検証用の成果物は `dotnet build TotoBook.sln -c Release` で生成し、テストも `dotnet test TotoBook_Test\TotoBook_Test.csproj -c Release` で実行する。Debugビルドだけで確認を完了とせず、ユーザーへ案内する実行ファイルは今回生成したRelease成果物のパスを示す。

### アプリのバージョン管理

アプリの変更は、実装や依存関係の更新だけで完了としない。配布物の変更を識別できるよう、アプリ自体のバージョン更新を開発フローに含める。

1. **変更前に確認する。** `TotoBook\TotoBook.csproj` の `Version` と、明示されている場合は `AssemblyVersion`・`FileVersion` を確認する。バージョン指定がない場合も、ビルド済みアセンブリなどで現在値を確認し、以後は `Version` を明示して管理する。
2. **アプリに影響する変更では更新する。** 機能追加、不具合・脆弱性修正、.NETや本体の依存ライブラリの更新を対象とする。.NETやライブラリのバージョン更新を、アプリのバージョン更新の代わりにしてはいけない。文書のみ、またはテスト・開発ツールのみで配布物に影響しない変更は対象外とする。
3. **変更内容に応じて採番する。** 利用者に互換性を壊す変更や必須実行環境の変更はメジャー、互換性を保つ機能追加はマイナー、不具合・脆弱性修正や互換性を保つ依存更新はパッチを上げる。上位桁を上げた場合は下位桁を0にする。利用者からバージョン指定があれば優先する。同じ一連の変更では一度だけ更新し、既にその変更を反映した番号なら再更新しない。
4. **実際のバージョンを検証する。** 更新後にReleaseビルドと変更に応じたテストを実行する。タイトルバーはアセンブリのバージョンを表示するため、`Version` の記載だけで判断せず、生成されたRelease成果物の `AssemblyVersion`・`FileVersion` とタイトルバーの表示値が意図した値になることを確認する。既存の明示指定が自動生成値を上書きしていないかにも注意する。
5. **完了報告に含める。** アプリの変更前後のバージョン、検証結果、コミット・pushの実施状況を報告する。更新対象なのにバージョン更新や検証が未実施なら、作業完了としない。更新対象外の場合はその理由を示す。コミット・pushは依頼された場合に行う。
