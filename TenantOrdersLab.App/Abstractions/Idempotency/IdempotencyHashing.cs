using System.Security.Cryptography;
using System.Text;

public static class IdempotencyHashing
{
    // Stable hash for “same logical create request”
    public static byte[] HashCreateOrder(int customerId, decimal totalAmount, string currency)
    {
        var payload = $"{customerId}|{totalAmount:0.################}|{currency?.Trim().ToUpperInvariant()}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(payload));
    }
}