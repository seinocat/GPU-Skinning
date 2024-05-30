git pull

for /f "tokens=1" %%i in ('git rev-parse origin/upm') do set "commit_hash=%%i"
echo remote upm hash: %commit_hash%

git tag -a %1 -m '%2' %commit_hash%
git push origin %1