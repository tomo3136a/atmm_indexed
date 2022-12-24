# atmm_indexed

## What?

This is my working file manipulator tool.

I want to create a document.
However, I'm not good at managing document files.  Of course, text files such as source code use a version control system.
The issue is office documents.
It can a version control system, but it's not for a purpose.  
Therefore, by establishing my own rules, I created a tool that
I could use casually for the purpose of structural management.

## Scene 1

Editting file backup

From the Explorer context menu, duplicate the code based
on the last edit date into a file named after the file. (Snapshot)

In my experience, document files are not often created in one go,
and they are edited and saved repeatedly.
The challenge is how to distinguish between old edited
and modern document files.
I defined it as follows:

* Old edited document files give time series code to file names.
* The document file to be edited must not have time series code in the file name.
* The code for the time series is formed by the characters
  in the order of recording within the year, month, day, and edit date.  
  (e.g 20210901, 20210901a)

## Scene 2

Old editting file move to archive folder

## Scene 3

View the working status of the file

## Scene 4

Time series in date folder
