import { useEffect, useRef, useState, type FormEvent } from 'react';
import { Navigate, useLocation, useSearchParams } from 'react-router';
import {
  ErroApi,
  requisicaoApi,
  type ListaPaginada,
  type Peca,
  type Venda,
} from '../api';
import { usarSessao } from '../sessao';

function dataHoraLocalPadrao(): string {
  const agora = new Date();
  agora.setMinutes(agora.getMinutes() - agora.getTimezoneOffset());
  return agora.toISOString().slice(0, 16);
}

export function PaginaVendas() {
  const { sessao, adicionarToast } = usarSessao();
  const localizacao = useLocation();
  const [searchParams] = useSearchParams();

  const [pecas, setPecas] = useState<Peca[]>([]);
  const [vendas, setVendas] = useState<Venda[]>([]);
  const [carregando, setCarregando] = useState(true);

  const [pecaId, setPecaId] = useState<string>(searchParams.get('pecaId') || '');
  const [dataVenda, setDataVenda] = useState<string>(dataHoraLocalPadrao);
  const [formaPagamento, setFormaPagamento] = useState<string>('PIX');
  const [observacoes, setObservacoes] = useState<string>('');

  const [enviando, setEnviando] = useState(false);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [errosValidacao, setErrosValidacao] = useState<string[]>([]);

  const [vendaParaCancelar, setVendaParaCancelar] = useState<Venda | null>(null);
  const dialogRef = useRef<HTMLDialogElement | null>(null);
  const botaoSeguroRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    document.title = 'Minhas Compras · Garimpo Vintage';
  }, []);

  useEffect(() => {
    if (!sessao) return;
    let ativo = true;
    setCarregando(true);

    Promise.all([
      requisicaoApi<ListaPaginada<Peca>>('/api/pecas?pagina=1&tamanho=50', {}, sessao.token),
      requisicaoApi<Venda[]>('/api/vendas', {}, sessao.token),
    ])
      .then(([resPecas, resVendas]) => {
        if (!ativo) return;
        setPecas(resPecas.itens);
        setVendas(resVendas);
        if (!pecaId && resPecas.itens.length > 0) {
          const primeiraDisponivel = resPecas.itens.find((p) => !p.vendida) || resPecas.itens[0];
          setPecaId(String(primeiraDisponivel.id));
        }
        setCarregando(false);
      })
      .catch((err: unknown) => {
        if (!ativo) return;
        setErroApi(err instanceof Error ? err.message : 'Erro ao carregar dados.');
        setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [sessao, pecaId]);

  if (!sessao) {
    return <Navigate to="/login" state={{ de: localizacao.pathname + localizacao.search }} replace />;
  }

  async function recarregarDados() {
    if (!sessao) return;
    const [resPecas, resVendas] = await Promise.all([
      requisicaoApi<ListaPaginada<Peca>>('/api/pecas?pagina=1&tamanho=50', {}, sessao.token),
      requisicaoApi<Venda[]>('/api/vendas', {}, sessao.token),
    ]);
    setPecas(resPecas.itens);
    setVendas(resVendas);
  }

  async function lidarCriarVenda(e: FormEvent) {
    e.preventDefault();
    if (!sessao) return;

    setErroApi(null);
    setErrosValidacao([]);
    setEnviando(true);

    try {
      const isoDate = new Date(dataVenda).toISOString();
      await requisicaoApi<Venda>(
        '/api/vendas',
        {
          method: 'POST',
          body: JSON.stringify({
            pecaId: Number(pecaId),
            dataVenda: isoDate,
            formaPagamento,
            observacoes: observacoes.trim() || null,
          }),
        },
        sessao.token
      );

      adicionarToast('sucesso', 'Compra registrada com sucesso!');
      setObservacoes('');
      await recarregarDados();
    } catch (err: unknown) {
      if (err instanceof ErroApi) {
        setErroApi(`Erro ${err.status}: ${err.message}`);
        setErrosValidacao(err.errosValidacao);
        adicionarToast('erro', err.message);
      } else {
        setErroApi('Erro inesperado ao registrar a compra.');
      }
    } finally {
      setEnviando(false);
    }
  }

  function abrirModalCancelamento(venda: Venda) {
    setVendaParaCancelar(venda);
    if (dialogRef.current) {
      dialogRef.current.showModal();
      window.setTimeout(() => {
        botaoSeguroRef.current?.focus();
      }, 20);
    }
  }

  function fecharModal() {
    dialogRef.current?.close();
    setVendaParaCancelar(null);
  }

  async function confirmarCancelamento() {
    if (!sessao || !vendaParaCancelar) return;
    try {
      await requisicaoApi<void>(
        `/api/vendas/${vendaParaCancelar.id}`,
        { method: 'DELETE' },
        sessao.token
      );
      adicionarToast('sucesso', 'Compra cancelada e peça liberada novamente no acervo!');
      fecharModal();
      await recarregarDados();
    } catch (err: unknown) {
      fecharModal();
      if (err instanceof ErroApi) {
        setErroApi(`Erro ${err.status}: ${err.message}`);
        adicionarToast('erro', err.message);
      }
    }
  }

  return (
    <div className="pagina-grid-dupla">
      <section className="cartao-formulario">
        <h1>Registrar compra de peça</h1>
        <p className="subtitulo-formulario">
          Logado como <strong>{sessao.nome}</strong> ({sessao.papel}). Escolha uma peça do acervo. Se tentar comprar uma peça já vendida, a API devolverá <strong>409 Conflict</strong>.
        </p>

        {erroApi && (
          <div className="alerta alerta-erro" role="alert">
            <strong>Resposta da API:</strong> {erroApi}
            {errosValidacao.length > 1 && (
              <ul>
                {errosValidacao.map((msg, idx) => (
                  <li key={idx}>{msg}</li>
                ))}
              </ul>
            )}
          </div>
        )}

        <form onSubmit={lidarCriarVenda} className="formulario">
          <div className="grupo-campo">
            <label htmlFor="venda-peca">Peça do brechó</label>
            <select
              id="venda-peca"
              value={pecaId}
              onChange={(e) => setPecaId(e.target.value)}
              required
              className="input-campo"
            >
              <option value="">Selecione uma peça...</option>
              {pecas.map((p) => (
                <option key={p.id} value={p.id}>
                  #{p.id} — {p.nome} ({p.tamanho}) · R$ {p.preco.toFixed(2)}{' '}
                  {p.vendida ? '[JÁ VENDIDA - TESTAR 409]' : '[DISPONÍVEL]'}
                </option>
              ))}
            </select>
          </div>

          <div className="grupo-campo">
            <label htmlFor="venda-data">Data e horário da compra/retirada</label>
            <input
              id="venda-data"
              type="datetime-local"
              required
              value={dataVenda}
              onChange={(e) => setDataVenda(e.target.value)}
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="venda-pagamento">Forma de pagamento</label>
            <input
              id="venda-pagamento"
              type="text"
              required
              value={formaPagamento}
              onChange={(e) => setFormaPagamento(e.target.value)}
              placeholder="PIX, Cartão de Crédito, Dinheiro..."
              className="input-campo"
            />
          </div>

          <div className="grupo-campo">
            <label htmlFor="venda-obs">Observações de entrega/retirada (opcional)</label>
            <textarea
              id="venda-obs"
              rows={2}
              value={observacoes}
              onChange={(e) => setObservacoes(e.target.value)}
              placeholder="Ex: Retirar no sábado pela manhã"
              className="input-campo"
            />
          </div>

          <button type="submit" disabled={enviando} className="botao botao-primario botao-bloco">
            {enviando ? 'Processando compra...' : 'Confirmar Compra'}
          </button>
        </form>
      </section>

      <section className="secao-lista-vendas">
        <h2>{sessao.papel === 'admin' ? 'Todas as vendas registradas' : 'Minhas compras realizadas'}</h2>

        {carregando ? (
          <div className="cartao-formulario">
            <div className="esqueleto-linha esqueleto-titulo" />
            <div className="esqueleto-linha esqueleto-texto" />
          </div>
        ) : vendas.length === 0 ? (
          <div className="estado-vazio">
            <p>Nenhuma compra registrada até o momento.</p>
          </div>
        ) : (
          <div className="lista-cartoes-venda">
            {vendas.map((v) => (
              <article key={v.id} className="card-peca">
                <div className="card-peca-cabecalho">
                  <span className="categoria-tag">Pedido #{v.id}</span>
                  <span className="etiqueta-situacao etiqueta-disponivel">{v.situacao}</span>
                </div>
                <h3 className="card-titulo">{v.nomePeca}</h3>
                <p className="card-descricao">
                  Tamanho: <strong>{v.tamanhoPeca}</strong> · Pagamento: <strong>{v.formaPagamento}</strong>
                </p>
                <p className="card-descricao">
                  Cliente: <strong>{v.nomeCliente}</strong> · Data:{' '}
                  {new Date(v.dataVenda).toLocaleString('pt-BR')}
                </p>
                {v.observacoes && <p className="card-descricao">Obs: {v.observacoes}</p>}
                <div className="card-rodape">
                  <span className="card-preco">
                    {v.precoPeca.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
                  <button
                    type="button"
                    onClick={() => abrirModalCancelamento(v)}
                    className="botao botao-perigo botao-compacto"
                  >
                    Cancelar compra
                  </button>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>

      <dialog ref={dialogRef} className="modal-confirmacao">
        <h3>Confirmar cancelamento</h3>
        <p>
          Deseja realmente cancelar a compra da peça{' '}
          <strong>{vendaParaCancelar?.nomePeca}</strong>? A peça voltará a ficar disponível na vitrine.
        </p>
        <div className="modal-acoes">
          <button
            ref={botaoSeguroRef}
            type="button"
            onClick={fecharModal}
            className="botao botao-contorno"
          >
            Manter compra
          </button>
          <button
            type="button"
            onClick={confirmarCancelamento}
            className="botao botao-perigo"
          >
            Sim, cancelar
          </button>
        </div>
      </dialog>
    </div>
  );
}
