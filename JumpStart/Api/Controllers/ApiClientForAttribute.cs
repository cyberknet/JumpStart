using JumpStart.Api.DTOs;
using JumpStart.Data;
using JumpStart.Forms.DTOs;
using JumpStart.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Api.Controllers;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
public class ApiClientForAttribute<TController, TEntity, TDto, TCreateDto, TUpdateDto, TRepository> : Attribute
    where TController : ApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository>
    where TEntity : Entity
    where TDto : EntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
    where TRepository : IRepository<TEntity>
{
}