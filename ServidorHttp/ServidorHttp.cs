using System.Net;
using System.Net.Sockets;

namespace ServidorHttp
{
     class ServidorHttp
    {
        private TcpListener Controlador {  get; set; }
        private int Porta { get; set; }
        private int QtdeRequest { get; set; }

        public ServidorHttp(int porta = 8080)
        {
            Porta = porta;
            try
            {
                Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), Porta);
                Controlador.Start();
                Console.WriteLine($"Servidor HTTP iniciado na porta {Porta}");
                Console.WriteLine($"Para acessar, digite: http://localhost:{Porta}/");
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Erro ao iniciar o servidor na porta {Porta}:\n{ex.Message}");
            }
        }
    }
}
