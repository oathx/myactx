cd ..\data\client\
del ..\client_error.log /Q
svn up
svn st > ..\st.log
..\..\export\lua.exe check_client.lua >> ..\st.log
if exist ..\client_error.log (
	echo "client script error, please check it."
	notepad ..\client_error.log 
) else ( 
	echo --------------------------------------------------------- >> ..\st.log
	svn diff >> ..\st.log
	notepad ..\st.log
)