using Api.Services;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Api.Test
{
    public class MockDataService : IDataService
    {
        private readonly List<LinkBundle> _linkBundles;

        public MockDataService()
        {
            _linkBundles = new List<LinkBundle>
            {
                new LinkBundle
                {
                    Id = "1",
                    VanityUrl = "test1",
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Id = "1",
                            Url = "https://www.google.com",
                            Description = "Google"
                        },
                        new Link
                        {
                            Id = "2",
                            Url = "https://www.microsoft.com",
                            Description = "Microsoft"
                        }
                    }
                },
                new LinkBundle
                {
                    Id = "2",
                    VanityUrl = "test2",
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Id = "3",
                            Url = "https://www.apple.com",
                            Description = "Apple"
                        },
                        new Link
                        {
                            Id = "4",
                            Url = "https://www.amazon.com",
                            Description = "Amazon"
                        }
                    }
                }
            };
        }

        public async Task DeleteLinkBundle(LinkBundle linkBundle)
        {
            var bundle = _linkBundles.FirstOrDefault(lb => lb.Id == linkBundle.Id);
            if (bundle != null)
            {
                _linkBundles.Remove(bundle);
            }
            else
            { 
                throw new Exception("Link bundle not found");
            }
        }

        public async Task<LinkBundle> GetLinkBundle(string vanityUrl)
        {
            var bundle = _linkBundles.FirstOrDefault(lb => lb.VanityUrl == vanityUrl);
            if (bundle != null)
            {
                return bundle;
            }
            else
            {
                return null;
            }
        }

        public string GetLinkBundleId(string vanityUrl)
        {
            var bundle = _linkBundles.FirstOrDefault(lb => lb.VanityUrl == vanityUrl);
            if (bundle != null)
            {
                return bundle.Id;
            }
            else
            {
                return null;
            }
        }

        public async Task SaveLinkBundle(LinkBundle linkBundle)
        {
            var bundle = _linkBundles.FirstOrDefault(lb => lb.Id == linkBundle.Id);
            if (bundle != null)
            {
                var index = _linkBundles.IndexOf(bundle);
                _linkBundles[index] = linkBundle;
            }
            else
            {
                _linkBundles.Add(linkBundle);
            }
        }

        public async Task UpdateLinkBundle(LinkBundle linkBundle)
        {
            var bundle = _linkBundles.FirstOrDefault(lb => lb.Id == linkBundle.Id);
            if (bundle != null)
            {
                var index = _linkBundles.IndexOf(bundle);
                _linkBundles[index] = linkBundle;
            }
            else
            {
                throw new Exception("Link bundle not found");
            }
        }
    }
}