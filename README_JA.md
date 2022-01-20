# ARK: Server Utility
ARK: Survival Evolved's dedicated server' utility application. (飽きたら開発止めます)

# README
### [日本語](https://github.com/nattyan-tv/ark-server-utility/blob/master/README_JA.md)/[English](https://github.com/nattyan-tv/ark-server-utility/blob/master/README.md)


# Detail
ARK: Survival Evolvedの非公式サーバーを立てるソフトです。  
他に似たようなソフトがありますが関係ないです(ってかそっちの方が多分有能です)

# System
C#WPFでGUI部分を作り、Pythonでメインのプログラムを書くことで、簡単にGUIアプリを動かしています。  
C#だけだと俺の能力じゃ書けないし...PythonだけだとGUIがちょっと弱いし...ってなっていって、C#からPythonを呼び出すことにしました。  
まだまだプログラミング初心者なので是非色々教えてください。IssueとかPull requestとか大歓迎です。(プルリク送るときは僕に分かるように頑張って詳細を伝えてほしいな...())

# Install
Releaseに公開していく予定です。  
今のところは公開しません。  
自分でビルドしたいって方は、ご自身でCloneしていただき、Visual Studioを使ってビルドしてください。（開発環境はVS2022です）

# Device
.NET Framework 4.8に対応したx64デバイス  
(64bitのWindows7/8.1/10/11)  

# License
本コードのライセンスは、MIT Licenseとして[LICENSE](https://github.com/nattyan-tv/ark-server-utility/blob/master/LICENSE)に記載されています。  
（MIT Licenseというのは、「ソフトウェアを自由に扱える」「再頒布時に著作権とライセンスの表示が必要」「著作者はいかなる責任も負わない」という感じです）
