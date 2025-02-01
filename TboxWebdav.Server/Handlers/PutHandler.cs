using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using TboxWebdav.Server.Modules.Webdav;
using TboxWebdav.Server.Modules.Webdav.Internal;
using TboxWebdav.Server.Modules.Webdav.Internal.Helpers;
using TboxWebdav.Server.Modules.Webdav.Internal.Stores;

namespace TboxWebdav.Server.Handlers
{
    /// <summary>
    /// Implementation of the PUT method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV PUT method can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#METHOD_PUT">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class PutHandler : IWebDavHandler
    {
        private readonly ILogger<PutHandler> _logger;

        public PutHandler(ILogger<PutHandler> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Handle a PUT request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous PUT operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<WebDavResult> HandleRequestAsync(HttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            //Todo
            //if (!Config.AccessMode.CheckAccess(JboxAccessMode.upload))
            //{
            //    response.SetStatus(DavStatusCode.Forbidden);
            //    return true;
            //}S

            // It's not a collection, so we'll try again by fetching the item in the parent collection
            var splitUri = RequestHelper.SplitUri(new Uri(request.GetDisplayUrl()));

            // Obtain collection
            var collection = await store.GetCollectionAsync(splitUri.CollectionUri, httpContext).ConfigureAwait(false);
            if (collection == null)
            {
                // Source not found
                return new WebDavResult(DavStatusCode.Conflict);
            }

            // Upload the information to the item
            var status = await collection.UploadFromStreamAsync(httpContext, splitUri.Name, request.Body, request.ContentLength.Value).ConfigureAwait(false);

            // Finished writing
            return new WebDavResult(status);
        }
    }
}
