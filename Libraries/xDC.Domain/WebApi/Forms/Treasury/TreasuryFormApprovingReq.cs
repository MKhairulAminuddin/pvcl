﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms.Treasury
{
    public class TreasuryFormApprovingReq
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string ApprovalNote { get; set; }
        public bool ApprovalStatus { get; set; }
    }
}
