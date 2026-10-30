import type { ReactNode } from 'react';
import { Link, NavLink, useNavigate } from 'react-router';
import { usarSessao } from '../sessao';

export function Layout({ children }: { children: ReactNode }) {
  const { sessao, sair, tema, alternarTema, toasts, removerToast } = usarSessao();
  const navegar = useNavigate();

  function lidarSair() {
    sair('Você saiu da sua conta.');
    navegar('/');
  }

  return (
    <div className="estrutura-pagina">
      <a href="#conteudo-principal" className="pular-conteudo">
        Pular para o conteúdo
      </a>

      <header className="cabecalho">
        <div className="container cabecalho-interno">
          <Link to="/" className="marca" aria-label="Garimpo Vintage — Página Inicial">
            <svg
              className="marca-icone"
              viewBox="0 0 36 36"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
              aria-hidden="true"
            >
              <path
                d="M18 6C15.7909 6 14 7.79086 14 10C14 11.8638 15.2747 13.4299 17 13.874V16L5 24.5C4.17157 25.08 4.5818 26.5 5.6 26.5H30.4C31.4182 26.5 31.8284 25.08 31 24.5L19 16V13.874C20.7253 13.4299 22 11.8638 22 10C22 7.79086 20.2091 6 18 6Z"
                stroke="currentColor"
                strokeWidth="2.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
            <span className="marca-texto">Garimpo Vintage</span>
          </Link>

          <nav className="navegacao" aria-label="Menu principal">
            <NavLink to="/" end className="nav-link">
              Acervo
            </NavLink>

            {sessao && (
              <NavLink to="/vendas" className="nav-link">
                {sessao.papel === 'admin' ? 'Vendas' : 'Minhas Compras'}
              </NavLink>
            )}

            {sessao?.papel === 'admin' && (
              <NavLink to="/admin/pecas" className="nav-link nav-admin">
                Admin
              </NavLink>
            )}

            {!sessao ? (
              <NavLink to="/login" className="botao botao-primario botao-compacto">
                Entrar
              </NavLink>
            ) : (
              <button
                type="button"
                onClick={lidarSair}
                className="botao botao-contorno botao-compacto"
                title={`Logado como ${sessao.nome} (${sessao.papel})`}
              >
                Sair
              </button>
            )}

            <button
              type="button"
              onClick={alternarTema}
              className="botao-icone"
              aria-label={tema === 'claro' ? 'Ativar tema escuro' : 'Ativar tema claro'}
              title={tema === 'claro' ? 'Ativar tema escuro' : 'Ativar tema claro'}
            >
              {tema === 'claro' ? (
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
                </svg>
              ) : (
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="5" />
                  <path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" />
                </svg>
              )}
            </button>
          </nav>
        </div>
      </header>

      <main id="conteudo-principal" className="conteudo-principal container">
        {children}
      </main>

      <div className="regiao-toasts" aria-live="polite" aria-atomic="true">
        {toasts.map((t) => (
          <div key={t.id} className={`toast toast-${t.tipo}`} role="status">
            <span>{t.texto}</span>
            <button
              type="button"
              className="toast-fechar"
              onClick={() => removerToast(t.id)}
              aria-label="Fechar aviso"
            >
              ×
            </button>
          </div>
        ))}
      </div>

      <footer className="rodape">
        <div className="container rodape-interno">
          <p className="rodape-marca">Garimpo Vintage · Brechó & Moda Circular</p>
          <p className="rodape-aviso">
            Projeto escolar — empresa fictícia, não é um serviço real. Não use seu e-mail nem sua senha de verdade.
          </p>
        </div>
      </footer>
    </div>
  );
}
