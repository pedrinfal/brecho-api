import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router';
import { ErroApi, requisicaoApi, type AuthResposta } from '../api';
import { usarSessao } from '../sessao';

function calcularForcaSenha(senha: string): { rotulo: string; nivel: number } {
  if (!senha) return { rotulo: 'Digite uma senha', nivel: 0 };
  let pontos = 0;
  if (senha.length >= 6) pontos++;
  if (senha.length >= 10) pontos++;
  if (/[A-Z]/.test(senha) && /[0-9]/.test(senha)) pontos++;
  if (/[^A-Za-z0-9]/.test(senha)) pontos++;

  if (pontos <= 1) return { rotulo: 'Fraca', nivel: 1 };
  if (pontos <= 2) return { rotulo: 'Média', nivel: 2 };
  return { rotulo: 'Forte', nivel: 3 };
}

export function PaginaCadastro() {
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmacao, setConfirmacao] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [errosLista, setErrosLista] = useState<string[]>([]);

  const { entrar, adicionarToast } = usarSessao();
  const navegar = useNavigate();

  useEffect(() => {
    document.title = 'Criar Conta · Garimpo Vintage';
  }, []);

  const forca = calcularForcaSenha(senha);

  async function lidarSubmit(e: FormEvent) {
    e.preventDefault();
    setErro(null);
    setErrosLista([]);

    if (senha !== confirmacao) {
      setErro('A confirmação de senha não confere.');
      return;
    }

    setEnviando(true);
    try {
      const resposta = await requisicaoApi<AuthResposta>('/api/auth/registro', {
        method: 'POST',
        body: JSON.stringify({ nome, email, senha }),
      });

      entrar(resposta);
      adicionarToast('sucesso', 'Conta criada com sucesso!');
      navegar('/vendas');
    } catch (err: unknown) {
      if (err instanceof ErroApi) {
        setErro(err.message);
        setErrosLista(err.errosValidacao);
      } else {
        setErro('Não foi possível concluir o cadastro.');
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="container-formulario">
      <div className="cartao-formulario">
        <h1>Criar conta de cliente</h1>
        <p className="subtitulo-formulario">
          Cadastre-se gratuitamente para reservar e comprar peças únicas no Garimpo Vintage.
        </p>

        {erro && (
          <div className="alerta alerta-erro" role="alert">
            <p>{erro}</p>
            {errosLista.length > 1 && (
              <ul>
                {errosLista.map((item, i) => (
                  <li key={i}>{item}</li>
                ))}
              </ul>
            )}
          </div>
        )}

        <form onSubmit={lidarSubmit} className="formulario">
          <div className="grupo-campo">
            <label htmlFor="cad-nome">Nome completo (fictício)</label>
            <input
              id="cad-nome"
              type="text"
              required
              minLength={2}
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              placeholder="Ex: Clara Mendes"
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="cad-email">E-mail (fictício)</label>
            <input
              id="cad-email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Ex: clara@exemplo.com"
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="cad-senha">Senha (mínimo 6 caracteres)</label>
            <input
              id="cad-senha"
              type="password"
              required
              minLength={6}
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              className="input-campo"
            />
            <div className="medidor-senha" aria-live="polite">
              <div className={`barra-forca nivel-${forca.nivel}`} />
              <span>Força da senha: {forca.rotulo}</span>
            </div>
          </div>

          <div className="grupo-campo">
            <label htmlFor="cad-confirmacao">Repetir senha</label>
            <input
              id="cad-confirmacao"
              type="password"
              required
              value={confirmacao}
              onChange={(e) => setConfirmacao(e.target.value)}
              className="input-campo"
            />
          </div>

          <button type="submit" disabled={enviando} className="botao botao-primario botao-bloco">
            {enviando ? 'Criando conta...' : 'Cadastrar e Entrar'}
          </button>
        </form>

        <p className="rodape-formulario">
          Já possui conta? <Link to="/login">Entrar agora</Link>
        </p>
      </div>
    </div>
  );
}
