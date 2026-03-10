using EasyPicPay.Web.ViewModels.Requests;
using EasyPicPay.Web.ViewModels.Responses;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Application.Interfaces;
using EasyPicPay.Application.Util;
using Microsoft.AspNetCore.Mvc;

namespace EasyPicPay.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController(
    IWalletService walletService,
    ILogger<WalletController> logger) : ControllerBase
{
    /// <summary>
    /// Cria uma nova wallet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateWalletResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateWalletResponse>> Create(
        [FromBody] CreateWalletRequest request)
    {
        // ✅ Valida ModelState (DataAnnotations)
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            logger.LogInformation("Criando wallet para Email: {Email}", request.Email);

            var created = await walletService.CreateWalletAsync(
                request.Name,
                request.CpfCnpj,
                request.Email,
                request.Password,
                request.UserType);

            var response = new CreateWalletResponse(ConstMessages.WalletCreated);

            logger.LogInformation("Wallet criada: {WalletId}", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Erro de negócio ao criar wallet");
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar wallet");
            return StatusCode(500, new { error = ConstMessages.InternalError });
        }
    }

    /// <summary>
    /// Busca uma wallet pelo seu Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetWalletByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetWalletByIdResponse>> GetById(Guid id)
    {
        logger.LogInformation("Buscando wallet: {WalletId}", id);

        var wallet = await walletService.GetWalletByIdAsync(id);

        if (wallet == null)
        {
            logger.LogWarning("Wallet {WalletId} não encontrada", id);
            return NotFound(new { error = ConstMessages.WalletNotFound });
        }

        var response = new GetWalletByIdResponse(
            wallet.Name,
            wallet.Email,
            wallet.Balance,
            wallet.UserType);

        return Ok(response);
    }

    /// <summary>
    /// Atualiza o saldo de uma wallet.
    /// </summary>
    [HttpPatch("balance")]
    [ProducesResponseType(typeof(UpdateWalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateWalletBalanceResponse>> UpdateBalance(
        [FromBody] UpdateWalletBalanceRequest request)
    {
        // ✅ Valida ModelState
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (!await walletService.WalletExistsAsync(request.WalletId))
            {
                logger.LogWarning("Wallet {WalletId} não encontrada para atualização", request.WalletId);
                return NotFound(new { error = ConstMessages.WalletNotFound });
            }

            var currentBalance = await walletService.GetBalanceAsync(request.WalletId);
            decimal newBalance = currentBalance + request.Amount;

            var response = new UpdateWalletBalanceResponse(newBalance);

            logger.LogInformation(
                "Saldo atualizado: WalletId={WalletId}, Anterior={Old}, Novo={New}", 
                request.WalletId, currentBalance, newBalance);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar saldo da wallet {WalletId}", request.WalletId);
            return StatusCode(500, new { error = ConstMessages.InternalError });
        }
    }
}