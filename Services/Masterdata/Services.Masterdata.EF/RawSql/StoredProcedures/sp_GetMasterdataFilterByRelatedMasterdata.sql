--efcore.migration.down
--DROP PROCEDURE IF EXISTS [dbo].sp_GetMasterdataFilterByRelatedMasterdata

--efcore.migration.up
CREATE OR ALTER PROC [dbo].sp_GetMasterdataFilterByRelatedMasterdata
    @masterdataType AS nvarchar(50),
    @filterMasterdata AS nvarchar(max),
    @includeDescendants AS bit = 0
AS BEGIN

-- DECLARE @masterdataType AS nvarchar(50) = 'pts';
-- DECLARE @filterMasterdata AS nvarchar(max) = 'bus:AFRICA,countries:KEN';
-- DECLARE @filterMasterdata AS nvarchar(max) = 'bus:NIGERIA,bus:KENIA';
-- DECLARE @filterMasterdata AS nvarchar(max) = 'bus:NIGERIA';
-- DECLARE @includeDescendants AS bit = 1;

-- CREATE FILTER TABLE CTE
WITH cteFilter(masterdataType, masterdataTypeIsId, masterdata, masterdataIsId) AS 
(
SELECT
    PARSENAME(REPLACE([value], ':', '.'), 2) AS masterdataType
  , CASE WHEN TRY_CONVERT(UNIQUEIDENTIFIER, PARSENAME(REPLACE([value], ':', '.'), 2)) IS NULL THEN 0 ELSE 1 END AS masterdataTypeIsId
  , PARSENAME(REPLACE([value], ':', '.'), 1) AS masterdata
  , CASE WHEN TRY_CONVERT(UNIQUEIDENTIFIER, PARSENAME(REPLACE([value], ':', '.'), 1)) IS NULL THEN 0 ELSE 1 END AS masterdataIsId
FROM
    STRING_SPLIT (@filterMasterdata, ',')
),
-- CREATE TABLE WITH MASTERDATA THAT MAKES UP THE FILTER
cteResult AS
(
-- START WITH A LIST OF MDs THAT WE NEED TO FILTER ON
SELECT DISTINCT
    mdParent.Id
  , mdParent.MasterdataTypeId
  , mdParent.[Key]
  , mdtParent.Code
  , 1 as level
  , CAST(mdParent.[Key] AS NVARCHAR(MAX)) as [path]
FROM
    Masterdatas mdParent
    INNER JOIN MasterdataTypes mdtParent ON mdtParent.Id = mdParent.MasterdataTypeId
    , cteFilter AS fltr
WHERE
    1 = 1
    AND mdtParent.Code = CASE WHEN TRY_CONVERT(UNIQUEIDENTIFIER, fltr.MasterdataType) IS NULL THEN fltr.MasterdataType ELSE mdtParent.Code END
    AND mdParent.MasterdataTypeId = ISNULL(TRY_CONVERT(UNIQUEIDENTIFIER, fltr.MasterdataType), mdParent.MasterdataTypeId)
    AND mdParent.[Key] = CASE WHEN TRY_CONVERT(UNIQUEIDENTIFIER, fltr.Masterdata) IS NULL THEN fltr.Masterdata ELSE mdParent.[Key] END
    AND mdParent.Id = ISNULL(TRY_CONVERT(UNIQUEIDENTIFIER, fltr.Masterdata), mdParent.Id)

UNION ALL

-- INCLUDE IN THAT MDs LIST ALSO THE CHILD MDs IF REQUESTED
SELECT
    mdParentTree.Id
  , mdParentTree.MasterdataTypeId
  , mdParentTree.[Key]
  , mdtParentTree.Code
  , result.[level] + 1
  , result.[path] + ' -> ' + mdParentTree.[Key]
FROM
    MasterdataRelated mdrTree
    INNER JOIN Masterdatas mdParentTree ON mdParentTree.Id = mdrTree.ChildMasterdataId
    INNER JOIN MasterdataTypes mdtParentTree ON mdtParentTree.Id = mdParentTree.MasterdataTypeId
    INNER JOIN cteResult result ON result.Id = mdrTree.ParentMasterdataId AND result.MasterdataTypeId = mdParentTree.MasterdataTypeId
WHERE
    1 = @includeDescendants
)

-- RETURN MDs THAT HAVE A PARENT THAT EXISTS IN THE FILTER LIST
-- AND THAT ARE OF TYPE THAT WAS REQUESTED (@masterdataType)
SELECT
    DISTINCT
    md.Id
  , md.[Key]
  , md.Name
FROM
    cteResult
    INNER JOIN MasterdataRelated mdr ON mdr.ParentMasterdataId = cteResult.Id
    INNER JOIN Masterdatas md ON md.Id = mdr.ChildMasterdataId
    INNER JOIN MasterdataTypes mt ON mt.Id = md.MasterdataTypeId
WHERE
    1 = 1
    AND mt.Code = CASE WHEN TRY_CONVERT(UNIQUEIDENTIFIER, @masterdataType) IS NULL THEN @masterdataType ELSE mt.Code END
    AND md.MasterdataTypeId = ISNULL(TRY_CONVERT(UNIQUEIDENTIFIER, @masterdataType), md.MasterdataTypeId)

END