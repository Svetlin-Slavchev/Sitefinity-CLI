using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;
using Progress.Sitefinity.RestSdk.Clients.Pages.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class CustomContentWidget : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new[]
    {
        "BackgroundColor",
        "ContentContainer",
        "ContainerPadding",
        "ColumnLayout",
        "IndentContent",
        "ContentAnimation",
        "AnimationDelay",
        "Title",
        "TitleTag",
        "TitleSize",
        "SubTitle",
        "SubTitleColor",
        "Title3",
        "Ribbon",
        "ButtonStyling",
        "AdaLink",
        "Insights",
        "InsightsProperty",
        "AdditionalLinks",
        "ImagePosition",
    };

    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "Template", "SfViewName" },
    };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);

        var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);

        string linkText = propsToRead.TryGetValue("LinkText", out string lt) ? lt : null;
        bool newTab = propsToRead.TryGetValue("NewTab", out string newTabString) && bool.TryParse(newTabString, out bool newTabParsed) && newTabParsed;
        string target = newTab ? "_blank" : null;

        if (propsToRead.TryGetValue("PageId", out string pageIdString) && Guid.TryParse(pageIdString, out Guid pageId) && pageId != Guid.Empty)
        {
            var pageNode = await context.SourceClient.GetItem<PageNodeDto>(new GetItemArgs()
            {
                Type = RestClientContentTypes.Pages,
                Id = pageIdString,
            });

            migratedProperties.Add("Link", JsonSerializer.Serialize(new
            {
                id = pageIdString,
                type = "Pages",
                href = pageNode?.ViewUrl,
                text = string.IsNullOrWhiteSpace(linkText) ? pageNode?.Title : linkText,
                target
            }));
        }
        else if (propsToRead.TryGetValue("ExternalLink", out string externalLink) && !string.IsNullOrEmpty(externalLink))
        {
            migratedProperties.Add("Link", JsonSerializer.Serialize(new
            {
                href = externalLink,
                text = linkText,
                target
            }));
        }

        if (propsToRead.TryGetValue("CtaImageId", out string ctaImageIdString) && Guid.TryParse(ctaImageIdString, out Guid ctaImageId) && ctaImageId != Guid.Empty &&
            propsToRead.TryGetValue("BackgroundImageProviderName", out string providerName) && !string.IsNullOrEmpty(providerName))
        {
            var image = await GetSingleItemMixedContentValue(context, [ctaImageIdString], "Telerik.Sitefinity.Libraries.Model.Image", providerName, true);
            migratedProperties.Add("Image", image);
        }

        return new MigratedWidget("CustomContent", migratedProperties);
    }
}
