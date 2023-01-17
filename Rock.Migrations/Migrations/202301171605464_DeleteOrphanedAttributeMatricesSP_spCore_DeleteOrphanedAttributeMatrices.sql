/****** Object:  StoredProcedure [dbo].[spCore_DeleteOrphanedAttributeMatrices] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
		This procedure AttributeMatrix records that are no longer associated with an attribute value.
	</summary>

	<returns>
	</returns>
	<code>
	</code>
</doc>
*/
	
CREATE PROCEDURE [dbo].[spCore_DeleteOrphanedAttributeMatrices]
AS
BEGIN
	DECLARE @MatrixItemEntityTypeId INT = (SELECT Id FROM [EntityType] WHERE [Guid]='3C9D5021-0484-4846-AEF6-B6216D26C3C8')
	DECLARE @MatrixFieldTypeId INT = (SELECT Id FROM [FieldType] WHERE [Guid]='f16fc460-dc1e-4821-9012-5f21f974c677')
	DECLARE @MatrixEntityTypeId INT = (SELECT Id FROM [EntityType] WHERE [Guid]='028228F0-B1D9-4DE5-9E6A-F898C34DDAB8')
	IF @MatrixItemEntityTypeId IS NOT NULL AND @MatrixFieldTypeId IS NOT NULL AND @MatrixEntityTypeId IS NOT NULL
	BEGIN
		SELECT
			am.[Id], am.[Guid]
		INTO #temp
		FROM
			[AttributeMatrix] am 
			LEFT OUTER JOIN [AttributeValue] av ON av.[Value] = CONVERT(nvarchar(36), am.[Guid] ) AND av.[AttributeId] IN (
				SELECT a.[Id] FROM [Attribute] a WHERE a.[FieldTypeId] = @MatrixFieldTypeId )
		WHERE av.[Value] IS NULL

		DELETE FROM #temp 
		WHERE [Guid] IN (SELECT 
				am.[Guid] 
			FROM 
				#temp AS am
				LEFT JOIN [dbo].[ConnectionRequest] AS cr ON cr.AssignedGroupMemberAttributeValues LIKE ('%' + CONVERT(nvarchar(36), am.[Guid]) + '%')
			WHERE
				[AssignedGroupMemberAttributeValues] LIKE ('%' + CONVERT(nvarchar(36), am.[Guid]) + '%'))

		DELETE FROM av
		FROM 
			#temp as am 
			INNER JOIN [AttributeMatrixItem] ai ON am.Id = ai.AttributeMatrixId 
			INNER JOIN [AttributeValue] as av ON av.[EntityId] = ai.[Id]
			INNER JOIN [Attribute] as a ON a.EntityTypeId = @MatrixItemEntityTypeId AND a.Id = av.AttributeId
		WHERE a.EntityTypeId = @MatrixItemEntityTypeId

		DELETE FROM ai
		FROM 
			#temp as am 
			INNER JOIN [AttributeMatrixItem] ai ON am.Id = ai.AttributeMatrixId 

		DELETE FROM [AttributeMatrix] WHERE [Id] IN (SELECT Id FROM #temp)
	END
END