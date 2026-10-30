import { useEffect, useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router';
import { ErroApi, requisicaoApi, type AuthResposta } from '../api';
import { usarSessao } from '../sessao';

export function PaginaLogin() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const { entrar, adicionarToast } = usarSessao();
  const navegar = useNavigate();
  const localizacao = useLocation();

  const destinoAnterior =
    (localizacao.state as { de?: string } | null)?.de || '/vendas';

  useEffect(() => {
    document.title = 'Entrar · Garimpo Vintage';
  }, []);

  async function lidarSubmit(e: FormEvent) {
    e.preventDefault();
    setErro(null);
    setEnviando(true);

    try {
      const resposta = await requisicaoApi<AuthResposta>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, senha }),
      });

      entrar(resposta);
      adicionarToast('sucesso', `Bem-vindo(a), ${resposta.nome}!`);
      navegar(destinoAnterior, { replace: true });
    } catch (err: unknown) {
      if (err instanceof ErroApi) {
        setErro(err.message);
      } else {
        setErro('Erro inesperado ao tentar entrar.');
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="container-formulario">
      <div className="cartao-formulario">
        <h1>Entrar na sua conta</h1>
        <p className="subtitulo-formulario">
          Acesse para comprar peças únicas do brechó ou gerenciar suas reservas.
        </p>

        {erro && (
          <div className="alerta alerta-erro" role="alert">
            {erro}
          </div>
        )}

        <form onSubmit={lidarSubmit} className="formulario">
          <div className="grupo-campo">
            <label htmlFor="login-email">E-mail</label>
            <input
              id="login-email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="ex: ana@exemplo.com"
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="login-senha">Senha</label>
            <input
              id="login-senha"
              type="password"
              required
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              placeholder="Sua senha"
              className="input-campo"
            />
          </div>

          <button type="submit" disabled={enviando} className="botao botao-primario botao-bloco">
            {enviando ? 'Autenticando...' : 'Entrar'}
          </button>
        </form>

        <p className="rodape-formulario">
          Ainda não tem conta? <Link to="/cadastro">Criar conta de cliente</Link>
        </p>
      </div>
    </div>
  );
}
