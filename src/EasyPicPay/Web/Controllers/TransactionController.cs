using EasyPicPay.Application.Interfaces;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Web.ViewModels.Requests;
using EasyPicPay.Web.ViewModels.Responses;
using EasyPicPay.Application.Util;
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
    [HttpPost]
    [ProducesResponseType(typeof(CreateTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTransactionResponse>> Create(
        [FromBody] CreateTransactionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            logger.LogInformation(
                "Criando transação para PayerId: {PayerId}, PayeeId: {PayeeId}, Amount: {Amount}", 
                request.PayerId, request.PayeeId, request.Amount);

            var created = await service.CreateTransactionAsync(
                request.PayerId,
                request.PayeeId,
                request.Amount);

            var response = new CreateTransactionResponse(
                created.Id,
                ConstMessages.TransactionCreated // ✅ Usando constante
            );

            logger.LogInformation("Transação criada com ID: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Erro de negócio ao criar transação");
            return UnprocessableEntity(new { error = ex.Message }); // ✅ Service já usa constantes
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar transação");
            return StatusCode(500, new { error = ConstMessages.InternalError }); // ✅ Usando constante
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
    {
        logger.LogInformation("Buscando transação com ID: {Id}", id);

        var entity = await service.GetTransactionByIdAsync(id);

        if (entity == null)
        {
            logger.LogWarning("Transação {Id} não encontrada", id);
            return NotFound(new { error = ConstMessages.TransactionNotFound }); // ✅ Usando constante
        }

        var response = new TransactionResponse(
            entity.Id,
            entity.PayerId,
            entity.PayeeId,
            entity.Amount,
            entity.CreatedAt);

        return Ok(response);
    }
}