-- Create Backups table for tracking backup history
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Backups]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Backups](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [FilePath] [nvarchar](500) NOT NULL,
        [FileName] [nvarchar](200) NOT NULL,
        [FileSize] [bigint] NOT NULL,
        [BackupType] [nvarchar](50) NOT NULL,
        [Status] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [CreatedAt] [datetime2] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [nvarchar](200) NULL,
        [ErrorMessage] [nvarchar](500) NULL,
        [ExpiresAt] [datetime2] NULL,
        [CompressionType] [nvarchar](50) NULL,
        [IsEncrypted] [bit] NOT NULL DEFAULT 0,
     CONSTRAINT [PK_Backups] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

PRINT 'Backups table created successfully!'
