IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ItemProcessingDB')
BEGIN
    CREATE DATABASE ItemProcessingDB;
END
GO

USE ItemProcessingDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
BEGIN
    CREATE TABLE Items (
        ItemId    INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
        Name      NVARCHAR(100) NOT NULL,
        Weight    FLOAT         NOT NULL,
        CreatedAt DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemRelations')
BEGIN
    CREATE TABLE ItemRelations (
        RelationId   INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        ParentItemId INT NOT NULL,
        ChildItemId  INT NOT NULL,

        CONSTRAINT UQ_ItemRelations UNIQUE (ParentItemId, ChildItemId),
        CONSTRAINT FK_ItemRelations_Parent FOREIGN KEY (ParentItemId) REFERENCES Items(ItemId),
        CONSTRAINT FK_ItemRelations_Child  FOREIGN KEY (ChildItemId)  REFERENCES Items(ItemId)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ItemRelations_Parent')
    CREATE INDEX IX_ItemRelations_Parent ON ItemRelations(ParentItemId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ItemRelations_Child')
    CREATE INDEX IX_ItemRelations_Child ON ItemRelations(ChildItemId);
GO

SELECT t.name AS TableName, p.rows AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
ORDER BY t.name;
GO