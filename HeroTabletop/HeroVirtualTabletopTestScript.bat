@echo off
if exist result_herovirtualtabletop.trx (
     del result_herovirtualtabletop.trx)
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\mstest.exe"  /testcontainer:bin\debug\herovirtualtabletop.dll /resultsfile:result_herovirtualtabletop.trx 