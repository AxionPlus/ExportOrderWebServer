namespace ExportOrderEntites.ReleaseRecord
{

    public class ReleaseImportContainerRecordEntity : EntityBase
    {
        public required string UserName { get; set; }
        /// <summary>
        /// генерируется в базе  //
        /// </summary>
        public string DocNumber { get; set; }
        // public string? RefToUpdateDoc { get; set; }
        public required string ReleaseUID { get; set; }
        public required string BillOfLadingNum { get; set; }
        public required string ContainerNum { get; set; }
        public required string ContainerType { get; set; }
        public DateTime ReleaseTo { get; set; }
        public ReleaseMode ReleaseMode { get; set; }// = ReleaseMode.Create;
        public ReleaseStatus ReleaseStatus { get; set; }
        public string? TerminalName { get; set; }
        public string? LineName { get; set; }
        //--------------------------------------------------------------
        //public string? TerminalName { get; set; } 
        //public string? AgreementContractor { get; set; } 
        //public string? AgreementNo { get; set; } 

        //--------------------------------------------------------------

        //public IEnumerable<ReleaseImportCustomerSendingRecord>? SendingCustomerRecords { get; set; }
        public string? TerminalErrorResponse { get; set; }

        //[JsonIgnore]
        //public SpecialReleaseEntity? SpecialRelease { get; set; }

        //[NotMapped]
        //public bool IsSpecial => SpecialRelease is not null;           // Special Order for Release (manual issue by User)
    }
    public enum ReleaseStatus
    {
        NeedSendTerminal = 0,
        SentTerminal = 1,
        ConfirmedTerminal = 10,
        RefusedTerminal = 2,
        WaitRefuseConfirmation = 3, // при продление релиза
        RequestApproval = 4,
        ApprovalReject = 40,
        Released = 5,
        XCancelled = 9
    }
    public enum ReleaseMode
    {
        Create, Update, Reject
    }

    public class ReleaseRemark : EntityBase
    {
        public required string BillOfLadingNum { get; set; }
        public required string ContainerNum { get; set; }
        public required string DocNumber { get; set; }
        public required string Remark { get; set; }

    }
}
