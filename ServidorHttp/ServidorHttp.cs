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
        public string HtmlExemplo { get; set; }
        private SortedList<string, string> TiposMime {  get; set; }
        private SortedList<string, string> DiretoriosHosts { get; set; }

        public ServidorHttp(int porta = 8080)
        {
            Porta = porta;
            CriarHtmlExemplo();
            PopularTiposMime();
            PopularDiretorioHosts();
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
                if (textoRequisicao.Length > 0)
                {
                    Console.WriteLine($"\n{textoRequisicao}\n");

                    string[] linhas = textoRequisicao.Split("\r\n");
                    int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                    int iSegundoEspaco = linhas[0].LastIndexOf(' ');
                    string metodoHttp = linhas[0].Substring(0, iPrimeiroEspaco);
                    string recursoBuscado = linhas[0].Substring(
                        iPrimeiroEspaco + 1, iSegundoEspaco - iPrimeiroEspaco - 1);
                    if (recursoBuscado == "/") recursoBuscado = "/index.html";
                    recursoBuscado = recursoBuscado.Split("?")[0];
                    string versaoHttp = linhas[0].Substring(iSegundoEspaco + 1);
                    iPrimeiroEspaco = linhas[1].IndexOf(' ');
                    string nomeHost = linhas[1].Substring(iPrimeiroEspaco + 1);

                    byte[] bytesCabecalho = null;
                    byte[] bytesConteudo = null;
                    FileInfo fiArquivo = new FileInfo(ObterCaminhoFisicoArquivo(nomeHost, recursoBuscado));
                    if (fiArquivo.Exists)
                    {
                        if (TiposMime.ContainsKey(fiArquivo.Extension.ToLower()))
                        {
                            //bytesConteudo = File.ReadAllBytes(fiArquivo.FullName);
                            bytesConteudo = GerarHTMLDinamico(fiArquivo.FullName);
                            string tipoMime = TiposMime[fiArquivo.Extension.ToLower()];
                            bytesCabecalho = GerarCabecalho(versaoHttp, tipoMime, "200",
                             bytesConteudo.Length);
                        }
                        else
                        {
                            bytesConteudo = Encoding.UTF8.GetBytes(
                                "<h1>Erro 415 - Tipo de arquivo não suportado.</h1>");
                            bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8",
                                "415", bytesConteudo.Length);
                        }
                        
                    }
                    else
                    {
                        bytesConteudo = Encoding.UTF8.GetBytes(
                            "<h1>erro 404 - Arquivo Não Encontrado</h1>");
                        bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8",
                            "404", bytesConteudo.Length);
                    }
                    int bytesEnviados = conexao.Send(bytesCabecalho,
                            bytesCabecalho.Length, 0);
                        bytesEnviados += conexao.Send(bytesConteudo, bytesConteudo.Length, 0);
                        conexao.Close();
                        Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta à requisição #{numeroRequest}.");
                    
                }
            }
            Console.WriteLine($"\nRequest {numeroRequest} finalizado.");
        }
        public byte[] GerarCabecalho(string versaoHttp, string tipoMime,
            string codigoHttp, int qtdeBytes = 0)
        {
            StringBuilder texto = new StringBuilder();
            texto.Append($"{versaoHttp} {codigoHttp}{Environment.NewLine}");
            texto.Append($"Server: Servidor Http 1.0{Environment.NewLine}");
            texto.Append($"Content-Type: {tipoMime}{Environment.NewLine}");
            texto.Append($"Content-Length: {qtdeBytes}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes( texto.ToString() );

        }

        private void CriarHtmlExemplo()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html><html lang=\"pt-br\"><head><meta charset=\"UTF-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.Append("<title>Página Estática</title></head><body>");
            html.Append("<h1>Página Estática</h1></body></html>");
            HtmlExemplo = html.ToString();
        }
    
        private void PopularTiposMime()
        {
            TiposMime = new SortedList<string, string>();
            TiposMime.Add(".html", "text/html;charset=utf-8");
            TiposMime.Add(".htm", "text/html;charset=utf-8");
            TiposMime.Add(".css", "text/css");
            TiposMime.Add(".js", "text/javascript");
            TiposMime.Add(".png", "image/png");
            TiposMime.Add(".jpg", "image/jpeg");
            TiposMime.Add(".gif", "image/gif");
            TiposMime.Add(".svg", "image/svg+xml");
            TiposMime.Add(".wep", "image/webp");
            TiposMime.Add(".ico", "image/ico");
            TiposMime.Add(".woff", "font/woff");
            TiposMime.Add(".woff2", "font/woff2");
        }
    
        private void PopularDiretorioHosts()
        {
            DiretoriosHosts = new SortedList<string, string>();
            DiretoriosHosts.Add("localhost", "C:\\Projetos\\.Net\\ServidorHttp\\ServidorHttp\\www\\localhost");
            DiretoriosHosts.Add("silvio.com", "C:\\Projetos\\.Net\\ServidorHttp\\ServidorHttp\\www\\silvio.com");
        }

        public string ObterCaminhoFisicoArquivo(string host, string arquivo)
        {
            string diretorio = DiretoriosHosts[host.Split(":")[0]];
            string caminhoArquivo = diretorio + arquivo.Replace("/", "\\");
            return caminhoArquivo;
        }

        public byte[] GerarHTMLDinamico(string caminhoArquivo)
        {
            string coringa = "{{HtmlGerado}}";
            string htmlModelo = File.ReadAllText(caminhoArquivo);
            StringBuilder htmlGerado = new StringBuilder();
            htmlGerado.AppendLine("<ul>");
            foreach (var tipo in TiposMime.Keys)
            {
                htmlGerado.Append($"<li>Arquivos com extensão {tipo}</li>");
            }
            htmlGerado.AppendLine("</ul>");
            string textoHtmlGerado = htmlModelo.Replace(coringa, htmlGerado.ToString());

            return Encoding.UTF8.GetBytes(textoHtmlGerado, 0, textoHtmlGerado.Length);
        }
    }
}
