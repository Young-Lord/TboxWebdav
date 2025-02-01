using Microsoft.AspNetCore.Server.Kestrel.Core;
using TboxWebdav.Server.Handlers;
using TboxWebdav.Server.Modules;
using TboxWebdav.Server.Modules.Tbox;
using TboxWebdav.Server.Modules.Tbox.Services;
using TboxWebdav.Server.Modules.Webdav;
using TboxWebdav.Server.Modules.Webdav.Internal;
using TboxWebdav.Server.Modules.Webdav.Internal.Stores;

namespace TboxWebdav.Server.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient<IStoreCollection, TboxStoreCollection>();
            builder.Services.AddTransient<IStoreItem, TboxStoreItem>();
            builder.Services.AddTransient<TboxUploader>();
            builder.Services.AddScoped<IStore, TboxStore>();
            builder.Services.AddScoped<IWebDavContext, WebDavContext>();
            builder.Services.AddScoped<IWebDavDispatcher, WebDavDispatcher>();
            builder.Services.AddScoped<IWebDavStoreContext, WebDavStoreContext>();
            builder.Services.AddScoped<TboxUserTokenProvider>();
            builder.Services.AddScoped<TboxService>();
            builder.Services.AddScoped<TboxSpaceInfoProvider>();
            builder.Services.AddScoped<TboxSpaceCredProvider>();
            builder.Services.AddScoped<TboxParameterResolverProvider>();
            builder.Services.AddSingleton<HttpClientFactory>();

            builder.Services.AddKeyedScoped<IWebDavHandler, GetHandler>(WebDavRequestMethods.GET.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, PropFindHandler>(WebDavRequestMethods.PROPFIND.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, HeadHandler>(WebDavRequestMethods.HEAD.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, OptionsHandler>(WebDavRequestMethods.OPTIONS.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, DeleteHandler>(WebDavRequestMethods.DELETE.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, PutHandler>(WebDavRequestMethods.PUT.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, LockHandler>(WebDavRequestMethods.LOCK.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, UnlockHandler>(WebDavRequestMethods.UNLOCK.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, CopyHandler>(WebDavRequestMethods.COPY.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, MkcolHandler>(WebDavRequestMethods.MKCOL.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, MoveHandler>(WebDavRequestMethods.MOVE.ToString());
            builder.Services.AddKeyedScoped<IWebDavHandler, PropPatchHandler>(WebDavRequestMethods.PROPPATCH.ToString());

            builder.Services.AddMemoryCache(); // Singleton

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseMiddleware<BasicAuthMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}
