using System.CommandLine.Binding;
using System.CommandLine;
using System;
using System.Net;

namespace TboxWebdav.Server.AspNetCore.Models
{
    public class AppCmdOption
    {
        public static AppCmdOption Default { get; set; }

        public FileInfo? ConfigFile { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public AppAuthMode AuthMode { get; set; }
        public List<AppCmdCustomUser> Users { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
        public string? Cookie { get; set; }
        public string? UserToken { get; set; }
        public AppAccessMode AccessMode { get; set; }
    }

    public class AppCmdCustomUser
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Cookie { get; set; }
        public string? UserToken { get; set; }
        public AppAccessMode AccessMode { get; set; }
    }

    public class AppCmdOptionBinder : BinderBase<AppCmdOption>
    {
        private readonly Option<FileInfo?> _configFileOption;
        private readonly Option<int> _portOption;
        private readonly Option<string> _hostOption;
        private readonly Option<AppAuthMode> _authModeOption;
        private readonly Option<string?> _userNameOption;
        private readonly Option<string?> _passwordOption;
        private readonly Option<string?> _cookieOption;
        private readonly Option<string?> _userTokenOption;
        private readonly Option<AppAccessMode> _accessModeOption;

        public AppCmdOptionBinder(
            Option<FileInfo?> configFileOption,
            Option<int> portOption,
            Option<string> hostOption,
            Option<AppAuthMode> authModeOption,
            Option<string?> userNameOption,
            Option<string?> passwordOption,
            Option<string?> cookieOption,
            Option<string?> userTokenOption,
            Option<AppAccessMode> accessModeOption)
        {
            _configFileOption = configFileOption;
            _portOption = portOption;
            _hostOption = hostOption;
            _authModeOption = authModeOption;
            _userNameOption = userNameOption;
            _passwordOption = passwordOption;
            _cookieOption = cookieOption;
            _userTokenOption = userTokenOption;
            _accessModeOption = accessModeOption;
        }

        protected override AppCmdOption GetBoundValue(BindingContext bindingContext)
        {
            var opt = new AppCmdOption
            {
                //ConfigFile = bindingContext.ParseResult.GetValueForOption(_configFileOption),
                Port = bindingContext.ParseResult.GetValueForOption(_portOption),
                Host = bindingContext.ParseResult.GetValueForOption(_hostOption),
                AuthMode = bindingContext.ParseResult.GetValueForOption(_authModeOption),
                Users = new List<AppCmdCustomUser>(),
            };

            var userName = bindingContext.ParseResult.GetValueForOption(_userNameOption);
            var password = bindingContext.ParseResult.GetValueForOption(_passwordOption);
            var cookie = bindingContext.ParseResult.GetValueForOption(_cookieOption);
            var userToken = bindingContext.ParseResult.GetValueForOption(_userTokenOption);
            var accessMode = bindingContext.ParseResult.GetValueForOption(_accessModeOption);

            if (opt.AuthMode == AppAuthMode.None && (userToken == null && cookie == null))
            {
                opt.IsError = true;
                opt.Message = "使用 --auth None 时，必须指定 --cookie 或者 --token 用于新云盘认证。";
                return opt;
            }
            if (opt.AuthMode == AppAuthMode.Custom && userName == null)
            {
                opt.IsError = true;
                opt.Message = "使用 --auth Custom 时，必须指定 --username 用于 WebDav 服务认证。（密码可为空，但不推荐！）";
                return opt;
            }
            if (opt.AuthMode == AppAuthMode.Custom && (userToken == null && cookie == null))
            {
                opt.IsError = true;
                opt.Message = "使用 --auth Custom 时，必须指定 --cookie 或者 --token 用于新云盘认证。";
                return opt;
            }
            if (opt.AuthMode == AppAuthMode.JaCookie
                || opt.AuthMode == AppAuthMode.UserToken
                || opt.AuthMode == AppAuthMode.Mixed
                )
            {
                opt.AccessMode = accessMode;
            }
            if (opt.AuthMode == AppAuthMode.None)
            {
                opt.AccessMode = accessMode;
                opt.UserToken = userToken;
                opt.Cookie = cookie;
            }
            if (opt.AuthMode == AppAuthMode.Custom
                || (opt.AuthMode == AppAuthMode.Mixed && userName != null))
            {
                opt.Users.Add(new AppCmdCustomUser()
                {
                    AccessMode = accessMode,
                    Cookie = cookie,
                    Password = password,
                    UserName = userName,
                    UserToken = userToken,
                });
            }
            return opt;
        }
    }
}
