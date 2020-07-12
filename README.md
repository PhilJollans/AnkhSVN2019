# AnkhSVN2019

AnkhSVN2019 is a fork of the Github project  
https://github.com/simonp22/AnkhSVN

That project is in turn based on the original [CollabNet project](https://ankhsvn.open.collab.net/source/browse/ankhsvn/)

The SVN repository of the CollabNet project is at  
https://ctf.open.collab.net/svn/repos/ankhsvn

## Target environment

AnkhSVN2019 is targeted only at Visual Studio 2019.

## Version history

| Version       | Date              | Description
| ------------- | ----------------- | ----------------------- 
| 1.00.0001     | 14-December-2019  | Initial release
| 1.00.0002     | 17-December-2019  | Fix handling of (dark and light) themes.<br/>Include some license files for open source libraries. 
| 1.00.0003     | 21-December-2019  | Temporarily disable the Annotate function.<br/>In the previous version this function caused Visual Studio to crash.  
| 1.00.0004     | 22-December-2019  | Catch an EntryPointNotFoundException on calling SetThreadDpiAwarenessContext. See Issue 3.  
| 1.00.0005     | 05-January-2020   | New implementation of the GUI for the Annotate function.
| 1.00.0007     | 13-January-2020   | Restructured implementation the Annotate function using a custom margin on an editor window.
| 1.00.0008     | 01-March-2020     | Add an exception handler to AnnotationMarginFactory.CreateMargin. See Issue 9.
| 1.00.0009     | 19-April-2020     | Issues 7, 11, 12 and 13.
| 1.00.0010     | 12-July-2020      | Issue 18.

