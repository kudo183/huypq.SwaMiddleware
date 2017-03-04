
GO
/****** Object:  Table [dbo].[SwaGroup]    Script Date: 11/22/2016 2:19:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SwaGroup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[GroupName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SwaUser]    Script Date: 11/22/2016 2:19:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SwaUser](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[PasswordHash] [varchar](128) NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SwaUserGroup]    Script Date: 11/22/2016 2:19:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SwaUserGroup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[GroupID] [int] NOT NULL,
	[IsGroupOwner] [bit] NOT NULL,
 CONSTRAINT [PK_UserGroup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[SwaGroup] ADD  CONSTRAINT [DF_SwaGroup_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[SwaUser] ADD  CONSTRAINT [DF_SwaUser_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[SwaUserGroup]  WITH CHECK ADD  CONSTRAINT [FK_SwaUserGroup_SwaGroup] FOREIGN KEY([GroupID])
REFERENCES [dbo].[SwaGroup] ([ID])
GO
ALTER TABLE [dbo].[SwaUserGroup] CHECK CONSTRAINT [FK_SwaUserGroup_SwaGroup]
GO
ALTER TABLE [dbo].[SwaUserGroup]  WITH CHECK ADD  CONSTRAINT [FK_SwaUserGroup_SwaUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[SwaUser] ([ID])
GO
ALTER TABLE [dbo].[SwaUserGroup] CHECK CONSTRAINT [FK_SwaUserGroup_SwaUser]
GO

INSERT [dbo].[SwaGroup] ([GroupName]) VALUES (N'swa')
INSERT [dbo].[SwaUser] ([Email], [PasswordHash]) VALUES (N'swa', N'GIM2I1LihP/Im/zVlMLZIJcN7EgWdFbWH3jN0HXIF3u2NHxd1FiJef4b01PiGwxH')--nobita
INSERT [dbo].[SwaUserGroup] ([UserID], [GroupID], [IsGroupOwner]) VALUES (1, 1, 1)
