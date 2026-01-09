using JumpStart.Data.Advanced.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Auditing;

internal interface ISimpleAuditable : IAuditable<Guid>,    
    ISimpleCreatable, ISimpleModifiable, ISimpleDeletable
{
}
