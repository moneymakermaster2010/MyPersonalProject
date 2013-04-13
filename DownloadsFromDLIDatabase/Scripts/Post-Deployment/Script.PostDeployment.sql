/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

CREATE TABLE [User]
(
	UserID int PRIMARY KEY IDENTITY(1,1),
	UserName varchar(200) NOT NULL,
	CreatedDate datetime DEFAULT GETDATE(),
	LastModifiedDate datetime DEFAULT GETDATE(),
	Active bit DEFAULT 1,
)

CREATE TABLE Book_Detail
(
	Book_DetailID int PRIMARY KEY IDENTITY(1,1),
	BookName varchar(500) NOT NULL,
	Author varchar(1000),
	BookDescription varchar(1000),
	BookSourceURL varchar(1000),
	CreatedBy int FOREIGN KEY REFERENCES [User]([UserID]),
	UpdatedBy int FOREIGN KEY REFERENCES [User]([UserID]),
	CreatedDate datetime DEFAULT GETDATE(),
	LastModifiedDate datetime DEFAULT GETDATE(),
	Active bit DEFAULT 1,
)

CREATE TABLE Book_Page_Content
(
	Book_Page_ContentID int PRIMARY KEY IDENTITY(1,1),
	BookID int FOREIGN KEY REFERENCES Book_Detail(Book_DetailID) NOT NULL,
	PageNumber bigint NOT NULL,
	Content varbinary,
	CreatedBy int FOREIGN KEY REFERENCES [User]([UserID]),
	UpdatedBy int FOREIGN KEY REFERENCES [User]([UserID]),
	CreatedDate datetime DEFAULT GETDATE(),
	LastModifiedDate datetime DEFAULT GETDATE(),
	Active bit DEFAULT 1,
)

INSERT INTO [User](UserName) VALUES ('RajeshA')