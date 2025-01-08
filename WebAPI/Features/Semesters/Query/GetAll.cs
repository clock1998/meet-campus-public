using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Semesters.Query
{
    public record GetAllSemesterResponse(Guid Id, int Year, string Name);
    public class GetAllSemesterHandler : Handler
    {
        public GetAllSemesterHandler(AppDbContext context) : base(context) { }

        public async Task<PagedList<GetAllSemesterResponse>> HandleAsync(QueryStringParameters queryStringParameters)
        {
            IQueryable<Semester> query = _context.Semesters;

            Expression<Func<Semester, object>>? keySelector = queryStringParameters.SortColum?.ToLower() switch
            {
                "name" => n => n.Name,
                _ => null
            };
            query = query.DefaultSort(keySelector, queryStringParameters.SortOrder);
            var responseQuery = query.Select(n => new GetAllSemesterResponse(n.Id, n.Year, n.Name));

            var data = await PagedList<GetAllSemesterResponse>.ToPagedListAsync(responseQuery, queryStringParameters.Page, queryStringParameters.PageSize);
            return data;
        }
    }

    public class GetAllSemesterController : SemesterController
    {
        private readonly GetAllSemesterHandler _handler;
        public GetAllSemesterController(AppDbContext context)
        {
            _handler = new GetAllSemesterHandler(context);
        }

        [HttpGet]
        [SwaggerOperation(Tags = new[] { "Semester" })]
        [EnableQuery]
        public async Task<IActionResult> Get([FromQuery] QueryStringParameters queryParameters)
        {
            return Ok(await _handler.HandleAsync(queryParameters));
        }
    }
}
