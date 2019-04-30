cd ..\data\client\
if exist ..\client_error.log (
	echo "find client_error.log, please check it."
) else (
	svn up
	svn ci -m "this is windows bat commit. "
)
pause
cd ..\..\export\output\client\