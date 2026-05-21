using System.Net;
using System.Net.Sockets;
using System.Text;

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
                Task servidorHttpTask = Task.Run(() => AguardarRequests());
                servidorHttpTask.GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Erro ao iniciar o servidor na porta {Porta}:\n{ex.Message}");
            }
        }

        private async Task AguardarRequests()
        {
            while(true)
            {
                Socket conexao = await Controlador.AcceptSocketAsync();
                QtdeRequest++;
                Task task = Task.Run(() => ProcessarRequest(conexao, QtdeRequest));
            }
        }

        private void ProcessarRequest(Socket conexao, int numeroRequest)
        {
            Console.WriteLine($"Processando request #{numeroRequest}...\n");
            if(conexao.Connected)
            {
                byte[] bytesRequisicao = new byte[1024];
                conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);
                string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao)
                    .Replace((char)0, ' ').Trim();
                if(textoRequisicao.Length > 0)
                {
                    Console.WriteLine($"\n{textoRequisicao}\n");
                    conexao.Close();
                }
            }
            Console.WriteLine($"\nRequest {numeroRequest} finalizado.");
        }
    }
}
