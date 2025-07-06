@echo off
:: Safe Cleaner by Peter Obala
:: Version 1.0 - 2025
:: Author: Peter Obala

:: Check if the script is running as administrator
reg query "HKU\S-1-5-19" >nul 2>&1
if %errorlevel% neq 0 (
    echo The script is not running as administrator. Restarting with administrator privileges...
    powershell -command "Start-Process '%~0' -Verb runAs"
    exit /b
)

:: Display header
echo =====================================================
echo Safe Cleaner - Developed by Peter Obala
echo Version 1.0 - 2025
echo =====================================================

:: Cleaning menu
echo Select folders to clean:
echo [1] Only SoftwareDistribution\Download
echo [2] Only Windows\Temp
echo [3] Only AppData Local Temp
echo [4] All three (default)
echo =====================================================
set /p choice=Enter your choice (1-4): 

:: Folder selection
set "dirpath1="
set "dirpath2="
set "dirpath3="

if "%choice%"=="1" set "dirpath1=C:\Windows\SoftwareDistribution\Download"
if "%choice%"=="2" set "dirpath2=C:\Windows\Temp"
if "%choice%"=="3" set "dirpath3=%LOCALAPPDATA%\Temp"
if "%choice%"=="4" (
    set "dirpath1=C:\Windows\SoftwareDistribution\Download"
    set "dirpath2=C:\Windows\Temp"
    set "dirpath3=%LOCALAPPDATA%\Temp"
)

:: Debug output
echo =====================================================
echo DEBUG: Selected folders:
echo SoftwareDistribution: %dirpath1%
echo Windows Temp: %dirpath2%
echo AppData Temp: %dirpath3%
echo =====================================================

:: Initialization of counters
set "total_files=0"
set "deleted_files=0"
set "failed_files=0"
set "failed_list="

:: Function to clean files with control
:delete_files
echo =====================================================
echo Cleaning folders using PowerShell...
echo =====================================================

if defined dirpath1 (
    echo Cleaning folder: %dirpath1%
    for /r "%dirpath1%" %%f in (*) do (
        set /a total_files+=1
        powershell -command "Remove-Item -Path '%%f' -Force -ErrorAction SilentlyContinue" || (
            set /a failed_files+=1
            echo %%f >> failed_files.txt
        ) && set /a deleted_files+=1
    )
)
if defined dirpath2 (
    echo Cleaning folder: %dirpath2%
    for /r "%dirpath2%" %%f in (*) do (
        set /a total_files+=1
        powershell -command "Remove-Item -Path '%%f' -Force -ErrorAction SilentlyContinue" || (
            set /a failed_files+=1
            echo %%f >> failed_files.txt
        ) && set /a deleted_files+=1
    )
)
if defined dirpath3 (
    echo Cleaning folder: %dirpath3%
    for /r "%dirpath3%" %%f in (*) do (
        set /a total_files+=1
        powershell -command "Remove-Item -Path '%%f' -Force -ErrorAction SilentlyContinue" || (
            set /a failed_files+=1
            echo %%f >> failed_files.txt
        ) && set /a deleted_files+=1
    )
)

:: Displaying results
echo =====================================================
echo  Safe Cleaner by Peter Obala - Results:
echo  Total files: %total_files%
echo  Successfully deleted: %deleted_files%
echo  Failed to delete: %failed_files%
if exist failed_files.txt (
    echo  List of files that could not be deleted:
    echo =====================================================
    type failed_files.txt
    del failed_files.txt
) else (
    echo  All files were successfully deleted.
)
echo =====================================================
echo Operation completed.
pause
