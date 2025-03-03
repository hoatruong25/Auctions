using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(s => s.Make)),
            "new" => query.Sort(x => x.Descending(s => s.CreateAt)),
            _ => query.Sort(x => x.Ascending(s => s.AuctionEnd)),
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.Where(s => s.AuctionEnd < DateTime.UtcNow)),
            "endingSoon" => query.Match(
                x => x.Where(
                    s => s.AuctionEnd < DateTime.UtcNow.AddHours(6)
                         && s.AuctionEnd > DateTime.UtcNow
                )
            ),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(
            new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            }
        );
    }
}