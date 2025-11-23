using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoServer.Abstractions
{
    public class TcpListenerWrapper : ITcpListenerWrapper, IDisposable
    {
        private readonly TcpListener _listener;
        private bool _disposed;

        public TcpListenerWrapper(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public async Task<ITcpClientWrapper> AcceptTcpClientAsync()
        {
            var client = await _listener.AcceptTcpClientAsync();
            return new TcpClientWrapper(client);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Звільнення managed ресурсів
                    _listener?.Stop();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
