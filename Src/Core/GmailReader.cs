using System.Net;
using System.Text.RegularExpressions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>
/// Reads the Shoppix "Password Reset" email over Gmail IMAP and extracts the
/// temporary password it contains. Used by the forgot-password end-to-end flow.
/// </summary>
public static class GmailReader
{
    private static readonly Regex TemporaryPasswordPattern = new(
        @"new temporary password is[:\s]*([A-Za-z0-9]{6,20})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HtmlTagPattern = new("<[^>]+>", RegexOptions.Compiled);

    /// <summary>
    /// Polls the inbox until a password-reset email newer than <paramref name="requestedAtUtc"/>
    /// is found, then returns the temporary password. Returns null on timeout.
    /// </summary>
    public static string? FetchTemporaryPassword(
        string gmailAddress,
        string appPassword,
        DateTime requestedAtUtc,
        double timeoutSeconds = 90,
        double pollSeconds = 5)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (true)
        {
            var password = TryFetchOnce(gmailAddress, appPassword, requestedAtUtc);
            if (!string.IsNullOrEmpty(password))
            {
                return password;
            }

            if (DateTime.UtcNow >= deadline)
            {
                return null;
            }

            Thread.Sleep(TimeSpan.FromSeconds(pollSeconds));
        }
    }

    private static string? TryFetchOnce(string gmailAddress, string appPassword, DateTime requestedAtUtc)
    {
        using var client = new ImapClient();
        try
        {
            client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            client.Authenticate(gmailAddress, appPassword);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            // IMAP SINCE is date-granular; subtract a day so same-day resets are included.
            var query = SearchQuery.SubjectContains("Password Reset")
                .And(SearchQuery.DeliveredAfter(requestedAtUtc.Date.AddDays(-1)));

            var uids = inbox.Search(query);
            for (var i = uids.Count - 1; i >= 0; i--)
            {
                var message = inbox.GetMessage(uids[i]);
                if (message.Date.UtcDateTime < requestedAtUtc.AddMinutes(-2))
                {
                    continue;
                }

                var body = message.TextBody;
                if (string.IsNullOrWhiteSpace(body))
                {
                    body = StripHtml(message.HtmlBody ?? "");
                }

                var match = TemporaryPasswordPattern.Match(body ?? "");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            try
            {
                if (client.IsConnected)
                {
                    client.Disconnect(true);
                }
            }
            catch
            {
                // best-effort teardown
            }
        }
    }

    private static string StripHtml(string html)
    {
        var withoutTags = HtmlTagPattern.Replace(html, " ");
        return WebUtility.HtmlDecode(withoutTags);
    }
}
