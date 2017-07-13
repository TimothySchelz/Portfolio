Author: OperationBundtCake: Scott Steadham, David Reeves, Timothy Schelz, Nazer Abu-Rmaileh

26 September 2016
My initial design thoughts about the project are to create a Cell class that
contains a Content Property. I will use a DependencyGraph to keep track of
dependency relations. I will need to use the Formula class to see what 
variables(names) a Formula contains. I will have a Dictionary of filledCells that
map cell names to their cells. The cells in the dictionary will not be empty.
If the cells become empty they will be removed from the dictionary. If a cell has 
no dependency and nothing depends on it, it will be removed from the dictionary.

5 October 2016
This is the second version of the spreadsheet. This version accomodates a cells
value. When a cell contents are set, this spreadsheet computes the values of all
the cells that were involved.
The same DependecyGraph keeps track of the depedency relations, and the same Formula
class evaluates a cell formula's value.
This version allows users to save a spreadsheet to file and load a spreadsheet from
file. The version of the spreadsheet is saved in the file as and the spreadsheet can 
only work with exact versions.

2 November 2016
This is the third version of the spreadsheet. This version represents a fully operating 
spreadsheet, complete with GUI component. The spreadsheet includes functionality to save
and open spreadsheets from file. The spreadsheet files have the extension ".sprd". The
spreadsheet windows funtion independently, meaning multiple spreadsheets can be opened at
the same time. When closing a spreadsheet, a prompt asks the user if they would like to save.
This spreadsheet also has a help menu. Information on the UI of the spreadsheet are available.
We also added a graphing functionality to this spreadsheet. The user can graph a sequence of 
cells. Try it out!
We also added an installer supports the spreadsheet being opened when file type of .sprd is 
clicked on.

25 April 2017
This is a server-client version of the spreadsheet.

Build Info:
I am building against the latest version of PS2(SpreadsheetUtilities.dll), 
		committed on 9/29/2016 11:32:21 AM After Build.
I am building against the latest version of PS3(Formula.dll), 
		committed on 9/26/2016 11:37:22 AM Enabled .XML docs to generate.
I am building against the latest version of SpreadsheetPanel provided by instructor,
		committed on 11/2/2016 03:01:19 PM After Build.
 
Notes to the grader:
-Please look at the help menu items for learning about how our spreadsheet works.
Basically we added functionality for using arrow, tab, and enter keys to orient 
through the spreadsheet.
-Cell values compute with the [Enter] button or by selecting a different cell either
with mouse click or keys.
-We added a graph component as our extra feature. Once data is entered into the spreadsheet,
you can Generate a graph. The graphing wizard will ask you to specify the cells for the 
x and y axis. To enter a range of cells use the "-" to seperate the cell names. 
For example: A1-Z1. The ranges can not skip cells, or go diagonally.
-We also added undo and redo hot keys. Look at the help menu to learn how to undo and redo.

And as always...

 ********** **                         **                                     **
/////**/// /**                        /**            **   **                 /**
    /**    /**       ******   ******* /**  **       //** **   ******  **   **/**
    /**    /******  //////** //**///**/** **         //***   **////**/**  /**/**
    /**    /**///**  *******  /**  /**/****           /**   /**   /**/**  /**/**
    /**    /**  /** **////**  /**  /**/**/**          **    /**   /**/**  /**// 
    /**    /**  /**//******** ***  /**/**//**        **     //****** //****** **
    //     //   //  //////// ///   // //  //        //       //////   ////// //