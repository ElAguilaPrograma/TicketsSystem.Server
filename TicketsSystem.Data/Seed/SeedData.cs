using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Data.Seed;

public static class SeedData
{
    private static readonly string[] DescriptionTemplates =
    [
        "I'm unable to log in to my account. It says 'Invalid credentials' even though I'm certain my password is correct. I've tried the password reset three times but never receive the reset email. Can you check if my account is locked?",
        "The two-factor authentication code is not being sent to my phone. I've verified my phone number in the settings. The 'Resend code' button also doesn't do anything. This blocks me from accessing the system entirely.",
        "I get a 'Session expired' error every 5 minutes while working. It makes it impossible to complete any task. I've tried different browsers but the problem persists across all of them.",
        "The reporting module is extremely slow when generating monthly summaries. A report that used to take 10 seconds now takes over 3 minutes. The database server shows high CPU usage during report generation.",
        "Page load times have increased significantly since the latest deployment. The home page takes 15-20 seconds to fully render. The network tab shows multiple API calls hanging without completing.",
        "Search functionality is timing out when looking for invoices from the last quarter. The search bar shows a spinner for about 45 seconds before displaying 'Request timed out'.",
        "The CSV import tool is failing when processing files larger than 5MB. The progress bar stops at 73% and then shows 'Import failed'. Smaller files import without issues.",
        "Data synchronization between our system and the main database stopped working after the last update. The sync log shows 'Connection refused' errors every time it tries to connect.",
        "Exported Excel reports have corrupted formatting. Cell colors, merged cells, and column widths are all wrong. The data is correct but the formatting makes it completely unreadable.",
        "The 'Save' button on the user profile page doesn't respond when clicked. No error message shows up in the console. The button just doesn't do anything after clicking.",
        "The navigation menu disappears when I resize the browser window below 1024px width. It works fine in full screen but I need to use the app on a tablet.",
        "Dark mode colors make some text unreadable. Specifically, the text in the sidebar and table headers is light gray on a white background. I had to switch back to light mode.",
        "The webhook endpoint is returning 502 errors when we try to push order data. This started yesterday afternoon. Our logs show 'Bad Gateway' responses from your server.",
        "The REST API is returning a 429 rate limit error even though we're well under the 1000 requests per hour limit. We counted only 342 requests in the last hour.",
        "Integration with Salesforce stopped working. The connection test fails with 'Authentication failed: token expired'. We regenerated the API key but it still doesn't work.",
        "System emails are going to recipients' spam folders instead of their inbox. We checked our domain SPF and DKIM records and they seem correct. This started about a week ago.",
        "Email notifications for ticket updates are not being sent to our support team. The notification settings are enabled in the admin panel and no errors are logged anywhere.",
        "The password reset email contains a broken link. The URL shows a double slash which returns a 404 page. Users are stuck unable to reset their passwords.",
        "Our invoice for last month shows the wrong total. It's charging us for 50 users but we only have 35 active accounts. The overcharge is significant. Please issue a corrected invoice.",
        "The billing dashboard shows 'Payment failed' but our credit card was charged successfully. The bank confirmed the transaction went through on their end.",
        "We need to update our payment method but the billing settings page shows 'Unable to retrieve payment information'. We can't add a new card or see the current one.",
        "Several users report they can't access the admin panel even though they have the Administrator role assigned. They get a 403 Forbidden error when navigating to the admin section.",
        "New users created through the API don't inherit the default role permissions. They can log in but can't see any modules. We have to manually reassign roles every time.",
        "The role editor page is missing several permission checkboxes that were there before. Specifically Export Data, Delete Users, and Manage Billing are no longer listed.",
        "We need the ability to filter tickets by multiple statuses at once. Currently we can only select one status filter, which makes it difficult to track Open and Reopened tickets together.",
        "It would be very helpful if the notification system supported email digests instead of individual emails for every update. We're getting overwhelmed by notification emails.",
        "The date picker component doesn't work in Safari on macOS. Clicking on the date field does nothing at all. Works fine in Chrome and Firefox.",
        "When I cancel editing a ticket comment the draft is lost. The system should auto-save drafts so users don't lose their work when navigating away by accident.",
        "The pagination on the search results page resets to page 1 when I click on any result and then go back using the browser back button. I have to scroll through everything again.",
        "After updating my profile picture the old picture still shows in the top navigation bar. The new picture appears correctly on the profile page but not in the header.",
        "I cannot delete a user account that has no active tickets or references. The system says 'Cannot delete user with existing references' even though all their data was archived.",
        "The mobile version of the dashboard is broken. Charts overlap with each other and the filter controls are off-screen on devices smaller than 768px width.",
        "The API endpoint for bulk user creation times out when processing more than 100 users at once. We need to import 500 users and splitting them into batches is tedious.",
        "WebSocket connections keep dropping every few minutes. The console shows 'WebSocket disconnected with code 1006'. This affects real-time notification delivery.",
        "The file preview feature doesn't support PNG images with transparent backgrounds. They display with a black background instead of transparent, which hides important diagram details.",
        "When we try to generate the annual audit report the system crashes with an out of memory exception. The server has 16GB RAM so this should not happen.",
        "The audit log does not capture DELETE operations on ticket attachments. We can see when files are uploaded but there is no record of when they are removed.",
        "Users are seeing duplicate notifications for the same event. Each ticket assignment triggers two identical emails and two in-app notifications.",
        "The time zone setting in user preferences is being ignored. All timestamps are shown in UTC even for users who have set their time zone to America/New_York.",
        "Search results include archived records that should be excluded by default. Users have to manually add a filter to hide archived items every single time they search.",
        "The drag and drop functionality for reordering dashboard widgets stopped working. Widgets snap back to their original position instead of staying where placed.",
        "OAuth login with Google accounts returns a 'redirect_uri_mismatch' error. The callback URL in the Google Cloud Console matches exactly what we have configured.",
        "The system allows creating tickets with blank titles. We had several tickets created by accident this week with no title at all, making them impossible to identify in lists.",
        "The REST API documentation shows the wrong data types for the create ticket endpoint. It says the priority field accepts integers but actually requires strings in the request body.",
        "Exporting a filtered view of tickets to PDF truncates long descriptions. Anything beyond 200 characters gets cut off with no indication that content was omitted.",
        "The SAML SSO integration fails with 'Invalid assertion' when users log in through Azure AD. The certificate thumbprint in our configuration matches what Azure provides.",
        "Loading the team management page with more than 50 team members causes the browser tab to become unresponsive. The page freezes for about 30 seconds before becoming usable.",
        "The webhook retry mechanism is not working as documented. Failed deliveries should retry 3 times with exponential backoff but they only attempt once then give up.",
        "The dashboard KPI widgets show different numbers than the detailed reports for the same time period. The summary widget shows 1,247 tickets but the report shows 1,289.",
        "Users can assign tickets to themselves even when they lack the permissions to handle that ticket category. This bypasses the queue management workflow entirely."
    ];

