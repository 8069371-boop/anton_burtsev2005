using System;
using System.Net.Sockets;

namespace EchoServer.Abstractions
{
    public class TcpClientWrapper : ITcpClientWrapper
    {
        private readonly TcpClient _client;
        private bool _disposed;

        public TcpClientWrapper(TcpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public INetworkStreamWrapper GetStream()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TcpClientWrapper));
            
            return new NetworkStreamWrapper(_client.GetStream());
        }

        public void Close()
        {
            if (_disposed)
                return;
            
            _client.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client?.Close();
                    _client?.Dispose();
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
