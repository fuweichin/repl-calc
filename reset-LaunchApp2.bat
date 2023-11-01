@echo off
REG DELETE HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AppKey\18 /v ShellExecute /f
pause