    public static async Task SeedAsync(
        SystemTicketsContext context,
        IPasswordHasher<User> passwordHasher,
        ILogger logger)
    {
        await SeedStatusesAsync(context, logger);
        await SeedPrioritiesAsync(context, logger);
        await SeedUsersAsync(context, passwordHasher, logger);
        await SeedTicketsAsync(context, logger);
    }

    private static async Task SeedStatusesAsync(SystemTicketsContext context, ILogger logger)
    {
        if (await context.TicketStatuses.AnyAsync())
            return;

        var statuses = new List<TicketStatus>
        {
            new() { StatusId = 1, Name = "Open" },
            new() { StatusId = 2, Name = "In Progress" },
            new() { StatusId = 3, Name = "On Hold" },
            new() { StatusId = 4, Name = "Closed" },
            new() { StatusId = 5, Name = "Reopened" }
        };

        context.TicketStatuses.AddRange(statuses);
        await context.SaveChangesAsync();
        logger.LogInformation("Default ticket statuses created successfully.");
    }

    private static async Task SeedPrioritiesAsync(SystemTicketsContext context, ILogger logger)
    {
        if (await context.TicketPriorities.AnyAsync())
            return;

        var priorities = new List<TicketPriority>
        {
            new() { PriorityId = 1, Name = "Low", Level = 1 },
            new() { PriorityId = 2, Name = "Medium", Level = 2 },
            new() { PriorityId = 3, Name = "High", Level = 3 },
            new() { PriorityId = 4, Name = "Critical", Level = 4 }
        };

        context.TicketPriorities.AddRange(priorities);
        await context.SaveChangesAsync();
        logger.LogInformation("Default ticket priorities created successfully.");
    }

