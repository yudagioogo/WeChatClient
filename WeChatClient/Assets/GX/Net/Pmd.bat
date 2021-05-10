@echo off
pushd %~dp0
upa PlatCommon git@git.code4.in:mobilegameserver/platcommon.git master

call PlatCommon\clear.bat
call protogx unity-json PlatCommon

del /Q /S "Pmd\*.proto.cs"
move /Y PlatCommon\*.proto.cs Pmd

call:clearMeta .

if "%1"=="" pause
GOTO:EOF

rem ===================================================================
rem 递归删除给定目录中所有孤立的*.meta文件
rem 参数: 路径
:clearMeta
echo clear meta: %~1
for /f "tokens=* delims=" %%i in ('dir /b /s "%~1\*.meta"') do (
	if not exist "%%~dpni" (
		echo %%i
		del /Q "%%i"
	)
)
GOTO:EOF