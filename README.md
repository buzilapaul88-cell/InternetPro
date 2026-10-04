# Internet Pro

Windows desktop browser built with WinForms and Microsoft Edge WebView2.

## Run

Open `InternetPro.csproj` in Visual Studio, or build from PowerShell:

```powershell
dotnet publish .\InternetPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The executable is written to `bin\Release\net10.0-windows\win-x64\publish\InternetPro.exe`. Microsoft Edge WebView2 Runtime must also be installed on the PC.

## Filter scope

Roblox, Minecraft, and TikTok are always blocked inside Internet Pro and cannot be switched off in the UI. Roblox links, known store listing identifiers, and downloads whose URL or filename identifies Roblox are also blocked in this browser. A download using an unrelated URL and filename may not be identifiable. Roblox/Minecraft/TikTok domain lists are local and not continuously updated; they do not cover every possible game-server address. The adult-site and advertising filters are enabled by default and can be switched off. YouTube is the home page and its video domains are not blocked.

The reputation checkbox controls Edge WebView2's reputation checking. It does not discover or automatically patch new software vulnerabilities. Keep Windows and the WebView2 Runtime updated. This browser does not provide network-wide filtering, DDoS protection, or a way to disable Roblox Hyperion. Edge WebView2 does not support ActiveX or Flash plugins.

## Developer mode and policies

Open `Instrumente > Mod dezvoltator și liste de site-uri` to edit local blocked and allowed domains. Enter one hostname per line. Rules are stored in `%LOCALAPPDATA%\InternetPro\policy.json`; JSON policies can be imported or exported for other Internet Pro installations. Sharing a file does not deploy it to other users or devices. Built-in Roblox, Minecraft, and TikTok blocks always take precedence over the allowed list. The allowed list only overrides entries in the custom blocklist and the optional adult/advertising filters.

The browser can block domains and known phishing sites flagged by Edge reputation checks. It cannot block installed apps, identify every scam clone, screen scam callers or SMS, or manage SIM/eSIM settings. Use device-level security and carrier protections for those functions.

Password saving and autofill are disabled. The browser does not store passwords. The clear-data command removes the WebView2 profile's browsing data, including cookies, history, and cache.

## Settings, themes, and bookmarks

The toolbar has a **Setări** button for the home page, saved filter switches, payment protection, and interface theme. Themes include Windows 11, 10, 8, 7, Vista, 98, and 95, plus Frătăuții Vechi, Maramureș, candy colors, galaxies, and an original pixel-cat/rainbow illustration. Themes change Internet Pro's browser chrome; they do not change the appearance of websites. The star button saves the current page; saved bookmarks remain in the local Internet Pro policy file and can be opened or removed from **Favorite > Semne de carte**.

Payment protection blocks HTTP addresses whose URL clearly indicates checkout, billing, payment, or card details. It does not inspect every form, certify a merchant, or replace a bank's fraud protection. Keep Edge reputation checking enabled and only enter payment details on trusted HTTPS sites.