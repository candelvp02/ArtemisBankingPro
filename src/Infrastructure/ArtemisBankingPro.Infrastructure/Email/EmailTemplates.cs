namespace ArtemisBankingPro.Infrastructure.Email;

public static class EmailTemplates
{
    public static string AccountActivation(string userName, string activationLink) => $"""
        <h2>Welcome to Artemis Banking Pro</h2>
        <p>Hello {userName},</p>
        <p>Please activate your account by clicking the link below:</p>
        <p><a href="{activationLink}">Activate Account</a></p>
        """;

    public static string PasswordReset(string userName, string resetLink) => $"""
        <h2>Password Reset Request</h2>
        <p>Hello {userName},</p>
        <p>Click the link below to reset your password. This link can only be used once:</p>
        <p><a href="{resetLink}">Reset Password</a></p>
        """;

    public static string CreditCardAssigned(string userName, string maskedCardNumber) => $"""
        <h2>New Credit Card Assigned</h2>
        <p>Hello {userName},</p>
        <p>A new credit card ending in {maskedCardNumber[^4..]} has been assigned to your account.</p>
        """;

    public static string TransactionNotification(string userName, string description, decimal amount) => $"""
        <h2>Transaction Notification</h2>
        <p>Hello {userName},</p>
        <p>{description}: <strong>{amount:C}</strong></p>
        """;
}