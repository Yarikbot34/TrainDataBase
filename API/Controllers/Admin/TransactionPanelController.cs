using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/adminPanel")]
public class TransactionPanelController : ControllerBase
{
    private readonly ITransactionsService _transactionsService;

    public TransactionPanelController(ITransactionsService transactionsService)
    {
        _transactionsService = transactionsService;
    }
    
    [HttpPost("transactions")]
    public async Task<IActionResult> GetTransactionsAsync(TransactionFilterDto filter)
    {
        var answ = await _transactionsService.GetTransactionsListAsync(filter);
        foreach (var item in answ)Console.WriteLine(item.TransactionType);
        return Ok(answ);
    }

    [HttpPatch("transactions/{transactionId}")]
    public async Task<IActionResult> PatchTransactionDesc(TransactionDto dto)
    {
        await _transactionsService.PatchTransactionDescFromDtoAsync(dto);
        return Ok();
    }

    [HttpDelete("transactions/{transactionId}")]
    public async Task<IActionResult> DeleteTransactionAsync(TransactionDeleteDto dto)
    {
        await _transactionsService.RemoveUnitsByTransactionAsync(dto, User);
        return Ok();
    }
}