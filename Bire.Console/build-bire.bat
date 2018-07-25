md old
copy *.pdb old\*.pdb
del *.pdb
ilmerge /out:bire.exe bire.console.exe bire.dll
copy old\*.* .\*.*
del old\*.pdb
rd old
