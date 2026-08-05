# Running the Electricity Trading Portal in WSL

Step-by-step setup and run guide for Windows Subsystem for Linux (WSL2).
This project is an ASP.NET Core 10 app, so the only real prerequisite is a
.NET 10 SDK with the ASP.NET Core runtime on the Linux side.

## Prerequisites

- **Windows 10/11** with **WSL2** installed and a Linux distro enabled
  (Ubuntu 22.04/24.04 recommended).
  Verify with:
  ```bash
  wsl --status
  ```

*Inside WSL*
## 1. Install dotnet 

```bash
sudo apt update && sudo apt upgrade -y
```

```bash
sudo apt install -y dotnet-sdk-10.0
```

## 2. Check for an existing .NET installation

```bash
dotnet --list-sdks
```

- If this lists a **10.x** SDK and `dotnet --list-runtimes` shows
  `Microsoft.AspNetCore.App 10.x`, skip to step 3 and 4.
- If a version of `dotnet` exists but the ASP.NET Core 10 runtime is missing,
  you will hit the `NETSDK1226: Prune Package data not found` build error — see
  the Troubleshooting section.

## 3. Verify:

```bash
dotnet --version      # should print a 10.0.x
dotnet --list-runtimes   # should include Microsoft.AspNetCore.App 10.x
```

## 4. Clone the code inside WSL

```bash
git clone https://github.com/shahgnp/12_factor_app.git
```

## 5. Open in VSCode

```bash
cd 12_factor_app
code ElectricityTradingPortal-new
code ElectricityTradingPortal-old
```
*Two VScodes should open*

## 6. Go to Lab 1

[https://github.com/shahgnp/12_factor_app/blob/main/docs/Lab1.md](https://github.com/shahgnp/12_factor_app/blob/main/docs/Lab1.md)