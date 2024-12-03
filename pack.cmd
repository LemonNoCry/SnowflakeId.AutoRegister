chcp 65001
@echo off

:: 设置解决方案路径和输出路径
set OUTPUT_PATH=nupkgs

:: 检查 .NET CLI 是否已安装
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo .NET CLI 未安装，请先安装 .NET SDK！
    exit /b 1
)

:: 确保输出路径存在
if not exist %OUTPUT_PATH% (
    mkdir %OUTPUT_PATH%
)

:: 打包解决方案
echo 正在打包解决方案
dotnet pack --configuration Release -o %OUTPUT_PATH%

:: 打包完成
if %errorlevel% neq 0 (
    echo 打包失败，请检查错误信息。
    exit /b 1
)

echo 打包成功，输出路径：%OUTPUT_PATH%
pause
