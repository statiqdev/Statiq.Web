using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentCollections;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class Redirects : Pipeline
    {
        public Redirects()
        {
            Dependencies.AddRange(nameof(Content), nameof(Assets), nameof(Archives), nameof(Data), nameof(Feeds));

            PostProcessHasDependencies = true;

            // Needs to be post-process to ensure all the document transformations to final destination paths have happened
            PostProcessModules = new ModuleList
            {
                new ReplaceDocuments(nameof(Content)),
                new ExecuteConfig(Config.FromSettings(settings =>
                {
                    string redirectPrefix = settings.GetString(WebKeys.NetlifyRedirectPrefix, "^");
                    GenerateRedirects generateRedirects = new GenerateRedirects()
                        .WithMetaRefreshPages(settings.GetBool(WebKeys.MetaRefreshRedirects, true));
                    if (settings.GetBool(WebKeys.NetlifyRedirects, false))
                    {
                        generateRedirects = generateRedirects
                            .AlwaysCreateAdditionalOutput()
                            .WithAdditionalOutput(
                                "_redirects",
                                async (redirects, ctx) =>
                                {
                                    StringBuilder redirectsBuilder = new StringBuilder();

                                    // Make sure we keep any existing manual redirect content
                                    IFile existingFile = ctx.FileSystem.GetInputFile("_redirects");
                                    if (existingFile.Exists)
                                    {
                                        redirectsBuilder.Append(await existingFile.ReadAllTextAsync());
                                    }

                                    // Generate content for any prefix redirects
                                    ConcurrentHashSet<NormalizedPath> prefixRedirects = new ConcurrentHashSet<NormalizedPath>();
                                    if (ctx.Settings.GetBool(WebKeys.NetlifyPrefixRedirects, true))
                                    {
                                        // Gather prefixed files and folders (do in parallel and remove duplicates since
                                        // path manipulation can be time consuming) - need to only get documents from
                                        // dependencies since those are the only pipelines that are guaranteed to have
                                        // executed their post-process phases due to PostProcessHasDependencies
                                        NormalizedPath[] destinationPaths = ctx.Outputs
                                            .FromPipelines(Dependencies.ToArray())
                                            .Select(x => x.Destination)
                                            .Where(x => !x.IsNullOrEmpty)
                                            .Distinct()
                                            .ToArray();
                                        Parallel.ForEach(
                                            destinationPaths,
                                            path =>
                                            {
                                                // First check if any parent folder has a prefix
                                                NormalizedPath parent = path.Parent;
                                                bool added = false;
                                                while (!parent.IsNullOrEmpty)
                                                {
                                                    if (parent.Name.StartsWith(redirectPrefix))
                                                    {
                                                        prefixRedirects.Add(parent);
                                                        added = true;
                                                        break;
                                                    }
                                                    parent = parent.Parent;
                                                }

                                                // Now check if this is a prefixed file
                                                if (!added && path.Name.StartsWith(redirectPrefix))
                                                {
                                                    prefixRedirects.Add(path);
                                                }
                                            });

                                        // Generate redirect content for each prefix redirect
                                        if (prefixRedirects.Count > 0)
                                        {
                                            if (redirectsBuilder.Length > 0)
                                            {
                                                redirectsBuilder.AppendLine();
                                                redirectsBuilder.AppendLine();
                                            }
                                            redirectsBuilder.AppendLine("# Prefix redirects generated by Statiq");
                                            foreach (NormalizedPath prefixRedirect in prefixRedirects)
                                            {
                                                redirectsBuilder.AppendLine("/"
                                                    + $"{ctx.GetLink(prefixRedirect.Parent)}/{ctx.GetLink(prefixRedirect.Name.Substring(redirectPrefix.Length)).TrimStart('/')} /{ctx.GetLink(prefixRedirect).TrimStart('/')}".TrimStart('/'));
                                            }
                                        }
                                    }

                                    // Produce the additional redirect content
                                    if (redirects.Count > 0)
                                    {
                                        if (redirectsBuilder.Length > 0)
                                        {
                                            if (prefixRedirects.Count == 0)
                                            {
                                                // Only include an extra line if we generated prefix redirects since
                                                // those will have already added a new line at the end due to AppendLine()
                                                redirectsBuilder.AppendLine();
                                            }
                                            redirectsBuilder.AppendLine();
                                        }
                                        redirectsBuilder.AppendLine("# Automatic redirects generated by Statiq");
                                        foreach (KeyValuePair<NormalizedPath, string> redirect in redirects)
                                        {
                                            redirectsBuilder.AppendLine($"/{redirect.Key} {redirect.Value}");
                                        }
                                    }

                                    return redirectsBuilder.ToString().Trim();
                                });
                    }
                    return generateRedirects;
                }))
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}