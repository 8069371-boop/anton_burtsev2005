using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using EchoServer.Abstractions;

namespace EchoServer.Services
{
    public class UdpTimedSender : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly UdpClient _udpClient;
        private Timer? _timer;
        private ushort _counter = 0;
        private bool _disposed;
        
        // S2245: Random is used only for generating test data payload, not for any security-sensitive operations
#pragma warning disable S2245
        private readonly Random _random = new Random();
#pragma warning restore S2245

        public UdpTimedSender(string host, int port, ILogger logger)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _udpClient = new UdpClient();
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UdpTimedSender));
            
            if (_timer != null)
                throw new InvalidOperationException("Sender is already running.");
            
            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private void SendMessageCallback(object? state)
        {
            if (_disposed)
                return;
            
            try
            {
                byte[] samples = new byte[1024];
                _random.NextBytes(samples);
                _counter++;
                byte[] msg = (new byte[] { 0x04, 0x84 })
                    .Concat(BitConverter.GetBytes(_counter))
                    .Concat(samples)
                    .ToArray();
                    
                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _udpClient.Send(msg, msg.Length, endpoint);
                _logger.Log($"Message sent to {_host}:{_port}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
            }
        }

        public void StopSending()
        {
            if (_disposed)
                return;
            
            _timer?.Dispose();
            _timer = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                    _timer = null;
                    _udpClient?.Dispose();
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
