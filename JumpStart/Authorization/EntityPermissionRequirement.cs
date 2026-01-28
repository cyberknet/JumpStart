using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Authorization;

public record EntityPermissionRequirement() : IAuthorizationRequirement;
