@echo off

if "%1" == "" (
    echo %1
    echo no tags, exiting...
    exit /b 1
)

for %%I in ("%batch_dir%.") do set "repo_name=%%~nxI"
echo repo_name: %repo_name%

echo start push subtree...
set "git_command=git subtree push --prefix=Unity/Assets/%repo_name% https://github.com/seinocat/%repo_name%.git upm"
%git_command%

echo start pull...
git pull

for /f "tokens=1" %%i in ('git rev-parse origin/upm') do set "commit_hash=%%i"
echo remote upm hash: %commit_hash%

echo start add tag %1
git tag -a %1 -m '%2' %commit_hash%
git push origin %1

