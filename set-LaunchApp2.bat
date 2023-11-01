@echo off
REG ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AppKey\18 /v ShellExecute /t REG_SZ /d %~dp0%SingleInstance.exe /f
pause
