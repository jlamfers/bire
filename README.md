# bire
binary replace utility
  replaces text patterns inside all kinds of files, as well as inside path names 


# Download
Download bire.exe from the release in GitHub.
[https://github.com/martdegraaf/bire/releases/tag/1.0.0](https://github.com/martdegraaf/bire/releases/tag/1.0.0)
# Example use
Run the bire.exe using the following command:
``` powershell
bire -from C:\git\source -to C:\git\target -replace this=that Something=Anything
```
Replacements are made in file sources as well as file paths.


# Usage
```
bire -from <directory-or-zip> [-to <target-directory-or-zip>][-replace<fields>]
[-clearTarget] [-skip <skip-extensions>] [-ignore <default | <ignore-regex>>]

or: bire -scaffold <directory> [-to <zip-target>] [-ignore <ignore-regex>]

 <directory-or-zip>        : existing directory or zip-file. it may contain a file named boilerplateinfo.json
 <target-directory-or-zip> : target directory or zip-file. it is newly created
 <fields>                  : space separated field-value pairs like {<fieldname>=<fieldvalue>}
                             all of these fields either override or fill up configured fields
                             in boilerplateinfo.json
 -clearTarget              : target is cleared before build
 <skip-extensions>         : space separated file extensions (dot included like.exe) that do not
                             need content processing but must be copied. Default is
                             .exe, .dll, .obj, .pdb, .zip, .nupkg
 <ignore-regex>            : regex that matches all file and directory names that must be ignored,
                             and not copied
 -ignore default           : equals "-ignore (.*(\.|\/|\\)(exe|dll|obj|bin|pdb|zip|\.git|\.vs|cache|packages))$"

 -scaffold                 : when using -scaffold the source must be a directory and the optional
                             target must be a zip. When omitted then <target> gets the same name
                             as <directory> with extension .zip. <directory> must include the file
                             boilerplateinfo.json
```

# Contributing

## Getting the bire.exe from sources

To get the bire.exe:
- Build the Console project.
- Run the ilmerge-bire.bat inside "Bire.Console\bin\Release" to build the exe file.
