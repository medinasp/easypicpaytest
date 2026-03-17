namespace EasyPicPay.Application.Util;

public static class ConstMessages
{
    // Transaction
    public const string InsufficientBalance = "Saldo insuficiente.";
    public const string PayerNotFound = "Pagador não encontrado.";
    public const string PayeeNotFound = "Recebedor não encontrado.";
    public const string MerchantCannotSend = "Lojistas não podem enviar dinheiro.";
    public const string TransactionCreated = "Transação criada com sucesso.";
    public const string TransactionNotFound = "Transação não encontrada.";

    // Wallet
    public const string EmailOrDocumentAlreadyExists = "Email ou CPF/CNPJ já cadastrado.";
    public const string WalletNotFound = "Carteira não encontrada.";
    public const string WalletCreated = "Carteira criada com sucesso.";
    public const string InvalidName = "Nome inválido.";
    public const string InvalidEmail = "Email inválido.";    
    
    // Erros gerais (500)
    public const string InternalError = "Erro interno ao processar transação.";
}