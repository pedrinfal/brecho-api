import { useEffect, useRef, useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router';
import { ErroApi, requisicaoApi, type ListaPaginada, type Peca } from '../api';
import { usarSessao } from '../sessao';

export function PaginaAdminPecas() {
  const { sessao, adicionarToast } = usarSessao();
  const [searchParams, setSearchParams] = useSearchParams();
  const editarIdParam = searchParams.get('editar');

  const [pecas, setPecas] = useState<Peca[]>([]);
  const [carregando, setCarregando] = useState(true);

  const [nome, setNome] = useState('');
  const [descricao, setDescricao] = useState('');
  const [tamanho, setTamanho] = useState('M');
  const [categoria, setCategoria] = useState('Casacos e Jaquetas');
  const [condicao, setCondicao] = useState('Excelente (seminova)');
  const [preco, setPreco] = useState('120.00');
  const [ativa, setAtiva] = useState(true);

  const [enviando, setEnviando] = useState(false);
  const [erroForm, setErroForm] = useState<string | null>(null);

  const [pecaParaExcluir, setPecaParaExcluir] = useState<Peca | null>(null);
  const dialogRef = useRef<HTMLDialogElement | null>(null);
  const botaoSeguroRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    document.title = 'Painel Admin · Garimpo Vintage';
  }, []);

  async function carregarAcervo() {
    if (!sessao) return;
    setCarregando(true);
    try {
      const res = await requisicaoApi<ListaPaginada<Peca>>(
        '/api/pecas?pagina=1&tamanho=50',
        {},
        sessao.token
      );
      setPecas(res.itens);
    } catch (err: unknown) {
      adicionarToast('erro', err instanceof Error ? err.message : 'Erro ao carregar acervo.');
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    if (sessao?.papel === 'admin') {
      void carregarAcervo();
    } else {
      setCarregando(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sessao]);

  useEffect(() => {
    if (!editarIdParam) {
      limparFormulario();
      return;
    }
    const idNum = Number(editarIdParam);
    const encontrada = pecas.find((p) => p.id === idNum);
    if (encontrada) {
      setNome(encontrada.nome);
      setDescricao(encontrada.descricao);
      setTamanho(encontrada.tamanho);
      setCategoria(encontrada.categoria);
      setCondicao(encontrada.condicao);
      setPreco(String(encontrada.preco));
      setAtiva(encontrada.ativa);
    }
  }, [editarIdParam, pecas]);

  function limparFormulario() {
    setNome('');
    setDescricao('');
    setTamanho('M');
    setCategoria('Casacos e Jaquetas');
    setCondicao('Excelente (seminova)');
    setPreco('120.00');
    setAtiva(true);
    setErroForm(null);
  }

  if (!sessao || sessao.papel !== 'admin') {
    return (
      <div className="estado-vazio">
        <h1>Acesso restrito à curadoria (Admin)</h1>
        <p>
          Esta área é exclusiva para contas com o papel <strong>admin</strong>. Faça login com a conta de administrador para cadastrar, editar ou remover peças do brechó.
        </p>
        <Link to="/login" className="botao botao-primario">
          Ir para o Login
        </Link>
      </div>
    );
  }

  async function lidarSalvar(e: FormEvent) {
    e.preventDefault();
    if (!sessao) return;
    setErroForm(null);
    setEnviando(true);

    try {
      if (editarIdParam) {
        await requisicaoApi<Peca>(
          `/api/pecas/${editarIdParam}`,
          {
            method: 'PUT',
            body: JSON.stringify({
              nome,
              descricao,
              tamanho,
              categoria,
              condicao,
              preco: Number(preco),
              ativa,
            }),
          },
          sessao.token
        );
        adicionarToast('sucesso', 'Peça atualizada com sucesso!');
        setSearchParams({});
      } else {
        await requisicaoApi<Peca>(
          '/api/pecas',
          {
            method: 'POST',
            body: JSON.stringify({
              nome,
              descricao,
              tamanho,
              categoria,
              condicao,
              preco: Number(preco),
            }),
          },
          sessao.token
        );
        adicionarToast('sucesso', 'Nova peça cadastrada no acervo!');
        limparFormulario();
      }
      await carregarAcervo();
    } catch (err: unknown) {
      if (err instanceof ErroApi) {
        setErroForm(err.message);
        adicionarToast('erro', err.message);
      } else {
        setErroForm('Erro ao salvar peça.');
      }
    } finally {
      setEnviando(false);
    }
  }

  function solicitarExclusao(peca: Peca) {
    setPecaParaExcluir(peca);
    if (dialogRef.current) {
      dialogRef.current.showModal();
      window.setTimeout(() => {
        botaoSeguroRef.current?.focus();
      }, 20);
    }
  }

  function fecharDialog() {
    dialogRef.current?.close();
    setPecaParaExcluir(null);
  }

  async function confirmarExclusao() {
    if (!sessao || !pecaParaExcluir) return;
    try {
      await requisicaoApi<void>(
        `/api/pecas/${pecaParaExcluir.id}`,
        { method: 'DELETE' },
        sessao.token
      );
      adicionarToast('sucesso', 'Peça excluída do acervo.');
      fecharDialog();
      await carregarAcervo();
    } catch (err: unknown) {
      fecharDialog();
      if (err instanceof ErroApi) {
        adicionarToast('erro', err.message);
      }
    }
  }

  return (
    <div className="pagina-grid-dupla">
      <section className="cartao-formulario">
        <h1>{editarIdParam ? `Editar Peça #${editarIdParam}` : 'Cadastrar Nova Peça'}</h1>
        <p className="subtitulo-formulario">
          Gerencie o catálogo do Garimpo Vintage. Peças com vendas vinculadas retornam <strong>409 Conflict</strong> ao tentar excluir.
        </p>

        {erroForm && (
          <div className="alerta alerta-erro" role="alert">
            {erroForm}
          </div>
        )}

        <form onSubmit={lidarSalvar} className="formulario">
          <div className="grupo-campo">
            <label htmlFor="adm-nome">Nome da peça</label>
            <input
              id="adm-nome"
              type="text"
              required
              minLength={3}
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              placeholder="Ex: Sobretudo Lã Batida Vintage"
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="adm-desc">Descrição detalhada</label>
            <textarea
              id="adm-desc"
              rows={3}
              required
              minLength={5}
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              placeholder="Tecido, época, medidas e detalhes da peça..."
              className="input-campo"
            />
          </div>

          <div className="linha-campos">
            <div className="grupo-campo">
              <label htmlFor="adm-tam">Tamanho</label>
              <input
                id="adm-tam"
                type="text"
                required
                value={tamanho}
                onChange={(e) => setTamanho(e.target.value)}
                className="input-campo"
              />
            </div>

            <div className="grupo-campo">
              <label htmlFor="adm-preco">Preço (R$)</label>
              <input
                id="adm-preco"
                type="number"
                step="0.01"
                min="0.01"
                required
                value={preco}
                onChange={(e) => setPreco(e.target.value)}
                className="input-campo"
              />
            </div>
          </div>

          <div className="grupo-campo">
            <label htmlFor="adm-cat">Categoria</label>
            <input
              id="adm-cat"
              type="text"
              required
              value={categoria}
              onChange={(e) => setCategoria(e.target.value)}
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="adm-cond">Condição de conservação</label>
            <input
              id="adm-cond"
              type="text"
              required
              value={condicao}
              onChange={(e) => setCondicao(e.target.value)}
              className="input-campo"
            />
          </div>

          {editarIdParam && (
            <div className="grupo-checkbox">
              <input
                id="adm-ativa"
                type="checkbox"
                checked={ativa}
                onChange={(e) => setAtiva(e.target.checked)}
              />
              <label htmlFor="adm-ativa">Peça visível na vitrine pública (Ativa)</label>
            </div>
          )}

          <div className="acoes-formulario">
            <button type="submit" disabled={enviando} className="botao botao-primario">
              {enviando ? 'Salvando...' : editarIdParam ? 'Salvar Alterações' : 'Cadastrar Peça'}
            </button>
            {editarIdParam && (
              <button
                type="button"
                onClick={() => setSearchParams({})}
                className="botao botao-contorno"
              >
                Cancelar Edição
              </button>
            )}
          </div>
        </form>
      </section>

      <section className="secao-lista-vendas">
        <h2>Acervo completo ({pecas.length} peças, incluindo inativas)</h2>

        {carregando ? (
          <div className="cartao-formulario">
            <div className="esqueleto-linha esqueleto-titulo" />
            <div className="esqueleto-linha esqueleto-texto" />
          </div>
        ) : (
          <div className="lista-cartoes-venda">
            {pecas.map((p) => (
              <article key={p.id} className="card-peca">
                <div className="card-peca-cabecalho">
                  <span className="categoria-tag">
                    #{p.id} · {p.categoria}
                  </span>
                  <div className="tags-grupo">
                    {!p.ativa && <span className="etiqueta-situacao etiqueta-inativa">Inativa</span>}
                    <span
                      className={`etiqueta-situacao ${p.vendida ? 'etiqueta-vendida' : 'etiqueta-disponivel'}`}
                    >
                      {p.vendida ? 'Vendida' : 'Disponível'}
                    </span>
                  </div>
                </div>
                <h3 className="card-titulo">{p.nome}</h3>
                <p className="card-descricao">{p.descricao}</p>
                <div className="card-rodape">
                  <span className="card-preco">
                    {p.preco.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
                  <div className="card-acoes">
                    <button
                      type="button"
                      onClick={() => setSearchParams({ editar: String(p.id) })}
                      className="botao botao-contorno botao-compacto"
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => solicitarExclusao(p)}
                      className="botao botao-perigo botao-compacto"
                    >
                      Excluir
                    </button>
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <dialog ref={dialogRef} className="modal-confirmacao">
        <h3>Confirmar exclusão da peça</h3>
        <p>
          Tem certeza que deseja excluir permanentemente a peça{' '}
          <strong>{pecaParaExcluir?.nome}</strong>?
        </p>
        <div className="modal-acoes">
          <button
            ref={botaoSeguroRef}
            type="button"
            onClick={fecharDialog}
            className="botao botao-contorno"
          >
            Cancelar
          </button>
          <button type="button" onClick={confirmarExclusao} className="botao botao-perigo">
            Confirmar Exclusão
          </button>
        </div>
      </dialog>
    </div>
  );
}
