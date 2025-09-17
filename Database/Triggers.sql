/* ============================================
   1) (Recommended) Keep StatusDescription in sync
   - If archiving SP always stamps StatusDescription, you can skip this.
   - Keeping it ensures correctness if someone inserts directly.
   ============================================ */
CREATE OR ALTER TRIGGER dbo.trg_ArchiveTable_StatusDescription
ON dbo.ArchiveTable
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  UPDATE a
     SET a.StatusDescription = ms.StatusDescription
  FROM dbo.ArchiveTable a
  JOIN inserted i              ON a.ID = i.ID
  JOIN dbo.MessageStatus ms    ON ms.StatusID = a.StatusId
  WHERE a.StatusDescription IS NULL
     OR a.StatusDescription <> ms.StatusDescription;
END
GO
