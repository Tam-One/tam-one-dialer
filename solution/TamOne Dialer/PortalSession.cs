using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace TamOne_Dialer
{
    public class PortalSession
    {

        public delegate void LoginResult(PortalSession session);
        public delegate void DeviceListResult(ICollection<Device> devices);
        private CookieContainer cookies;

        public Device Device { get; set; }

        private PortalSession()
        {

        }

        public void BeginCall(string number)
        {
            Thread t = new Thread(delegate()
            {
                string url = "https://demoportal.voipcentrale.nl/nl/ajax/user/init_call/1/0/" + number + "/" + this.Device.Id + "/2";
                Console.WriteLine(url);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                request.CookieContainer = this.cookies;
                request.AllowAutoRedirect = false;
                request.Headers.Add("X-Requested-With: XMLHttpRequest");
                HttpWebResponse r = (HttpWebResponse)request.GetResponse();
                StreamReader s = new StreamReader(r.GetResponseStream());
                Console.WriteLine((int)(r.StatusCode) + " " + r.StatusDescription);
                Console.WriteLine(s.ReadToEnd());
            });

            t.Start();
        }

        public void BeginGetDeviceList(DeviceListResult result)
        {
            Thread t = new Thread(delegate()
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://demoportal.voipcentrale.nl/nl/ajax/user/device_list");
                request.Method = "GET";
                request.CookieContainer = this.cookies;
                request.AllowAutoRedirect = false;
                request.Headers.Add("X-Requested-With: XMLHttpRequest");
                HttpWebResponse r = (HttpWebResponse)request.GetResponse();
                StreamReader s = new StreamReader(r.GetResponseStream());
                string body = s.ReadToEnd();
                Console.WriteLine("status " + r.StatusCode + " " + r.StatusDescription);
                Console.WriteLine("de body" + body);
                Console.WriteLine(r.Headers["Location"]);
                dynamic json = JObject.Parse(body);
                var devices = new List<Device>();
                foreach(dynamic device in json.data) {
                    devices.Add(new Device((string)device.id, (string)device.name));
                    Console.WriteLine(device.id + ": " + device.name);
                }
                result(devices);
            });
            t.Start();
        }

        public static void BeginLogin(string username, string password, LoginResult result)
        {
            
            Thread t = new Thread(delegate()
            {
                CookieContainer cookies = new CookieContainer();
                /*{
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://portal.voipcentrale.nl/nl/auth/login");
                    request.Method = "GET";
                    StreamReader s = new StreamReader(request.GetResponse().GetResponseStream());
                    string body = s.ReadToEnd();
                    var tokenPos = body.IndexOf("name=\"top_csrf_tok\"");
                    if (tokenPos == -1)
                    {
                        result(null);
                        Console.WriteLine("token not found");
                        return;
                    }

                    // 27
                    var token = body.Substring(tokenPos + 27, body.IndexOf("\"", tokenPos + 27) - (tokenPos + 27));
                    Console.WriteLine(token);
                }*/
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://demoportal.voipcentrale.nl/nl/auth/login");
                    request.Method = "POST";
                    request.Headers.Add("X-Requested-With: XMLHttpRequest");
                    request.CookieContainer = cookies;
                    cookies.Add(new Uri("https://demoportal.voipcentrale.nl/"), new Cookie("top_csrf", "yolo"));
                    request.ContentType = "application/x-www-form-urlencoded";
                    //request.Headers.Add("Cookie: top_csrf=yolo");
                    request.Referer = "https://demoportal.voipcentrale.nl/nl/auth/login";
                    request.AllowAutoRedirect = false;
                    byte[] postBody = Encoding.UTF8.GetBytes("top_csrf_tok=yolo&identity=" +
                        HttpUtility.UrlEncode(username, Encoding.UTF8) + "&password=" +
                        HttpUtility.UrlEncode(password, Encoding.UTF8));
                    request.ContentLength = postBody.Length;
                    Stream stream = request.GetRequestStream();
                    stream.Write(postBody, 0, postBody.Length);
                    stream.Close();
                    HttpWebResponse r = (HttpWebResponse) request.GetResponse();
                    Console.WriteLine((int)(r.StatusCode) + " " + r.StatusDescription);
                    StreamReader s = new StreamReader(r.GetResponseStream());
                    string body = s.ReadToEnd();
                    Console.WriteLine("De body" + body);
                    //Console.WriteLine(r.Headers["Location"]);

                    if (!(bool)(((dynamic)JObject.Parse(body)).status))
                    {
                        result(null);
                    }
                    else
                    {
                        foreach (Cookie cookieValue in r.Cookies)
                        {
                            Console.WriteLine("Cookie: " + cookieValue.ToString());
                        }
                        var session = new PortalSession();
                        session.cookies = cookies;
                        session.cookies.Add(new Uri("https://demoportal.voipcentrale.nl/"), r.Cookies);
                        result(session);
                    }
                }
                

            });
            t.Start();
            
        }
    }
}
