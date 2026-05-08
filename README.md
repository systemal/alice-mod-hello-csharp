# Hello C# Plugin for Alice

A demo C# plugin for the [Alice](https://github.com/systemal/Alice) AI Agent Runtime Platform.

Tests all `alice.*` APIs: log, fs, kv, service, event, platform, timer.

## Install

In Alice Manager: **Install from GitHub** → paste this repo URL.

Or manually:
```bash
git clone https://github.com/systemal/alice-mod-hello-csharp.git
dotnet build -c Release -o <alice>/mods/hello-csharp/ -p:AliceSdkDll=<alice>/sdk/Alice.SDK.dll
```

## Structure

```
alice.json          — plugin manifest
HelloCSharp.csproj  — project file (references Alice.SDK via DLL)
PluginEntry.cs      — plugin entry point
```
