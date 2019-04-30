cd ..\data\server\
if not exist ..\client_error.log (
svn cleanup
svn up
svn ci -m "this is windows bat commit. "
pause
)