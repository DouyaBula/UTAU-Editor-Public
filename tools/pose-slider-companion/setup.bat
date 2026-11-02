@echo off
setlocal
cd /d "%~dp0"

if not exist .venv\Scripts\python.exe (
    py -3.12 -m venv .venv
    if errorlevel 1 (
        echo Python 3.12 was not found. Install Python 3.12 and try again.
        pause
        exit /b 1
    )
)

.venv\Scripts\python.exe -m pip install --upgrade pip
.venv\Scripts\python.exe -m pip install -r requirements.txt
if errorlevel 1 (
    echo Dependency installation failed.
    pause
    exit /b 1
)

echo Setup complete. Run run.bat to open the pose extractor.
pause
