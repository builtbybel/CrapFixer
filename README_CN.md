# Crap F🧼xer (垃圾修复器)

# 一款说出大家心声的工具

## 如果微软也像我们一样痛恨臃肿软件，他们就会开发这款工具

还记得那些就算你其实并不需要，也还是会运行注册表清理器的日子吗？（或许我们当时确实需要？我可能太年轻搞不懂——对那些破事儿来说太年轻了 😅）<br>
那时候，像 CCleaner 这样的清理工具遍地都是；感觉每隔一个技术论坛就会有一个“十大 Windows 优化器”列表。

你可能会觉得，有了像 Windows 11 这样流畅的现代操作系统，这些东西应该早就过时了。嗯……内置的 Windows 清理器也许够用，当然。<br>
但事与愿违，“现代操作系统”反而给我们带来了一大堆新问题：开始菜单里的广告、令人毛骨悚然的数据收集，以及那些你没要求也删不掉的预装垃圾应用。<br>
都 2025 年了，我们居然还需要这类工具，这本身就挺离谱的。而且它们听起来也没什么不同——比如眼下这个：CrapFixer。<br>
我能说什么呢？科技圈就是这么个德行。

这是我个人用了很多年的一个小小的 IT 工具箱，专门用来清理和调整我经手的 Windows 系统。这个工具大约有 7 年历史了，但我对代码库进行了全面翻新，特别针对 Windows 11 进行了优化（也兼容 W10）。我还移除了大部分旧的企业版脚本，所以你现在看到的是一个轻量、易用且安全的应用程序。几乎你所做的每一个更改都是可以撤销的，所以你可以放心地进行调整。

CrapFixer 看起来仍然像是 Windows XP 时代的产物（也许叫 Crap Cleaner 更贴切 😄）——老实说，这正是我想要的效果。有时候，简单胜过花哨。两次点击：“分析”(Analyze)，检查结果，“修复”(Fix)——搞定。不拖泥带水，没有臃肿。

在清理我的 GitHub（30 多个仓库现在精简到 20 个）时，我也清理了数千行旧代码。有些项目来了又去，但 CrapFixer 一直都在。它快速、简单，而且基本上坚不可摧。反正我还没把系统搞崩过。😉<br>
如果你喜欢那些好用不花哨的老派工具，你会感觉宾至如归。<br>
如果大家感兴趣，我很快也会把更新后的代码提交到 GitHub。

![explorer_xu59FtMUnG](https://github.com/user-attachments/assets/fe462326-ebfb-41ea-83b5-d4cf72659c2d)


<details>
  <summary>💬 来自开发者的个人说明</summary>

如果你对这个项目及其他项目背后的个人故事感兴趣……
👉 [在此阅读完整故事](https://github.com/Belim/support/blob/main/STORY.md)

</details>


## 🚀 如何使用 CrapFixer

1.  **启动工具**  
    所有优化选项默认启用——无需进行任何调整。

2.  **分析你的系统**  
    点击 **“分析”(Analyze)** 按钮，根据所选设置扫描你的系统。

    -   **红色** 项目 = 建议修复
    -   **灰色** 项目 = 已经优化

3.  **应用修复**  
    猛击 **“运行 CFixer!”(Run CFixer!)** 按钮来应用建议的调整。

> ⚠️ **提示：** 要查看某个调整的具体作用，你可以 **右键点击** 项目并选择 **帮助(Help)** 或按下 **F1**。  
> 帮助系统还包含一个在线查找功能，可以为你在线搜索该调整项的信息。

5.  **🔍 可选：查看日志**  
    仍然不确定结果？  
    → 上传你的日志到 [在线日志分析器](https://builtbybel.github.io/CrapFixer/log-analyzer/index.html)  
    → 获取更改的详细分析  
    → 如果需要，可以分享链接以获取反馈

> ⚠️ **提示：** 为了获得完整功能，请以 **管理员身份** 运行 CrapFixer。  
> 某些修复（如 `HKEY_LOCAL_MACHINE` 下的注册表编辑）需要提升权限。

## ☕ 动力 ≈ 咖啡因

**CrapFixer** 是我最新——也可能是最后一款——为 Windows 精心打造的优化应用。  
我致力于让它长期保持活力，未来的开发将由 **自愿捐赠** 提供支持。

---

> 💡 每一杯咖啡大小的打赏不仅能催生新功能——  
> 也能降低我个人出现“**蓝屏死机**”的风险。😵‍💫

### 🙏 支持我的工作

如果你喜欢 CrapFixer，考虑给它“续杯”：

[![通过 PayPal 捐赠](https://img.shields.io/badge/Donate-PayPal-0070BA?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=M9DW4VNKH9ECQ)  
[![在 Ko-fi 上支持](https://img.shields.io/badge/Support-Ko–fi-F16061?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/builtbybel)

**感谢你为我“续命发电”！** ❤️

## 安装

*   从我的 [发布页面](https://github.com/builtbybel/CrapFixer/releases) 下载最新版本
*   解压压缩包

## 构建说明

-   安装 Visual Studio 2022+ 及 .NET 桌面开发工作负载
-   （可选但推荐）Windows 8.1+ SDK 以支持 WinRT
-   克隆仓库：
    ```bash
    git clone https://github.com/builtbybel/CFixer.git
    ```
-   打开解决方案或运行：
    *   Debug 构建：`msbuild CFixer.sln /p:Configuration=Debug`
    *   Release 构建：`msbuild CFixer.sln /p:Configuration=Release`

> ⚠️ 此项目使用 Windows.Management.Deployment API，它是 WinRT 的一部分。传统的 .NET Framework WinForms 项目本身不支持此 API。
> 要成功构建项目，你必须手动添加对 `Windows.winmd` 元数据文件的引用。
> 添加引用至 `C:\Program Files (x86)\Windows Kits\8.1\References\CommonConfiguration\Neutral\Annotated\Windows.winmd`

### 使用 Visual Studio (GUI) 构建
-   打开 `CFixer.sln`
-   将配置设置为 Release | Any CPU
-   按下 `Ctrl + Shift + B` 或使用 生成 → 生成解决方案

构建完成后，你可以在项目目录下的 `./bin/Debug/CFixer.exe` 或 `./bin/Release/CFixer.exe` 文件夹中找到可执行文件。运行 `CFixer.exe` 启动应用程序。

## 系统要求

-   Windows 11 (推荐)
-   Windows 10
-   需要管理员权限才能使用全部功能。

## 许可证

本项目采用 MIT 许可证 - 详情请参阅 [LICENSE](./LICENSE) 文件。