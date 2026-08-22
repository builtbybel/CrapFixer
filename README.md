<blockquote class="twitter-tweet"><p lang="en" dir="ltr">it&#39;s called CrapFixer. let that clean in🧽<a href="https://t.co/UP2iLnAgif">https://t.co/UP2iLnAgif</a><a href="https://twitter.com/hashtag/CrapFixer?src=hash&amp;ref_src=twsrc%5Etfw">#CrapFixer</a> <a href="https://twitter.com/hashtag/Windows?src=hash&amp;ref_src=twsrc%5Etfw">#Windows</a> <a href="https://twitter.com/hashtag/Windows11?src=hash&amp;ref_src=twsrc%5Etfw">#Windows11</a> <a href="https://twitter.com/hashtag/app?src=hash&amp;ref_src=twsrc%5Etfw">#app</a> <a href="https://twitter.com/hashtag/microsoft?src=hash&amp;ref_src=twsrc%5Etfw">#microsoft</a> <a href="https://t.co/OMruEjvuUb">pic.twitter.com/OMruEjvuUb</a></p>&mdash; Belim (@builtbybel) <a href="https://twitter.com/builtbybel/status/1917594071582773272?ref_src=twsrc%5Etfw">April 30, 2025</a></blockquote>


# Crap F🧼xer – Fixes the crap Windows leaves behind.

## The tool that says what everyone's thinking

Remember the days when you'd run a registry cleaner even if you didn't really need it? (Or maybe we did need it? I was probably too young to figure that out - too young for that crap 😅) <br>Back then, cleaner tools like CCleaner were everywhere; it felt like every other tech forum had a "top 10 Windows Optimizers" list.

You'd think that stuff would be over by now with sleek, modern OSes like Windows 11. Well... the built-in Windows cleaner might be enough, sure. <br>But instead, the "modern OS" blesses us with a whole new batch of problems: ads in the Start menu, creepy data collection, and preinstalled junk apps you didn't ask for and can't easily remove. <br>It's kind of wild that we still need tools like this in 2025. And they don't even sound that different - like this one here: CrapFixer. <br>What can I say? The tech world is a bitch.

This is my personal little IT toolbox that I've been using for years to clean up and tweak Windows systems I work on. The tool is about 7 years old, but I've given the codebase a full refresh, especially tuned for Windows 11 (works on W10 too). I've also removed most of the old enterprise scripts, so what you're seeing now is a lightweight, easy-to-use, and safe app. Almost every change you make can be undone, so you can tweak without fear.

CrapFixer still looks like something straight out of the Windows XP era (maybe Crap Cleaner 😄) - and honestly, that's exactly the vibe I was going for. Sometimes simple just beats fancy. Two clicks: 'Analyze', check the results, 'Fix' - done. No drama, no bloat.

While cleaning up my GitHub (30+ repos down to 20 now), I also cleaned up thousands of lines of old code. Some projects come and go, but CrapFixer stays. It's fast, simple, and basically bulletproof. I haven't managed to break anything yet. 😉 <br>If you like old-school tools that just work, you're gonna feel right at home. <br>

![CrapFixer](https://github.com/user-attachments/assets/cff37456-5676-4e3a-85a7-0c71e39e2bd0)


<details>
  <summary>💬 A personal note from the developer</summary>

If you're curious about the personal story behind this project and others...
👉 [Read the full story here](STORY.md)

</details>


## 🚀 How to Use CrapFixer

1. **Launch the Tool**  
   All optimization options are enabled by default – no need to tweak anything.

2. **Analyze Your System**  
   Click **"Analyze"** to scan your system based on the selected settings.

   - **Red** items = Recommended fixes
   - **Gray** items = Already optimized

3. **Apply Fixes**  
   Smash the **"Run Fixer"** button to apply the recommended tweaks.
   
> ⚠️ **Tip:** To view what a tweak does you can **Right-Click** on an item and select **Help** or hit **F1**.  
> The help system also includes an online lookup that will search the tweak online for you.

5. **🔍 Optional: Review the Log**  
   Still unsure about the results?  
   → Upload your log to the [Online Log Analyzer](https://builtbybel.github.io/CrapFixer/log-analyzer/index.html)  
   → Get a breakdown of the changes  
   → Share the link for feedback if needed

> ⚠️ **Tip:** For full functionality, run CrapFixer as **Administrator**.  
> Some fixes (like registry edits under `HKEY_LOCAL_MACHINE`) need elevated permissions.

## ☕ Motivation ≈ Caffeine

**CrapFixer** is my newest — and likely last — fine-tuning app for Windows.  
I'm committed to keeping it alive for the long haul, and future development will be powered by **voluntary donations**.

---

> 💡 Every coffee-sized tip not only fuels new features —  
> it also lowers the risk of *me* throwing a personal **Blue Screen of Death**. 😵‍💫

### 🙏 Support My Work

If you like CrapFixer, consider keeping it caffeinated:

[![Donate via PayPal](https://img.shields.io/badge/Donate-PayPal-0070BA?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=M9DW4VNKH9ECQ)  
[![Support on Ko‑fi](https://img.shields.io/badge/Support-Ko–fi-F16061?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/builtbybel)

**Thank you for keeping the lights on!** ❤️

## Installation

* Download the latest release from my [releases page](https://github.com/builtbybel/CrapFixer/releases)
* Extract the archive

## Build Instructions

- Install Visual Studio 2022+ with .NET Desktop workload
- (Optional but recommended) Windows 8.1+ SDK for WinRT support
- Clone the repo:  
  ```bash
  git clone https://github.com/builtbybel/CFixer.git
- Open the solution or run:
  * Debug build: msbuild CFixer.sln /p:Configuration=Debug
  * Release build: msbuild CFixer.sln /p:Configuration=Release

> ⚠️ This project uses the Windows.Management.Deployment API, which is part of WinRT. Classic .NET Framework. WinForms projects do not support this out of the box.
> To build the project successfully, you must manually add a reference to the Windows.winmd metadata file.
Add Reference to `C:\Program Files (x86)\Windows Kits\8.1\References\CommonConfiguration\Neutral\Annotated\Windows.winmd`

### Build with Visual Studio (GUI)
- Open CFixer.sln
- Set the configuration to Release | Any CPU
- Press `Ctrl + Shift + B` or use Build → Build Solution
  
After building, you can find the executable in the `./bin/Debug/CFixer.exe` or `./bin/Release/CFixer.exe` folder inside the project directory. Run CFixer.exe to start the app.
  
## System Requirements

- Windows 11 (Recommended)
- Windows 10
- Administrator privileges required for full functionality.

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details.
