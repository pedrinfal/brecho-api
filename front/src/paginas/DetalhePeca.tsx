import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router';
import { ErroApi, requisicaoApi, type Peca } from '../api';

export function PaginaDetalhePeca() {
  const { id } = useParams();
  const [peca, setPeca] = useState<Peca | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [naoEncontrado, setNaoEncontrado] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    document.title = 'Detalhe da Peça · Garimpo Vintage';
    setCarregando(true);
    setNaoEncontrado(false);
    setErro(null);

    requisicaoApi<Peca>(`/api/pecas/${id}`)
      .then((res) => {
        setPeca(res);
        document.title = `${res.nome} · Garimpo Vintage`;
        setCarregando(false);
      })
      .catch((err: unknown) => {
        if (err instanceof ErroApi && err.status === 404) {
          setNaoEncontrado(true);
        } else {
          setErro(err instanceof Error ? err.message : 'Erro ao carregar detalhes da peça.');
        }
        setCarregando(false);
      });
  }, [id]);

  if (carregando) {
    return (
      <div className="cartao-formulario">
        <div className="esqueleto-linha esqueleto-titulo" />
        <div className="esqueleto-linha esqueleto-texto" />
        <div className="esqueleto-linha esqueleto-texto-curto" />
      </div>
    );
  }

  if (naoEncontrado || !peca) {
    return (
      <div className="estado-vazio">
        <h1>Peça não encontrada no acervo (404)</h1>
        <p>{erro || 'Esta peça não existe ou foi removida do catálogo.'}</p>
        <Link to="/" className="botao botao-primario">
          ← Voltar para a vitrine
        </Link>
      </div>
    );
  }

  return (
    <article className="cartao-detalhe">
      <div className="detalhe-topo">
        <span className="categoria-tag">{peca.categoria}</span>
        <span className={`etiqueta-situacao ${peca.vendida ? 'etiqueta-vendida' : 'etiqueta-disponivel'}`}>
          {peca.vendida ? 'Indisponível (Peça já vendida)' : 'Peça Única Disponível'}
        </span>
      </div>

      <h1>{peca.nome}</h1>
      <p className="detalhe-descricao">{peca.descricao}</p>

      <dl className="detalhe-grade">
        <div>
          <dt>Tamanho</dt>
          <dd>{peca.tamanho}</dd>
        </div>
        <div>
          <dt>Condição</dt>
          <dd>{peca.condicao}</dd>
        </div>
        <div>
          <dt>Preço</dt>
          <dd className="detalhe-preco">
            {peca.preco.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
          </dd>
        </div>
        <div>
          <dt>Cadastrada em</dt>
          <dd>{new Date(peca.criadaEm).toLocaleDateString('pt-BR')}</dd>
        </div>
      </dl>

      <div className="detalhe-acoes">
        <Link to="/" className="botao botao-contorno">
          ← Voltar ao acervo
        </Link>
        <Link to={`/vendas?pecaId=${peca.id}`} className="botao botao-primario">
          {peca.vendida ? 'Testar compra (ver regra 409)' : 'Comprar esta peça'}
        </Link>
      </div>
    </article>
  );
}
