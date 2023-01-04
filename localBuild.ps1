#requires -Modules UnitySetup

for (($i = 0); $i -lt 1; $i++)
{
#-Project "VR\Test"
    Start-UnityEditor -BuildTarget Android -ExecuteMethod TestBuilder.Build -BatchMode -Quit -LogFile ".\build$(get-date -f yyyy-MM-dd-hh-ss).log" -Wait
}