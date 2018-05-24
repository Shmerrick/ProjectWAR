# Character Utility #

## Usage ##

CharacterUtility.exe <verb> <options>

### Import ###

Import Character from Character-Export file.

Verb = import
Options = output-file=<filename>

### Export ###

Export Character to Character-Export file.

Verb = export
Options = output-file=<filename>

### Create ###

Create Character directly into DB.

Verb = create
Options = template-file=<filename>

### Item Set ###

Extend ItemSet data in DB.

Rebuild the values of the item set table (this creates and rebuilds the ItemSet additional fields), or Extract and view the values of the item set table

Verb = rebuild
Verb = view
Options = output-file=<filename>

### Examples ###

`CharacterUtility.exe itemset -r`
`CharacterUtility.exe export -output-file=g:\temp\somefile.dat`
`CharacterUtility.exe itemset -v -output-file=g:\temp\somefile.dat`

     