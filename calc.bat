@echo off
cd "%~dp0%"
set NODE_REPL_HISTORY=.repl_history
"node.exe" -i -e "require('./index.js')" --title=Calculator
