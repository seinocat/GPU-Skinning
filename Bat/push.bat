@echo off

for %%I in ("%batch_dir%.") do set "repo_name=%%~nxI"
echo set repo_name: %repo_name%

echo start push subtree...
set "git_command=git subtree push --prefix=Unity/Assets/%repo_name% https://github.com/seinocat/%repo_name%.git upm"
%git_command%