    private static async Task SeedUsersAsync(
        SystemTicketsContext context,
        IPasswordHasher<User> passwordHasher,
        ILogger logger)
    {
        if (await context.Users.AnyAsync())
            return;

        var faker = new Faker("en");

        var users = new List<User>();
        for (int i = 0; i < 30; i++)
        {
            var user = new User
            {
                FullName = faker.Name.FullName(),
                IsActive = faker.Random.Bool(0.9f),
                CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-60), DateTime.UtcNow)
            };

            var firstName = user.FullName.Split(' ')[0].ToLower();
            var lastName = user.FullName.Split(' ').Last().ToLower();
            user.Email = faker.Internet.Email(firstName, lastName).ToLower();
            user.Role = i == 0 ? "Admin" : i <= 5 ? "Agent" : "User";
            user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");

            users.Add(user);
        }

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        logger.LogInformation("30 seed users created successfully.");
    }

    private static async Task SeedTicketsAsync(SystemTicketsContext context, ILogger logger)
    {
        if (await context.Tickets.AnyAsync())
            return;

        var users = await context.Users.ToListAsync();
        var agents = users.Where(u => u.Role is "Agent" or "Admin").ToList();
        var regularUsers = users.Where(u => u.Role == "User").ToList();
        var admin = users.First(u => u.Role == "Admin");

        var faker = new Faker("en");

        var titlePrefixes = new[]
        {
            "Cannot", "Unable to", "Error", "Bug:", "Issue with", "Problem:",
            "Feature Request:", "Incorrect", "Broken", "Missing", "Failed",
            "Slow", "Update:", "Question about", "Access issue"
        };

        var titleSubjects = new[]
        {
            "Login", "Dashboard", "Search", "Export", "Import",
            "Email Notifications", "Password Reset", "User Permissions",
            "Billing Invoice", "API Integration", "File Upload", "Data Sync",
            "Report Generation", "User Registration", "Role Management",
            "Two-Factor Auth", "Profile Picture", "CSV Import", "PDF Export",
            "Webhook Delivery", "OAuth Login", "SAML SSO", "Audit Log",
            "Mobile Layout", "Dark Mode", "Date Picker", "Search Filters",
            "Team Management", "Ticket Assignment", "Notification Settings"
        };

        var tickets = new List<Ticket>();
        for (int i = 0; i < 100; i++)
        {
            var statusRand = faker.Random.Float();

            var statusId = statusRand < 0.40f ? 1
                : statusRand < 0.65f ? 2
                : statusRand < 0.75f ? 3
                : statusRand < 0.95f ? 4
                : 5;

            var priorityRand = faker.Random.Float();
            var priorityId = priorityRand < 0.20f ? 1
                : priorityRand < 0.60f ? 2
                : priorityRand < 0.85f ? 3
                : 4;

            var creatorProb = faker.Random.Float();
            var createdByUserId = creatorProb < 0.75f
                ? faker.PickRandom(regularUsers).UserId
                : creatorProb < 0.95f
                    ? faker.PickRandom(agents).UserId
                    : admin.UserId;

            Guid? assignedToUserId = null;
            if (statusId != (int)TicketsStatusValue.Open || faker.Random.Float() >= 0.4f)
                assignedToUserId = faker.PickRandom(agents).UserId;

            var createdAt = faker.Date.Between(DateTime.UtcNow.AddDays(-45), DateTime.UtcNow);

            DateTime? updatedAt = statusId == (int)TicketsStatusValue.Open
                ? null
                : faker.Date.Between(createdAt, DateTime.UtcNow);

            DateTime? closedAt = statusId == (int)TicketsStatusValue.Closed
                ? faker.Date.Between(createdAt.AddHours(1), DateTime.UtcNow)
                : null;

            var title = $"{faker.PickRandom(titlePrefixes)} {faker.PickRandom(titleSubjects)}";

            var descriptionIndex = i % DescriptionTemplates.Length;
            var description = DescriptionTemplates[descriptionIndex];

            if (faker.Random.Float() < 0.3f)
            {
                var altIndex = faker.Random.Int(0, DescriptionTemplates.Length - 1);
                description = DescriptionTemplates[altIndex];
            }

            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                StatusId = statusId,
                PriorityId = priorityId,
                CreatedByUserId = createdByUserId,
                AssignedToUserId = assignedToUserId,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ClosedAt = closedAt
            };

            tickets.Add(ticket);
        }

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        var history = GenerateTicketHistory(tickets, agents, faker);
        if (history.Count > 0)
        {
            context.TicketHistories.AddRange(history);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("100 seed tickets with history created successfully.");
    }

    private static List<TicketHistory> GenerateTicketHistory(
        List<Ticket> tickets, List<User> agents, Faker faker)
    {
        var history = new List<TicketHistory>();
        var rng = new Random();

        foreach (var ticket in tickets.Where(t => t.StatusId != (int)TicketsStatusValue.Open))
        {
            var changeCount = rng.Next(1, 4);
            var baseTime = ticket.UpdatedAt ?? ticket.CreatedAt;

            for (int i = 0; i < changeCount && i < 3; i++)
            {
                var hoursDiff = Math.Max(1, (int)((DateTime.UtcNow - baseTime).TotalHours * 0.8));
                var changeTime = baseTime.AddHours(-rng.Next(1, hoursDiff + 1));

                switch (i)
                {
                    case 0:
                        history.Add(new TicketHistory
                        {
                            TicketId = ticket.TicketId,
                            ChangedByUserId = faker.PickRandom(agents).UserId,
                            ChangeGroupId = Guid.NewGuid(),
                            FieldName = "StatusId",
                            OldValue = "1",
                            NewValue = ticket.StatusId.ToString(),
                            ChangedAt = changeTime
                        });
                        break;

                    case 1 when ticket.AssignedToUserId.HasValue:
                        history.Add(new TicketHistory
                        {
                            TicketId = ticket.TicketId,
                            ChangedByUserId = faker.PickRandom(agents).UserId,
                            ChangeGroupId = Guid.NewGuid(),
                            FieldName = "AssignedToUserId",
                            OldValue = null,
                            NewValue = ticket.AssignedToUserId.Value.ToString(),
                            ChangedAt = changeTime
                        });
                        break;

                    case 2 when ticket.PriorityId > 1:
                        var oldPriority = rng.Next(1, ticket.PriorityId);
                        history.Add(new TicketHistory
                        {
                            TicketId = ticket.TicketId,
                            ChangedByUserId = faker.PickRandom(agents).UserId,
                            ChangeGroupId = Guid.NewGuid(),
                            FieldName = "PriorityId",
                            OldValue = oldPriority.ToString(),
                            NewValue = ticket.PriorityId.ToString(),
                            ChangedAt = changeTime
                        });
                        break;
                }
            }
        }

        return history;
    }
}
