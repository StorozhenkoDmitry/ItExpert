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
    public class PdfLoader : IDisposable 
    {
        #region Fields

        private const string Method = "POST";

        private const string Url = Settings.Domen + "mobile/tim45250/v1/index.php";

        public event EventHandler<PdfEventArgs> PdfGetted;

        private const int DefaultTimeout = 1 * 60 * 1000;

        private bool _operationComplete = false;

        private RequestState _activeRequest = null;

        private bool _isOperation;

        #endregion

        #region Public methods

        public void BeginGetMagazinePdf(string src)
        {
            _isOperation = true;
            var query = "action=get&object=pdf&SRC=" + src;
            BeginLoadPdf(query);
        }

        public void Dispose()
        {
            PdfGetted = null;
            AbortOperation();
            _activeRequest = null;
        }

        public void AbortOperation()
        {
            try
            {
                _isOperation = false;
                Abort();
            }
            catch (Exception)
            {
                
            }
        }

        public bool IsOperation()
        {
            return _isOperation;
        }

        #endregion

        #region Private methods

        private void BeginLoadPdf(string query)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(Url);
                req.Method = Method;
                var bytes = Encoding.UTF8.GetBytes(query);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bytes.Length;
                using (var reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                    reqStream.Flush();
                }
                var requestState = new RequestState();
                requestState.Request = req;
                req.AllowReadStreamBuffering = true;
                var result = (IAsyncResult)req.BeginGetResponse(DataTransmissed, requestState);
                _operationComplete = false;
                requestState.Handle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, requestState, DefaultTimeout, true);
                Interlocked.Exchange(ref _activeRequest, requestState);
            }
            catch (Exception ex)
            {
                var handler = Interlocked.CompareExchange(ref PdfGetted, null, null);
                if (handler != null)
                {
                    var eventArgs = new PdfEventArgs() {Error = true};
                    handler(this, eventArgs);
                }
            }
        }

        private void DataTransmissed(IAsyncResult result)
        {
            byte[] pdf = null;
            var error = false;
            var aborted = false;
            var requestState = (RequestState)result.AsyncState;
            WebResponse response = null;
            try
            {
                var request = requestState.Request;
                response = request.EndGetResponse(result);
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    if (response.ContentType == "application/pdf")
                    {
                        var rd = new StreamReader(stream, Encoding.Unicode);
                        var data = rd.ReadToEnd();
                        pdf = Encoding.Unicode.GetBytes(data);
                    }
                }
            }
            catch (Exception e)
            {
                aborted = true;
                error = true;
            }
            finally
            {
                if (response != null) response.Close();
                _operationComplete = true;
                _isOperation = false;
                var handler = Interlocked.CompareExchange(ref PdfGetted, null, null);
                if (handler != null)
                {
                    var eventArgs = new PdfEventArgs() { Error = error, Abort = aborted, Pdf = pdf };
                    handler(this, eventArgs);
                }
            }
        }

        private void Abort()
        {
            try
            {
                if (_activeRequest != null && !_operationComplete)
                {
                    _activeRequest.Handle.Unregister(null);
                    HttpWebRequest request = _activeRequest.Request;
                    if (request != null)
                        request.Abort();
                    Interlocked.Exchange(ref _activeRequest, null);
                }
            }
            catch (Exception)
            {
            }
            
        }

        private void TimeoutCallback(object state, bool timedOut)
        {
            var requestState = state as RequestState;
            if (timedOut)
            {
                if (requestState != null && requestState.Request != null)
                {
                    requestState.Request.Abort();
                }
            }
            if (requestState != null) requestState.Handle.Unregister(null);
        }

        #endregion
    }
}
