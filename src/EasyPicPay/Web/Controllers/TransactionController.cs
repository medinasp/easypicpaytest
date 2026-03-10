using EasyPicPay.Application.Exceptions;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Web.ViewModels.Requests;
using EasyPicPay.Web.ViewModels.Responses;
using EasyPicPay.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EasyPicPay.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class TransactionController(
    ITransactionService service,
    ILogger<TransactionController> logger)
    : ControllerBase
{
    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateTransactionResponse>> Create(
        [FromBody] CreateTransactionRequest request)
    {
        logger.LogInformation("Criando transação para PayerId: {PayerId}, PayeeId: {PayeeId}", request.PayerId, request.PayeeId);

            var created = await service.CreateTransactionAsync(
                request.PayerId,
                request.PayeeId,
                request.Amount);

            var response = new CreateTransactionResponse
            (
                created.Id,
                "Transação criada com sucesso."
            );

        logger.LogInformation("Transação criada com ID: {Id}", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Busca a transação pelo Id (uso interno ou para testes).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        logger.LogInformation("Buscando transação com ID: {Id}", id);
        
        var entity = await service.GetTransactionByIdAsync(id);

        if (entity == null) return NotFound();

        var dto = new TransactionDto(
            entity.Id,
            entity.PayerId,
            entity.PayeeId,
            entity.Amount,
            entity.CreatedAt);

        return Ok(dto);
    }
}