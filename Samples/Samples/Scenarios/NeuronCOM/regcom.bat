@echo off


regasm bin\Release\Neuron.ESB.Excel.Interop.dll /u
regasm bin\Release\Neuron.ESB.Excel.Interop.dll /tlb:Neuron.ESB.Excel.Interop.tlb /codebase

@REM - below is for execution from 64 bit processes

%SYSTEMROOT%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe bin\Release\Neuron.ESB.Excel.Interop.dll /u
%SYSTEMROOT%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe bin\Release\Neuron.ESB.Excel.Interop.dll /tlb:Neuron.ESB.Excel.Interop.tlb /codebase
