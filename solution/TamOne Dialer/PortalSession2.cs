using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Net;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TamOne_Dialer
{
    public class PortalSession2
    {
        private CookieContainer cookies;
        public Uri EndpointPrefix { get; set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public LoginStates LoginState { get; private set; }
        public string Version { get; set; }

        public delegate void LoginSucceededHandler(LoginSucceededEventArgs e);
        public delegate void LoginFailedHandler(LoginFailedEventArgs e);
        public delegate void LoggingInHandler(LoggingInEventArgs e);
        public delegate void LoginStateChangedHandler(LoginStateChangedEventArgs e);
        public delegate void DeviceListReceivedHandler(DeviceListReceivedEventArgs e);
        public delegate void DeviceListFailedHandler(DeviceListFailedEventArgs e);
        public delegate void PrefixListReceivedHandler(PrefixListReceivedEventArgs e);
        public delegate void PrefixListFailedHandler(PrefixListFailedEventArgs e);
        public delegate void CalledHandler(CalledEventArgs e);
        public delegate void CallFailedHandler(CallFailedEventArgs e);

        public event LoginSucceededHandler LoginSucceeded;
        public event LoginFailedHandler LoginFailed;
        public event LoggingInHandler LoggingIn;
        public event LoginStateChangedHandler LoginStateChanged;
        public event DeviceListReceivedHandler DeviceListReceived;
        public event DeviceListFailedHandler DeviceListFailed;
        public event PrefixListReceivedHandler PrefixListReceived;
        public event PrefixListFailedHandler PrefixListFailed;
        public event CalledHandler Called;
        public event CallFailedHandler CallFailed;

        public enum LoginStates
        {
            LOGGED_IN,
            LOGGED_OUT,
            LOGGING_IN
        }

        public enum LoginType
        {
            NORMAL,
            SILENT,
            INTERNAL
        }

        public PortalSession2()
        {
            this.cookies = new CookieContainer();
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LoginState = LoginStates.LOGGED_OUT;
        }

        public void AssociateNewCookieContainer()
        {
            this.cookies = new CookieContainer();
        }

        public void Login(string username, string password, LoginType loginType)
        {
            Login(username, password, loginType, null);
        }


        public void ReLogin(LoginType loginType)
        {
            ReLogin(loginType, null);
        }

        private void ReLogin(LoginType loginType, LoginSucceededHandler successHandler) {
            if (Username == null || Password == null)
            {
                throw new ArgumentException("ReLogin cannot be called when username and password have not been set.");
            }
            Login(Username, Password, loginType, successHandler);
        }

        private void Login(string username, string password, LoginType loginType, LoginSucceededHandler successHandler)
        {
            Username = username;
            Password = password;
            var f = (ThreadStart)delegate()
            {
                this.cookies = new CookieContainer();
                cookies.Add(EndpointPrefix, new Cookie("top_csrf", "yolo"));
                OnLoggingIn(new LoggingInEventArgs(loginType));
                try
                {
                    var request = configPostRequest("nl/auth/login");
                    byte[] postBody = Encoding.UTF8.GetBytes("top_csrf_tok=yolo&identity=" +
                                HttpUtility.UrlEncode(username, Encoding.UTF8) + "&password=" +
                                HttpUtility.UrlEncode(password, Encoding.UTF8));
                    request.ContentLength = postBody.Length;
                    var stream = request.GetRequestStream();
                    stream.Write(postBody, 0, postBody.Length);
                    stream.Close();
                    getAndParseResponse(request);
                    if (successHandler != null)
                    {
                        successHandler(new LoginSucceededEventArgs(loginType));
                    }
                    OnLoginSucceeded(new LoginSucceededEventArgs(loginType));
                }
                catch (PortalException e)
                {
                    var args = new LoginFailedEventArgs(loginType, e);
                    OnLoginFailed(args);
                }
                catch (WebException e)
                {
                    var args = new LoginFailedEventArgs(loginType, e);
                    OnLoginFailed(args);
                }
                catch (JsonReaderException e)
                {
                    var args = new LoginFailedEventArgs(loginType, e);
                    OnLoginFailed(args);
                }
                catch (Exception e)
                {
                    var args = new LoginFailedEventArgs(loginType, e);
                    OnLoginFailed(args);
                }
            };

            var t = new Thread(f);
            t.Start();
        }

        public void GetDeviceList()
        {
            var f = (ThreadStart)delegate()
            {
                try
                {
                    var request = configGetRequest("nl/ajax/user/device_list");
                    dynamic result = getAndParseResponse(request);
                    var devices = new List<Device>();
                    foreach (dynamic device in result.data)
                    {
                        devices.Add(new Device((string)device.id, (string)device.name));
                        Console.WriteLine(device.id + ": " + device.name);
                    }
                    OnDeviceListReceived(new DeviceListReceivedEventArgs(devices));
                }
                catch (PortalException e)
                {
                    var args = new DeviceListFailedEventArgs(e);
                    OnDeviceListFailed(args);
                }
                catch (WebException e)
                {
                    var args = new DeviceListFailedEventArgs(e);
                    OnDeviceListFailed(args);
                }
                catch (JsonReaderException e)
                {
                    var args = new DeviceListFailedEventArgs(e);
                    OnDeviceListFailed(args);
                }
                catch (Exception e)
                {
                    var args = new DeviceListFailedEventArgs(e);
                    OnDeviceListFailed(args);
                }

            };
            var t = new Thread(f);
            t.Start();
        }

        public void GetPrefixList()
        {
            var f = (ThreadStart)delegate()
            {
                try
                {
                    var request = configGetRequest("nl/ajax/user/prefix_list");
                    dynamic result = getAndParseResponse(request);
                    var prefixes = new List<Prefix>();
                    foreach (dynamic prefix in result.data)
                    {
                        prefixes.Add(new Prefix((string)prefix.id, (string)prefix.name));
                        Console.WriteLine(prefix.id + ": " + prefix.name);
                    }
                    OnPrefixListReceived(new PrefixListReceivedEventArgs(prefixes));
                }
                catch (PortalException e)
                {
                    var args = new PrefixListFailedEventArgs(e);
                    OnPrefixListFailed(args);
                }
                catch (WebException e)
                {
                    var args = new PrefixListFailedEventArgs(e);
                    OnPrefixListFailed(args);
                }
                catch (JsonReaderException e)
                {
                    var args = new PrefixListFailedEventArgs(e);
                    OnPrefixListFailed(args);
                }
                catch (Exception e)
                {
                    var args = new PrefixListFailedEventArgs(e);
                    OnPrefixListFailed(args);
                }

            };
            var t = new Thread(f);
            t.Start();
        }

        public void Call(Prefix prefix, string isInternal, string to, Device from)
        {
            Call(prefix, isInternal, to, from, 0);
        }

        private void Call(Prefix prefix, string isInternal, string to, Device from, int attempts)
        {
            var f = (ThreadStart)delegate()
            {
                try
                {
                    // drie keer encoden om CodeIgniter te slim af te zijn
                    string url = "nl/ajax/user/init_call/" + prefix.Id + "/" + isInternal + "/" + HttpUtility.UrlEncode(HttpUtility.UrlEncode(HttpUtility.UrlEncode(to, Encoding.UTF8), Encoding.UTF8), Encoding.UTF8) + "/" + from.Id;
                    Console.WriteLine(url);
                    var request = configPostRequest(url);
                    dynamic result = getAndParseResponse(request);
                    OnCalled(new CalledEventArgs((string) result.data.given, (string) result.data.dialed));
                }
                catch (PortalAuthorizationException)
                {
                    if (attempts == 0)
                    {
                        LoginSucceededHandler h = delegate(LoginSucceededEventArgs args)
                        {
                            Call(prefix, isInternal, to, from, attempts + 1);
                        };
                        ReLogin(LoginType.INTERNAL, h);
                    }
                    else
                    {
                        Console.WriteLine("Given up on calling after " + attempts + " attempts.");
                    }
                }
                catch (PortalException e)
                {
                    Console.WriteLine("Call request failed (PortalException).");
                    OnCallFailed(new CallFailedEventArgs(e));
                }
                catch (WebException e)
                {
                    Console.WriteLine("Call request failed (WebException).");
                    OnCallFailed(new CallFailedEventArgs(e));
                }
                catch (JsonReaderException e)
                {
                    Console.WriteLine("Call request failed (JsonReaderException).");
                    OnCallFailed(new CallFailedEventArgs(e));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Call request failed (JsonReaderException).");
                    OnCallFailed(new CallFailedEventArgs(e));
                }

            };
            var t = new Thread(f);
            t.Start();
        }

        private HttpWebRequest configRequest(string path)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(EndpointPrefix.OriginalString + path);
            request.CookieContainer = this.cookies;
            request.AllowAutoRedirect = false;
            request.Headers.Add("X-Requested-With: XMLHttpRequest");
            request.UserAgent = "TamOneDialer/" + Version;
            request.Headers.Add("X-API-Version: 1.0");
            request.Timeout = 15000;
            foreach (Cookie cookieValue in this.cookies.GetCookies(EndpointPrefix))
            {
                Console.WriteLine("Request cookie: " + cookieValue.ToString());
            }
            return request;
        }

        private HttpWebRequest configGetRequest(string path)
        {
            var request = configRequest(path);
            request.Method = "GET";
            return request;
        }

        private HttpWebRequest configPostRequest(string path)
        {
            var request = configRequest(path);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        private dynamic getAndParseResponse(HttpWebRequest request)
        {
            HttpWebResponse r = (HttpWebResponse)request.GetResponse();
            Console.WriteLine((int)(r.StatusCode) + " " + r.StatusDescription);
            this.cookies.Add(r.Cookies);
            var s = new StreamReader(r.GetResponseStream());
            string body = s.ReadToEnd();

            Console.WriteLine("Body: " + body);

            dynamic result = JObject.Parse(body);
            if (!((bool)result.status))
            {
                int errorcode = -1;
                try
                {
                    errorcode = (int) result.errorcode;
                }
                catch { }
                if (errorcode == 401)
                {
                    throw new PortalAuthorizationException("Niet ingelogd.", result, errorcode);
                }
                string message = "General failure";
                try
                {
                    message = (string)result.message;
                }
                catch { }
                throw new PortalException(message, result);
            }
            return result;
        }

        public class PortalException : Exception
        {
            public dynamic Result { get; set; }

            public PortalException(string message) : base(message)
            {

            }

            public PortalException(string message, dynamic result) : base(message)
            {
                this.Result = result;
            }
        }

        public class PortalAuthorizationException : PortalException
        {
            public int ErrorCode { get; set; }

            public PortalAuthorizationException(string message, dynamic result, int errorCode) : base(message)
            {
                this.Result = result;
                this.ErrorCode = errorCode;
            }
        }

        public class LoginSucceededEventArgs : EventArgs
        {
            public LoginType LoginType { get; private set; }

            public LoginSucceededEventArgs(LoginType loginType)
            {
                LoginType = loginType;
            }
        }

        public class LoginFailedEventArgs : EventArgs
        {
            public Exception Exception { get; private set; }
            public LoginType LoginType { get; private set; }

            public LoginFailedEventArgs(LoginType loginType)
            {
                LoginType = loginType;
            }

            public LoginFailedEventArgs(LoginType loginType, Exception e) : this(loginType)
            {
                this.Exception = e;
            }
        }

        public class LoggingInEventArgs : EventArgs
        {
            public LoginType LoginType { get; private set; }

            public LoggingInEventArgs(LoginType loginType)
            {
                LoginType = loginType;
            }
        }

        public class LoginStateChangedEventArgs : EventArgs
        {

        }

        public class DeviceListReceivedEventArgs : EventArgs
        {
            public ICollection<Device> Devices { get; private set; }
            public DeviceListReceivedEventArgs(ICollection<Device> devices)
            {
                this.Devices = devices;
            }
        }

        public class DeviceListFailedEventArgs : EventArgs
        {
            public Exception Exception { get; private set; }

            public DeviceListFailedEventArgs()
            {

            }

            public DeviceListFailedEventArgs(Exception e)
            {
                this.Exception = e;
            }
        }

        public class PrefixListFailedEventArgs : EventArgs
        {
            public Exception Exception { get; private set; }

            public PrefixListFailedEventArgs()
            {

            }

            public PrefixListFailedEventArgs(Exception e)
            {
                this.Exception = e;
            }
        }

        public class PrefixListReceivedEventArgs : EventArgs
        {
            public ICollection<Prefix> Prefixes { get; private set; }
            public PrefixListReceivedEventArgs(ICollection<Prefix> prefixes)
            {
                this.Prefixes = prefixes;
            }
        }

        public class CalledEventArgs : EventArgs
        {
            public string Given { get; private set; }
            public string Dialed { get; private set; }

            public CalledEventArgs(string given, string dialed)
            {
                this.Given = given;
                this.Dialed = dialed;
            }
        }

        public class CallFailedEventArgs : EventArgs
        {
            public Exception Exception { get; private set; }

            public CallFailedEventArgs()
            {

            }

            public CallFailedEventArgs(Exception e)
            {
                this.Exception = e;
            }
        }

        protected virtual void OnLoginSucceeded(LoginSucceededEventArgs e)
        {
            this.LoginState = LoginStates.LOGGED_IN;
            OnLoginStateChanged(new LoginStateChangedEventArgs());
            var handler = LoginSucceeded;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnLoginFailed(LoginFailedEventArgs e)
        {
            this.LoginState = LoginStates.LOGGED_OUT;
            OnLoginStateChanged(new LoginStateChangedEventArgs());
            var handler = LoginFailed;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnLoggingIn(LoggingInEventArgs e)
        {
            this.LoginState = LoginStates.LOGGING_IN;
            OnLoginStateChanged(new LoginStateChangedEventArgs());
            var handler = LoggingIn;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnLoginStateChanged(LoginStateChangedEventArgs e)
        {
            var handler = LoginStateChanged;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnDeviceListReceived(DeviceListReceivedEventArgs e)
        {
            var handler = DeviceListReceived;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnDeviceListFailed(DeviceListFailedEventArgs e)
        {
            var handler = DeviceListFailed;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnPrefixListReceived(PrefixListReceivedEventArgs e)
        {
            var handler = PrefixListReceived;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnPrefixListFailed(PrefixListFailedEventArgs e)
        {
            var handler = PrefixListFailed;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnCalled(CalledEventArgs e)
        {
            var handler = Called;
            if (handler != null)
            {
                handler(e);
            }
        }

        protected virtual void OnCallFailed(CallFailedEventArgs e)
        {
            var handler = CallFailed;
            if (handler != null)
            {
                handler(e);
            }
        }
    }
}
