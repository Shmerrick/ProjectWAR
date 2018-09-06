using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace PatcherServer
{
    public class HttpServerBase
    {
        private Dictionary<string, MethodData> _methods = new Dictionary<string, MethodData>();
        private string _address;
        private int _port;
        private IWebHost _httpHost;

        public HttpServerBase()
        {
            AddHandlers(GetType(), this);
        }

        protected virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected async Task ResponseJson(HttpContext context, object obj)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
            context.Response.ContentLength = data.Length;
            await context.Response.Body.WriteAsync(data, 0, data.Length);
        }
        protected async Task ResponseError(HttpContext context)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("");
        }

        public async Task RunHttpServer(string address, int port, string name, CancellationTokenSource tokenSource)
        {
            _address = address;
            _port = port;

            await Initialize();

            var builder = new WebHostBuilder()
                .UseKestrel(options => options.Limits.MaxRequestBodySize = null)
                .UseUrls($"http://{_address}:{_port}")
                .ConfigureLogging(l => { })
                .ConfigureServices(services => services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue))
                .Configure(app =>
                {
                    Console.WriteLine($"Starting HTTP ({name}) Server http://{_address}:{_port}");
                    app.Run(async (context) =>
                    {
                        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = long.MaxValue;
                        try
                        {
                            string path = context.Request.Path.ToString().Replace("/", "").Replace("\\", "");
                            if (path.Length > 0 && _methods.ContainsKey(path))
                            {
                                var method = _methods[path];
                                var parameters = new List<object>();
                                if (method.HasContext)
                                    parameters.Add(context);

                                foreach (var p in method.Params.Values)
                                {
                                    if (p.Name == "context")
                                        continue;

                                    object data = null;
                                    if (p.Required && !(context.Request.Query.ContainsKey(p.Name) || (p.FromBody)))
                                    {
                                        Console.WriteLine($"Requested route {path} is missing required parameter {p.Name}");
                                        return;
                                    }

                                    if (p.Type == typeof(byte[]))
                                        data = System.Text.Encoding.ASCII.GetBytes(context.Request.Query[p.Name]);
                                    else
                                        data = Utils.FromString(context.Request.Query[p.Name], p.Type);

                                    parameters.Add(data);
                                }

                                _methods[path].Method.DynamicInvoke(parameters.ToArray());
                            }
                            else
                                Console.WriteLine($"Requested unknown route {path}");

                        }
                        catch (Exception e)
                        {
                            context.Response.StatusCode = 500;
                            Console.WriteLine("Error processing http request", e);
                            await context.Response.WriteAsync("Error");
                        }
                    });
                });


            _httpHost = builder.Build();
            await _httpHost.StartAsync(tokenSource.Token);
        }


        protected virtual void AddHandlers(Type type, object obj)
        {
            foreach (var method in type.GetMethods())
            {
                var attrib = method.GetCustomAttribute<HttpRoute>();
                if (attrib != null)
                {
                    var route = new MethodData()
                    {
                        Route = attrib,
                        Method = Utils.CreateDelegate(method, obj),
                        Params = new Dictionary<string, ParamData>()
                    };

                    _methods[attrib.Route] = route;

                    for (int i = 0; i < method.GetParameters().Length; i++)
                    {
                        if (method.GetParameters()[i].ParameterType == typeof(HttpContext))
                        {
                            route.Params["context"] = new ParamData()
                            {
                                Name = "context",
                                Required = true,
                                Type = typeof(HttpContext)
                            };
                            route.HasContext = true;
                            continue;
                        }

                        route.Params[method.GetParameters()[i].Name] = new ParamData()
                        {
                            Name = method.GetParameters()[i].Name,
                            Required = true,
                            Type = method.GetParameters()[i].ParameterType,
                        };
                    }
                }
            }
        }

        public class HttpRoute : Attribute
        {
            public string Route { get; set; }

            public HttpRoute(string route)
            {
                Route = route;
            }
        }

        public class MethodData
        {
            public bool HasContext;
            public Delegate Method;
            public HttpRoute Route;
            public Dictionary<string, ParamData> Params = new Dictionary<string, ParamData>();
        }

        public class ParamData
        {
            public string Name;
            public bool Required;
            public bool FromBody;
            public Type Type;
            public bool FromJson;
        }

    }
}
