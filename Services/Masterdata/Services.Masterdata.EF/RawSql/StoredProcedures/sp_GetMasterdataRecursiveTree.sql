--efcore.migration.down
--DROP PROCEDURE IF EXISTS [system].[sp_GetMasterdataRecursiveTree]

--efcore.migration.up
CREATE OR ALTER PROC [dbo].sp_GetMasterdataRecursiveTree 
	@rootId AS UNIQUEIDENTIFIER
AS BEGIN
	WITH cte(Id, [Key], Name, MasterdataTypeId, [level], [path]) AS
	(
		SELECT
			mdParent.Id
			, mdParent.[Key]
			, mdParent.Name
			, mdParent.MasterdataTypeId
			, 1
			, CAST(mdParent.[Key] AS NVARCHAR(MAX))
		FROM
			Masterdatas mdParent
		WHERE
			1=1
			AND mdParent.Id = @rootId
		UNION ALL
		SELECT
			mdChild.Id
			, mdChild.[Key]
			, mdChild.Name
			, mdChild.MasterdataTypeId
			, parentCte.[level] + 1
			, parentCte.[path] + ' -> ' + mdChild.[Key]
		FROM
			Masterdatas mdChild
		INNER JOIN MasterdataRelated r ON r.ChildMasterdataId = mdChild.Id
		INNER JOIN cte parentCte ON parentCte.Id = r.ParentMasterdataId AND parentCte.MasterdataTypeId = mdChild.MasterdataTypeId
	)
	SELECT * FROM cte
END