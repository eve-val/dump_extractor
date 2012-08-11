CCP Dump Extractor
==================

The CCP Dump Extractor is the tool Eve-Val uses to extract the whole CCP Static Data Dump from its MS SQL format to a easier-to-use YAML format.

Dependencies
------------
The Dump Extractor requires a the .NET Runtime 4.0. If you don't have that runtime, download and install it from [Microsoft](http://www.microsoft.com/en-us/download/details.aspx?id=17718) first.

If you are developing on the Dump Extractor, however, the following software is required to run the tests and generate an MSI:

 - [`NUnit`](http://nunit.org/?p=download)
 - [`WiX`](http://wix.codeplex.com/releases/view/60102)


Usage
-----

**NOTE**: The following guide is for using the GUI tool. If you want to use the command line tool, open a command line, execute DumpExtractorCLI.exe, and a help message will display.

1. Make sure you have access to a MS SQL Server loaded with the [CCP Static Dump](http://community.eveonline.com/community/toolkit.asp).
2. Install the GUI using the provided MSI
3. Open `Dump Extractor 1.0` **not** `Dump Extractor 1.0 CLI`.
4. If the following conditions are true, go to step 6:
    - Your SQL Server is installed on your local machine
    - You didn't change the database name from ebs_DATADUMP when you restored it
    - You changed none of the authentication parameters
5. If all you are here because of one of the first two items, you can just make the appropriate changes to the connection string in the textbox of the prorgam (change 'localhost' to the address of your SQL Server, change 'ebs_DATADUMP' to your database name). If you changed your authentication option, I'm going to assume you know how to generate an appropriate Connection String, because I have no idea :)
6. Click 'Load Tables'
7. Select the tables you wish to export to YAML
8. Click 'Browse', and specify the file you want the YAML to be saved to. **NOTE**: the selected file will be overwritten if it exists!
9. Click 'Extract!'. A progress bar will slowly fill, and when the extraction is done, a dialog box will pop up.
10. ???
11. Profit!

YAML Format
-----------

The extractor outputs a YAML document per table it extracts, using the stream delimiter style of document indicators -- each document starts with `---` and ends with `...` on a line by themselves. The actual document is encoded with the flow style, for compactness/sanity's sake. Each document is a dictionary containing the keys `table_name`, `columns`, and `data`. `table_name` is a string containing the name of the extracted table. `columns` is a list of strings which are the names of the columns in the table. `data` is a list of dictionaries, where the keys in each dictionary are the names of the columns, and the values are the retrieved values for that row. If a value is NULL in the database, it's represented in the YAML by omitting that column's key from the row dictionary. For example:

    ---
    {table_name: 'testTable', columns: [ 'col1', 'col2'], data: [{col1: 'some text', col2: !!float 1.2}, {col1: "row with NULL"}]}
    ...
