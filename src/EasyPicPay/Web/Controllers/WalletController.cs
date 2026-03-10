using EasyPicPay.Web.ViewModels.Requests;
using EasyPicPay.Web.ViewModels.Responses;
using EasyPicPay.Application.Exceptions;
using EasyPicPay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization; // IWalletService
using Microsoft.AspNetCore.Mvc;

namespace EasyPicPay.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletService walletService,
        ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _logger        = logger;
    }

    /// <summary>
    /// Cria uma nova wallet.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateWalletResponse>> Create([FromBody] CreateWalletRequest request)
    {
        try
        {
            var created = await _walletService.CreateWalletAsync(
                request.Name,
                request.CpfCnpj,
                request.Email,
                request.Password,
                request.UserType);

            var response = new CreateWalletResponse("Wallet criada com sucesso.");

            // 201 Created + Location header que aponta para o GET da wallet
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (BusinessException ex)
        {
            // Erros de validação ou de regra de negócio vêm aqui
            _logger.LogWarning(ex, "Validação falhou ao criar wallet");
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar wallet");
            return StatusCode(500, "Ocorreu um erro interno.");
        }
    }

    /// <summary>
    /// Busca uma wallet pelo seu Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetWalletByIdResponse>> GetById(Guid id)
    {
        var wallet = await _walletService.GetWalletByIdAsync(id);
        if (wallet == null) return NotFound();

        var response = new GetWalletByIdResponse(
            wallet.Name,
            wallet.Email,
            wallet.Balance,
            wallet.UserType);

        return Ok(response);
    }

    /// <summary>
    /// Atualiza (debit ou credit) o saldo de uma wallet.
    /// </summary>
    [HttpPatch("balance")]
    public async Task<ActionResult<UpdateWalletBalanceResponse>> UpdateBalance(
        [FromBody] UpdateWalletBalanceRequest request)
    {
        try
        {
            // Primeiro verifica se a wallet existe
            if (!await _walletService.WalletExistsAsync(request.WalletId))
                return NotFound(new { Message = "Wallet não encontrada." });

            var currentBalance = await _walletService.GetBalanceAsync(request.WalletId);
            decimal newBalance = currentBalance + request.Amount;

            var resp = new UpdateWalletBalanceResponse (newBalance);
            return Ok(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar saldo da wallet");
            return StatusCode(500, "Erro interno.");
        }
    }
}