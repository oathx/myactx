cd ..\data\server\
svn up
if not exist ..\client_error.log (
svn st > ..\st.log
echo --------------------------------------------------------- >> ..\st.log
svn diff >> ..\st.log
notepad ..\st.log
)