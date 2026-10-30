import { useEffect, useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router';
import { requisicaoApi, type ListaPaginada, type Peca } from '../api';

export function PaginaVitrine() {
  const [searchParams, setSearchParams] = useSearchParams();
  const buscaUrl = searchParams.get('busca') || '';
  const paginaUrl = Math.max(1, Number(searchParams.get('pagina')) || 1);

  const [campoBusca, setCampoBusca] = useState(buscaUrl);
  const [dados, setDados] = useState<ListaPaginada<Peca> | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    document.title = 'Acervo · Garimpo Vintage';
  }, []);

  useEffect(() => {
    setCampoBusca(buscaUrl);
  }, [buscaUrl]);

  useEffect(() => {
    let ativo = true;
    setCarregando(true);
    setErro(null);

    const params = new URLSearchParams();
    if (buscaUrl.trim()) params.set('busca', buscaUrl.trim());
    params.set('pagina', String(paginaUrl));
    params.set('tamanho', '6');

    requisicaoApi<ListaPaginada<Peca>>(`/api/pecas?${params.toString()}`)
      .then((res) => {
        if (ativo) {
          setDados(res);
          setCarregando(false);
        }
      })
      .catch((err: unknown) => {
        if (ativo) {
          setErro(err instanceof Error ? err.message : 'Erro ao carregar acervo.');
          setCarregando(false);
        }
      });

    return () => {
      ativo = false;
    };
  }, [buscaUrl, paginaUrl]);

  function submeterBusca(e: FormEvent) {
    e.preventDefault();
    const proximo = new URLSearchParams();
    if (campoBusca.trim()) {
      proximo.set('busca', campoBusca.trim());
    }
    proximo.set('pagina', '1');
    setSearchParams(proximo);
  }

  function limparBusca() {
    setCampoBusca('');
    setSearchParams({});
  }

  function irParaPagina(novaPagina: number) {
    const proximo = new URLSearchParams();
    if (buscaUrl.trim()) proximo.set('busca', buscaUrl.trim());
    proximo.set('pagina', String(novaPagina));
    setSearchParams(proximo);
  }

  return (
    <div className="pagina-vitrine">
      <section className="hero-banner">
        <div className="hero-conteudo">
          <span className="hero-selo">Moda Circular & Peças Únicas</span>
          <h1>Garimpo com história, curadoria feita peça a peça.</h1>
          <p>
            Cada item do nosso brechó possui estoque exclusivo de uma única unidade. Garanta seus achados vintage antes que alguém leve!
          </p>
          <div className="hero-acoes">
            <a href="#acervo-lista" className="botao botao-primario">
              Explorar Peças
            </a>
            <Link to="/vendas" className="botao botao-contorno">
              Registrar Compra
            </Link>
          </div>
        </div>
      </section>

      <section id="acervo-lista" className="secao-acervo">
        <div className="barra-busca-container">
          <form onSubmit={submeterBusca} className="form-busca" role="search">
            <label htmlFor="campo-busca-pecas" className="sr-only">
              Buscar peça por nome, categoria ou tamanho
            </label>
            <input
              id="campo-busca-pecas"
              type="search"
              value={campoBusca}
              onChange={(e) => setCampoBusca(e.target.value)}
              placeholder="Buscar por jaqueta, vestido, couro, tamanho M..."
              className="input-campo input-busca"
            />
            <button type="submit" className="botao botao-primario">
              Buscar
            </button>
            {buscaUrl && (
              <button type="button" onClick={limparBusca} className="botao botao-contorno">
                Limpar
              </button>
            )}
          </form>
        </div>

        {erro && (
          <div className="alerta alerta-erro" role="alert">
            <strong>Atenção:</strong> {erro}
          </div>
        )}

        {carregando ? (
          <div className="grade-cards" aria-busy="true" aria-label="Carregando peças do acervo">
            {[1, 2, 3, 4, 5, 6].map((item) => (
              <div key={item} className="card-peca card-esqueleto">
                <div className="esqueleto-topo" />
                <div className="esqueleto-linha esqueleto-titulo" />
                <div className="esqueleto-linha esqueleto-texto" />
                <div className="esqueleto-linha esqueleto-texto-curto" />
                <div className="esqueleto-rodape" />
              </div>
            ))}
          </div>
        ) : dados && dados.itens.length === 0 ? (
          <div className="estado-vazio">
            <h2>Nenhuma peça encontrada no garimpo</h2>
            <p>
              Não encontramos peças para <strong>"{buscaUrl}"</strong>. Tente outros termos ou veja todo o acervo disponível.
            </p>
            <button type="button" onClick={limparBusca} className="botao botao-primario">
              Limpar busca e ver tudo
            </button>
          </div>
        ) : (
          dados && (
            <>
              <div className="resumo-resultados">
                <span>
                  Mostrando <strong>{dados.itens.length}</strong> de <strong>{dados.total}</strong> peça(s) no acervo
                </span>
              </div>

              <div className="grade-cards">
                {dados.itens.map((peca) => (
                  <article key={peca.id} className="card-peca">
                    <div className="card-peca-cabecalho">
                      <span className="categoria-tag">
                        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                          <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
                          <line x1="7" y1="7" x2="7.01" y2="7" />
                        </svg>
                        {peca.categoria}
                      </span>
                      <span className={`etiqueta-situacao ${peca.vendida ? 'etiqueta-vendida' : 'etiqueta-disponivel'}`}>
                        {peca.vendida ? 'Peça Vendida' : 'Disponível'}
                      </span>
                    </div>

                    <h2 className="card-titulo">
                      <Link to={`/pecas/${peca.id}`}>{peca.nome}</Link>
                    </h2>

                    <p className="card-descricao">{peca.descricao}</p>

                    <div className="card-metadados">
                      <span>
                        <strong>Tam:</strong> {peca.tamanho}
                      </span>
                      <span>
                        <strong>Estado:</strong> {peca.condicao}
                      </span>
                    </div>

                    <div className="card-rodape">
                      <span className="card-preco">
                        {peca.preco.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                      </span>
                      <div className="card-acoes">
                        <Link to={`/pecas/${peca.id}`} className="botao botao-contorno botao-compacto">
                          Detalhes
                        </Link>
                        {!peca.vendida && (
                          <Link
                            to={`/vendas?pecaId=${peca.id}`}
                            className="botao botao-primario botao-compacto"
                          >
                            Comprar
                          </Link>
                        )}
                      </div>
                    </div>
                  </article>
                ))}
              </div>

              {dados.totalPaginas > 1 && (
                <nav className="paginacao" aria-label="Paginação do acervo">
                  <button
                    type="button"
                    disabled={dados.pagina <= 1}
                    onClick={() => irParaPagina(dados.pagina - 1)}
                    className="botao botao-contorno botao-compacto"
                  >
                    ← Anterior
                  </button>
                  <span className="paginacao-info">
                    Página {dados.pagina} de {dados.totalPaginas}
                  </span>
                  <button
                    type="button"
                    disabled={dados.pagina >= dados.totalPaginas}
                    onClick={() => irParaPagina(dados.pagina + 1)}
                    className="botao botao-contorno botao-compacto"
                  >
                    Próxima →
                  </button>
                </nav>
              )}
            </>
          )
        )}
      </section>
    </div>
  );
}
