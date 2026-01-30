// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Correlate;
using JumpStart.Api.Controllers;
using JumpStart.Forms;
using JumpStart.Forms.DTOs;
using JumpStart.Forms.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JumpStart.Forms.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionTypesController : ApiControllerBase<
    QuestionType,
    QuestionTypeDto,
    CreateQuestionTypeDto,
    UpdateQuestionTypeDto,
    IQuestionTypeRepository>
{
    public QuestionTypesController(
        IQuestionTypeRepository repository,
        IMapper mapper,
        ILogger<QuestionTypesController> logger,
        ICorrelationContextAccessor correlationContext)
        : base(repository, mapper, logger, correlationContext)
    {
    }
}
