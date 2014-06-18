using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ItExpert.Model;

namespace ItExpert.ServiceLayer
{
    public class HttpConnector : IDisposable
    {
        #region Fields

        private const string Method = "POST";

        private const string Url = Settings.Domen + "mobile/tim45250/v1/index.php";

        private const string CssUrl = Settings.Domen + "mobile/tim45250/v1/style.css";

        public event EventHandler<HttpConnectorEventArgs> DataReceived;

        private const int DefaultTimeout = 1 * 60 * 1000;

        private HttpWebRequest _req = null;

        private bool _cancelled = false;

        private bool _operationComplete = false;

        private object _lockObj = new object();

        private RequestState _activeRequest = null;

        #endregion

        public void GetData(string query, object state)
        {
            try
            {
                if (_req != null)
                {
                    _req.Abort();
                }
                _req = null;
                _req = (HttpWebRequest)WebRequest.Create(Url);
                _req.Method = Method;
                var bytes = Encoding.UTF8.GetBytes(query);
                _req.ContentType = "application/x-www-form-urlencoded";
                _req.ContentLength = bytes.Length;
                using (var reqStream = _req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                    reqStream.Flush();
                }
                var requestState = new RequestState();
                requestState.Request = _req;
                requestState.State = state;
                _req.AllowReadStreamBuffering = true;
                var result = (IAsyncResult)_req.BeginGetResponse(DataTransmissed, requestState);
                requestState.Handle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, requestState, DefaultTimeout, true);
                Interlocked.Exchange(ref _activeRequest, requestState);
                Monitor.Enter(_lockObj);
                if (_cancelled)
                {
                    Abort();
                }
                Monitor.Exit(_lockObj);
            }
            catch (Exception ex)
            {
            }
        }

        public void GetCss()
        {
            try
            {
                if (_req != null)
                {
                    _req.Abort();
                }
                _req = null;
                _req = (HttpWebRequest)WebRequest.Create(CssUrl);
                _req.Method = "GET";
                var requestState = new RequestState();
                requestState.Request = _req;
                var result = (IAsyncResult)_req.BeginGetResponse(DataTransmissed, requestState);
                requestState.Handle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, requestState, DefaultTimeout, true);
                Interlocked.Exchange(ref _activeRequest, requestState);
                Monitor.Enter(_lockObj);
                if (_cancelled)
                {
                    Abort();
                }
                Monitor.Exit(_lockObj);
            }
            catch (Exception ex)
            {
            }
        }

        public void RaiseAbortEvent()
        {
            var handler = Interlocked.CompareExchange(ref DataReceived, null, null);
            if (handler != null)
            {
                var eventArgs = new HttpConnectorEventArgs() {Abort = true};
                handler(this, eventArgs);
            }
        }

        public void Dispose()
        {
            DataReceived = null;
            Abort();
            _req = null;
            _lockObj = null;
            _activeRequest = null;
        }

        public void Abort()
        {
            Monitor.Enter(_lockObj);
            _cancelled = true;
            if (_activeRequest != null && !_operationComplete)
            {
                _activeRequest.Handle.Unregister(null);
                HttpWebRequest request = _activeRequest.Request;
                if (request != null)
                    request.Abort();
                Interlocked.Exchange(ref _activeRequest, null);
            }
            Monitor.Exit(_lockObj);
        }

        private void DataTransmissed(IAsyncResult result)
        {
            string data = null;
            var error = false;
            var requestState = (RequestState)result.AsyncState;
            WebResponse response = null;
            try
            {
                var request = requestState.Request;
                response = request.EndGetResponse(result);
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        data = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                error = true;
            }
            finally
            {
                if (response != null) response.Close();
                _req = null;
                var handler = Interlocked.CompareExchange(ref DataReceived, null, null);
                if (handler != null)
                {
                    var eventArgs = new HttpConnectorEventArgs() { Error = error, Data = data, State = requestState.State };
                    handler(this, eventArgs);
                }   
            }
        }

        private void TimeoutCallback(object state, bool timedOut)
        {
            var requestState = state as RequestState;
            if (timedOut)
            {
                if (requestState != null)
                {
                    requestState.Request.Abort();
                }
            }
            if (requestState != null) requestState.Handle.Unregister(null);
        }
    }

    public class HttpConnectorEventArgs : EventArgs
    {
        public string Data { get; set; }

        public byte[] PdfData { get; set; }

        public object State { get; set; }

        public bool Error { get; set; }

        public bool Abort { get; set; }
    }

    public class RequestState
    {
        public HttpWebRequest Request { get; set; }

        public RegisteredWaitHandle Handle { get; set; }

        public object State { get; set; }
    }
}
