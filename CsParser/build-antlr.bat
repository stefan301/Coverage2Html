@echo off
setlocal enableDelayedExpansion

:: Arbeitspfad auf den Pfad des Batchskripts setzen
pushd "%~dp0"

call ..\antlr\antlr4.bat -Dlanguage=CSharp -o generated -package CsParser -listener CSharpLexer.g4
pause