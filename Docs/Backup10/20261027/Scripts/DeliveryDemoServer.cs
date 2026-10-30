using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Read-only loopback document server. Delivery commands use the scoped UWB bridge,
// not unauthenticated HTTP endpoints and never the published website's cookies.
public sealed class DeliveryDemoServer:IDisposable {
 readonly TcpListener listener;readonly Thread worker;readonly string route,document;volatile bool stopped;
 public string Token{get;}public string Url{get;}
 public DeliveryDemoServer(string template){Token=Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N");route="/"+Token+"/index.html";document=template.Replace("__GRIDSHIFT_TOKEN__",Token);listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Url="http://127.0.0.1:"+((IPEndPoint)listener.LocalEndpoint).Port+route;worker=new Thread(Serve){IsBackground=true,Name="GridShift local demo document"};worker.Start();}
 void Serve(){while(!stopped){try{using(var client=listener.AcceptTcpClient()){client.ReceiveTimeout=2000;client.SendTimeout=2000;using(var stream=client.GetStream()){var request=new StringBuilder();int last=0;while(request.Length<8192){int value=stream.ReadByte();if(value<0)break;request.Append((char)value);if(value=='\n'&&last=='\n')break;if(value=='\n'&&request.ToString().EndsWith("\r\n\r\n"))break;last=value;}string first=request.ToString().Split('\n')[0].Trim();bool valid=first=="GET "+route+" HTTP/1.1"||first=="GET "+route+" HTTP/1.0";byte[] body=Encoding.UTF8.GetBytes(valid?document:"Not found");string header="HTTP/1.1 "+(valid?"200 OK":"404 Not Found")+"\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: "+body.Length+"\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\nReferrer-Policy: no-referrer\r\nContent-Security-Policy: default-src 'none'; script-src 'unsafe-inline'; style-src 'unsafe-inline'; img-src data:; connect-src 'none'; base-uri 'none'; frame-ancestors 'none'\r\nConnection: close\r\n\r\n";byte[] bytes=Encoding.ASCII.GetBytes(header);stream.Write(bytes,0,bytes.Length);stream.Write(body,0,body.Length);}}}catch(Exception){if(stopped)break;}}}
 public void Dispose(){stopped=true;listener.Stop();if(worker.IsAlive)worker.Join(500);}
}
