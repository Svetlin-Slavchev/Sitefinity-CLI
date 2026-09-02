using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Progress.Sitefinity.MigrationTool.Core.Widgets;
using Progress.Sitefinity.RestSdk;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Mvc;
internal class EmbedTest : MigrationBase, IWidgetMigration
{
    private static readonly string[] propertiesToCopy = new string[] { "CssClass", "SuggestionFields" };
    private static readonly IDictionary<string, string> propertiesToRename = new Dictionary<string, string>()
    {
        { "IndexCatalogue", "SearchIndex" },
        { "BackgroundHint", "SearchBoxPlaceholder" },
        { "ScoringProfiles-ScoringProfile", "ScoringProfile" }
    };

    public async Task<MigratedWidget> Migrate(WidgetMigrationContext context)
    {
        var propsToRead = context.Source.Properties.ToDictionary(x => x.Key.Replace("Model-", string.Empty, StringComparison.InvariantCultureIgnoreCase), x => x.Value);
        //var migratedProperties = ProcessProperties(propsToRead, propertiesToCopy, propertiesToRename);
        var migratedProperties = new Dictionary<string, string>();

        if (propsToRead.TryGetValue("InlineCode", out string inlineCode))
        {
            if (!string.IsNullOrEmpty(inlineCode))
            {
                migratedProperties["Code"] = inlineCode;
            }
        }

        if (propsToRead.TryGetValue("Description", out string description))
        {
            if (!string.IsNullOrEmpty(description))
            {
                migratedProperties["Description"] = description;
            }
        }

        return new MigratedWidget("EmbedCode", migratedProperties);
    }
}
