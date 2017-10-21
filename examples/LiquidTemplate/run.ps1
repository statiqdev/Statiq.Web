Param(
    [switch]$attach
)

$AllArgs = @('-a', '../../../../extensions/**/bin/Debug/*.dll')
if ($attach) {
    $AllArgs += '--attach'
}
 
& "${pwd}\..\..\src\clients\Wyam\bin\Debug\wyam.exe" $AllArgs